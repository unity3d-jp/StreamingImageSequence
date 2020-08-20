using System;
using System.IO;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor.Timeline;
using UnityEditor;

#endif

namespace UnityEngine.StreamingImageSequence {

    /// <summary>
    /// The PlayableAsset of the TimelineClip to be used inside the Timeline Window.
    /// Implements the following interfaces:
    /// - ITimelineClipAsset: for defining clip capabilities (ClipCaps) 
    /// - IPlayableBehaviour: for displaying the curves in the timeline window
    /// - ISerializationCallbackReceiver: for serialization
    /// - IObserver(string): to receive updates when the contents of a folder are changed
    /// - ISerializationCallbackReceiver: to perform version upgrade, if necessary
    /// </summary>
    [System.Serializable]
    internal class StreamingImageSequencePlayableAsset : ImageFolderPlayableAsset, ITimelineClipAsset
                                                     , IPlayableBehaviour, IObserver<string>, ISerializationCallbackReceiver
    {      
        
//----------------------------------------------------------------------------------------------------------------------
#region IPlayableBehaviour interfaces
        /// <inheritdoc/>
        public void OnBehaviourPause(Playable playable, FrameData info){

        }
        
        /// <inheritdoc/>
        public void OnBehaviourPlay(Playable playable, FrameData info){

        }
        
        
        /// <inheritdoc/>
        public void OnGraphStart(Playable playable) {
            
#if UNITY_EDITOR
            string folder = GetFolder();

            //Check folder MD5
            if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder)) {
                string curFolderMD5 = PathUtility.CalculateFolderMD5ByFileSize(folder, m_imageFilePatterns, FileNameComparer);
                if (curFolderMD5 != m_folderMD5) {
                    Reload(curFolderMD5);                    
                }
            }
            
            FolderContentsChangedNotifier.GetInstance().Subscribe(this);
#endif            
        }
        
        
        /// <inheritdoc/>
        public void OnGraphStop(Playable playable){
#if UNITY_EDITOR
            FolderContentsChangedNotifier.GetInstance().Unsubscribe(this);
#endif            
        }
        
        /// <inheritdoc/>
        public void OnPlayableCreate(Playable playable){

        }
        /// <inheritdoc/>
        public void OnPlayableDestroy(Playable playable){
            //Destroy hidden resources
            ResetTexture();
        }

        /// <inheritdoc/>
        public void PrepareFrame(Playable playable, FrameData info){

        }

        /// <inheritdoc/>
        public void ProcessFrame(Playable playable, FrameData info, object playerData) {
        }

#endregion

//----------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Constructor
        /// </summary>
        public StreamingImageSequencePlayableAsset() {
            m_lastCopiedImageIndex = -1;            
        }
        
//----------------------------------------------------------------------------------------------------------------------

        //[Note-sin: 2020-7-17] This is also called when the TimelineClip in TimelineWindow is deleted, instead of just
        //The TimelineClipAsset (on file, for example) is deleted
        protected override void OnDestroy() {
            base.OnDestroy();

            Reset();
        }
        
//----------------------------------------------------------------------------------------------------------------------

        //Calculate the used image index for the passed globalTime
        internal int GlobalTimeToImageIndex(TimelineClip clip, double globalTime) {
            double localTime = clip.ToLocalTime(globalTime);
            return LocalTimeToImageIndex(clip, localTime);
        }

//----------------------------------------------------------------------------------------------------------------------

        //Calculate the used image index for the passed localTime
        internal int LocalTimeToImageIndex(TimelineClip clip, double localTime) {

            TimelineClipSISData timelineSISData = GetBoundTimelineClipSISData();

            if (null != timelineSISData) {
                double scaledTimePerFrame = TimelineUtility.CalculateTimePerFrame(clip) * clip.timeScale;            
          
                //Try to check if this frame is "dropped", so that we should use the image in the prev frame
                int              playableFrameIndex = Mathf.RoundToInt((float) localTime / (float)scaledTimePerFrame);
                SISPlayableFrame playableFrame      = timelineSISData.GetPlayableFrame(playableFrameIndex);
                while (playableFrameIndex > 0 && !playableFrame.IsUsed()) {
                    --playableFrameIndex;
                    playableFrame = timelineSISData.GetPlayableFrame(playableFrameIndex);
                    localTime     = playableFrameIndex * scaledTimePerFrame;
                }                
            }


            double imageSequenceTime = LocalTimeToCurveTime(clip, localTime);
            int count = GetImageFileNames().Count;
            
            int index = Mathf.RoundToInt(count * (float) imageSequenceTime);
            index = Mathf.Clamp(index, 0, count - 1);
            return index;
        }

//----------------------------------------------------------------------------------------------------------------------
        private static double LocalTimeToCurveTime(TimelineClip clip, double localTime) {
            AnimationCurve curve = GetAndValidateAnimationCurve(clip);                       
            return curve.Evaluate((float)(localTime));
        }
        
//----------------------------------------------------------------------------------------------------------------------


        /// <summary>
        /// Returns the texture that contains the active image according to the PlayableDirector's time.
        /// </summary>
        /// <returns></returns>
        public Texture2D GetTexture() { return m_texture; }        
                        
        
//----------------------------------------------------------------------------------------------------------------------        
        private void Reset() {
            m_primaryImageIndex         = 0;
            m_forwardPreloadImageIndex  = 0;
            m_backwardPreloadImageIndex = 0;
            
            m_lastCopiedImageIndex = -1;
            ResetTexture();
            ResetResolution();
        }

//----------------------------------------------------------------------------------------------------------------------        
        
        /// <inheritdoc/>
        public ClipCaps clipCaps {
#if AT_USE_TIMELINE_GE_1_4_0            
            get { return ClipCaps.ClipIn | ClipCaps.AutoScale; }
#else            
            get { return ClipCaps.ClipIn | ClipCaps.SpeedMultiplier; }
#endif            
        }
                
//---------------------------------------------------------------------------------------------------------------------

#region PlayableAsset functions override
        /// <inheritdoc/>
        public sealed override Playable CreatePlayable(PlayableGraph graph, GameObject go) {
            return Playable.Null;
        }
       
#endregion    
        
       
//---------------------------------------------------------------------------------------------------------------------

        internal void ContinuePreloadingImages() {

            IList<string> imageFileNames = GetImageFileNames();
            if (null == imageFileNames || 0== imageFileNames.Count)
                return;

            const int NUM_IMAGES = 2;

            //forward
            int maxForwardPreloadIndex = Mathf.Min(m_forwardPreloadImageIndex + NUM_IMAGES, imageFileNames.Count) -1;
            int startForwardPreloadIndex = m_forwardPreloadImageIndex;
            for (int i = startForwardPreloadIndex; i <= maxForwardPreloadIndex; ++i) {
                if (QueueImageLoadTask(i, out _)) {
                    ++m_forwardPreloadImageIndex;                    
                } else {
                    break;
                }
            }
            
            //backward
            int minBackwardPreloadIndex = Mathf.Max((m_backwardPreloadImageIndex - NUM_IMAGES)+1, 0);
            int startBackwardPreloadIndex = m_backwardPreloadImageIndex;
            for (int i = startBackwardPreloadIndex; i >=minBackwardPreloadIndex; --i) {
                if (QueueImageLoadTask(i, out _)) {
                    --m_backwardPreloadImageIndex;                    
                } else {
                    break;
                }
            }
            
        }


//----------------------------------------------------------------------------------------------------------------------        
        //return true if we should continue preloading the next image. False otherwise
        private bool QueueImageLoadTask(int index, out ImageData imageData) {
            const int TEX_TYPE = StreamingImageSequenceConstants.IMAGE_TYPE_FULL;
            string fullPath = GetImageFilePath(index);

            if (!File.Exists(fullPath)) {
                imageData = new ImageData(StreamingImageSequenceConstants.READ_STATUS_FAIL);                
                return true;
            }

            ImageLoader.GetImageDataInto(fullPath,TEX_TYPE,out imageData);
            //Debug.Log("imageData.readStatus " + imageData.readStatus + "Loading " + filename);
            
            switch (imageData.ReadStatus) {
                case StreamingImageSequenceConstants.READ_STATUS_LOADING: 
                case StreamingImageSequenceConstants.READ_STATUS_SUCCESS: {
                    break;
                }
                default: {
                    return ImageLoader.RequestLoadFullImage(fullPath);
                
                }
            }
                       
            return true;
        }
//----------------------------------------------------------------------------------------------------------------------        
        

        internal bool RequestLoadImage(int index) {
            IList<string> imageFileNames = GetImageFileNames();
            if (null == imageFileNames || index < 0 || index >= imageFileNames.Count || string.IsNullOrEmpty(imageFileNames[index])) {
                return false;
            }

            m_primaryImageIndex         = index;

            if (QueueImageLoadTask(index, out ImageData readResult)) {
                m_forwardPreloadImageIndex  = Mathf.Min(m_primaryImageIndex + 1, imageFileNames.Count - 1);
                m_backwardPreloadImageIndex = Mathf.Max(m_primaryImageIndex - 1, 0);                
            } else {
                //If we can't queue, try from the primary index again
                m_forwardPreloadImageIndex = m_backwardPreloadImageIndex = index;
            }

            if (null == m_texture &&  readResult.ReadStatus == StreamingImageSequenceConstants.READ_STATUS_SUCCESS) {

                ResetTexture();
                m_texture = readResult.CreateCompatibleTexture(HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor);
                m_texture.name = "Full: " + imageFileNames[index];
                readResult.CopyBufferToTexture(m_texture);
                
                UpdateResolution(ref readResult);
            }

            //Update the texture
            if (readResult.ReadStatus == StreamingImageSequenceConstants.READ_STATUS_SUCCESS && m_lastCopiedImageIndex != index) {

                readResult.CopyBufferToTexture(m_texture);
                m_lastCopiedImageIndex = index;
            }

            return null!=m_texture;
        }        

//---------------------------------------------------------------------------------------------------------------------
        void ResetTexture() {
            if (null != m_texture) {
                ObjectUtility.Destroy(m_texture);
                m_texture = null;
            }

        }

//----------------------------------------------------------------------------------------------------------------------
        //Make sure to set the curve of the TimelineClip 
        internal void InitTimelineClipCurve(TimelineClip clip) {
            Assert.IsNotNull(clip);            
            AnimationCurve curve = GetAndValidateAnimationCurve(clip);
            SetTimelineClipCurve(clip, curve);            
        }
        
        internal static void ResetTimelineClipCurve(TimelineClip clip) {
            
            Assert.IsNotNull(clip);
            AnimationCurve animationCurve = new AnimationCurve();
            ValidateAnimationCurve(ref animationCurve, (float) (clip.duration * clip.timeScale));
            SetTimelineClipCurve(clip, animationCurve);
            clip.clipIn = 0;
        }

//----------------------------------------------------------------------------------------------------------------------
        //Get the animation curve from the TimelineClip.  
        private static AnimationCurve GetAndValidateAnimationCurve(TimelineClip clip) {
            AnimationCurve animationCurve = null;
            
            //[TODO-sin: 2020-7-30] Support getting animation curve in Runtime
#if UNITY_EDITOR
            animationCurve = AnimationUtility.GetEditorCurve(clip.curves, m_timelineEditorCurveBinding);
#endif
            if (null == animationCurve)
                animationCurve = new AnimationCurve();
            
            ValidateAnimationCurve(ref animationCurve, (float) clip.duration);
            return animationCurve;
        }

//----------------------------------------------------------------------------------------------------------------------
        //Validate: make sure we have at least two keys
        internal  static void ValidateAnimationCurve(ref AnimationCurve animationCurve, float clipDuration) {
            int numKeys = animationCurve.keys.Length;
            switch (numKeys) {
                case 0: {
                    animationCurve = AnimationCurve.Linear(0, 0, clipDuration,1 );
                    break;
                }
                case 1: {
                    animationCurve.keys[0] = new Keyframe(0.0f,0.0f);
                    animationCurve.AddKey(clipDuration, 1.0f);
                    break;
                }
                default: break;
            }
        }
        
//----------------------------------------------------------------------------------------------------------------------

        private static void SetTimelineClipCurve(TimelineClip clip, AnimationCurve curve) {
            clip.curves.SetCurve("", typeof(StreamingImageSequencePlayableAsset), "m_time", curve);
#if UNITY_EDITOR            
            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved );
#endif            
        }

//----------------------------------------------------------------------------------------------------------------------        
#region Observer
        
        public void OnCompleted() {
        }

        public void OnError(Exception e) {
            Debug.LogError($"StreamingImageSequencePlayableAsset::OnError(): {e.ToString()}");
        }

        public void OnNext(string updatedFolder) {
#if UNITY_EDITOR
            if (updatedFolder != GetFolder())
                return;
            
            Reload();
#endif
        }

#endregion Observer
        
//----------------------------------------------------------------------------------------------------------------------        

#region ISerializationCallbackReceiver

        public void OnBeforeSerialize() {
            
        }

        public void OnAfterDeserialize() {
            if (m_version < (int) SISPlayableAssetVersion.FOLDER_MD5) {
                string folder = GetFolder();
                
                if (!string.IsNullOrEmpty(folder)) {
                    m_folderMD5 = PathUtility.CalculateFolderMD5ByFileSize(folder, m_imageFilePatterns, FileNameComparer);                
                }                
            }
            m_version = CUR_SIS_PLAYABLE_ASSET_VERSION;
        }
        
#endregion ISerializationCallbackReceiver
        
//----------------------------------------------------------------------------------------------------------------------        

#region Unity Editor code

#if UNITY_EDITOR         
        internal void InitFolder(StreamingImageSequencePlayableAssetParam param) {
            string folder = param.Folder;
            SetFolder(folder );
            SetImageFileNames(param.Pictures);
            UpdateResolution(param.Resolution);
            
            if (null!=folder && folder.StartsWith("Assets")) {
                m_timelineDefaultAsset = AssetDatabase.LoadAssetAtPath<UnityEditor.DefaultAsset>(folder);
            } else {
                m_timelineDefaultAsset = null;
            }
            m_texture = null;
            EditorUtility.SetDirty(this);
            m_folderMD5 = PathUtility.CalculateFolderMD5ByFileSize(folder, m_imageFilePatterns, FileNameComparer);
        }

        internal void Reload(string folderMD5 = null) {
            string folder = GetFolder();
            
            if (string.IsNullOrEmpty(folder))
                return;
            
            //Unload existing images
            if (HasImages()) {
                IList<string> prevImageFileNames = GetImageFileNames();
                foreach (string fileName in prevImageFileNames) {
                    string imagePath = PathUtility.GetPath(folder, fileName);
                    StreamingImageSequencePlugin.UnloadImageAndNotify(imagePath);
                }
            }            
            
            SetImageFileNames(FindImages(folder));
            Reset();
            EditorUtility.SetDirty(this);
            if (!string.IsNullOrEmpty(folderMD5)) {
                m_folderMD5 = folderMD5;
            }
            else {
                m_folderMD5 = PathUtility.CalculateFolderMD5ByFileSize(folder, m_imageFilePatterns, FileNameComparer);                
            }
            

        }
        
        internal UnityEditor.DefaultAsset GetTimelineDefaultAsset() { return m_timelineDefaultAsset; }

        //Return FileNames
        internal static List<string> FindImages(string path) {
            Assert.IsFalse(string.IsNullOrEmpty(path));
            Assert.IsTrue(Directory.Exists(path));

            //Convert path to folder here
            string fullSrcPath    = Path.GetFullPath(path).Replace("\\", "/");

            //Enumerate all files with the supported extensions and sort
            List<string> fileNames = new List<string>();
            foreach (string pattern in m_imageFilePatterns) {
                IEnumerable<string> files = Directory.EnumerateFiles(fullSrcPath, pattern, SearchOption.TopDirectoryOnly);
                foreach (string filePath in files) {                    
                    fileNames.Add(Path.GetFileName(filePath));
                }
            }
            fileNames.Sort(FileNameComparer);
            return fileNames;
        }
        
        
        private static int FileNameComparer(string x, string y) {
            return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase);
        }
#endif        
        
#endregion
        
//----------------------------------------------------------------------------------------------------------------------

        [HideInInspector][SerializeField] private string m_folderMD5 = null;
        
        
        [HideInInspector][SerializeField] private int m_version = CUR_SIS_PLAYABLE_ASSET_VERSION;        
        [SerializeField] double m_time;
        

#if UNITY_EDITOR
        [SerializeField] private UnityEditor.DefaultAsset m_timelineDefaultAsset = null; //Folder D&D. See notes below
        private static EditorCurveBinding m_timelineEditorCurveBinding =  
            new EditorCurveBinding() {
                path         = "",
                type         = typeof(StreamingImageSequencePlayableAsset),
                propertyName = "m_time"
            };
        
#endif
       
        
        private int m_lastCopiedImageIndex; //the index of the image copied to m_texture

        private int m_primaryImageIndex         = 0;
        private int m_forwardPreloadImageIndex  = 0;
        private int m_backwardPreloadImageIndex = 0;
        
        private bool m_verified;

        Texture2D m_texture = null;

//----------------------------------------------------------------------------------------------------------------------
        
        private const int CUR_SIS_PLAYABLE_ASSET_VERSION = (int) SISPlayableAssetVersion.FOLDER_MD5;
                
        static readonly string[] m_imageFilePatterns = {
            "*.png",
            "*.tga"             
        };        

    }

//----------------------------------------------------------------------------------------------------------------------

enum SISPlayableAssetVersion {
    INITIAL = 1, //initial
    FOLDER_MD5 = 2, //added folder MD5 hash
    
}


} //end namespace

//----------------------------------------------------------------------------------------------------------------------
//[Note-Sin: 2019-12-23] We need two things, in order to enable folder drag/drop to the timeline Window
//1. Derive this class from PlayableAsset
//2. Declare UnityEditor.DefaultAsset variable 

