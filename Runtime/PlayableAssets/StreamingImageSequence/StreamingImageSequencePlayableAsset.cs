using System;
using System.IO;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
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
    /// </summary>
    [System.Serializable]
    public class StreamingImageSequencePlayableAsset : PlayableAsset, ITimelineClipAsset
                                                     , IPlayableBehaviour, ISerializationCallbackReceiver 
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
            float fps = m_timelineClip.parentTrack.timelineAsset.editorSettings.fps;
            m_timePerFrame = m_timelineClip.timeScale / fps;

            if (null == m_playableFrames) {
                ResetPlayableFrames();
            }

            //Change the size of m_playableFrames and reinitialize if necessary
            int numIdealNumPlayableFrames = CalculateIdealNumPlayableFrames();
            int prevNumPlayableFrames = m_playableFrames.Count;
            if (numIdealNumPlayableFrames != prevNumPlayableFrames) {
#if UNITY_EDITOR
                Undo.RegisterCompleteObjectUndo(this, "StreamingImageSequencePlayableAsset: Updating PlayableFrame List");
#endif                
                //Change the size of m_playableFrames and reinitialize if necessary
                List<bool> prevUsedFrames = new List<bool>(prevNumPlayableFrames);
                foreach (PlayableFrame frame in m_playableFrames) {
                    prevUsedFrames.Add(frame.IsUsed());
                }
                
                UpdatePlayableFramesSize(numIdealNumPlayableFrames);
                
                //Reinitialize 
                if (prevNumPlayableFrames > 0) {
                    for (int i = 0; i < numIdealNumPlayableFrames; ++i) {
                        int prevIndex = (int)(((float)(i) / numIdealNumPlayableFrames) * prevNumPlayableFrames);
                        m_playableFrames[i].SetUsed(prevUsedFrames[prevIndex]);
                    }
                }
                
            }
            
            //Refresh all markers
            foreach (PlayableFrame playableFrame in m_playableFrames) {
                playableFrame.Refresh(m_useImageMarkerVisibility);
            }
            
        }
        
        /// <inheritdoc/>
        public void OnGraphStop(Playable playable){

        }
        /// <inheritdoc/>
        public void OnPlayableCreate(Playable playable){

        }
        /// <inheritdoc/>
        public void OnPlayableDestroy(Playable playable){

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
            m_loadingIndex = -1;
            m_lastIndex = -1;
#if UNITY_EDITOR            
            m_timelineEditorCurveBinding  = new EditorCurveBinding() {
                path = "",
                type = typeof(StreamingImageSequencePlayableAsset),
                propertyName = "m_time"
            };
#endif        
            
        }

//----------------------------------------------------------------------------------------------------------------------

        /// <inheritdoc cref="PlayableAsset" />
        private void OnDestroy() {

            Reset();
            
            
            if (null == m_playableFrames)
                return;
            
            foreach (PlayableFrame frame in m_playableFrames) {
                ObjectUtility.Destroy(frame);
            }
        }

//----------------------------------------------------------------------------------------------------------------------
        /// <summary>
        /// Get the source folder
        /// </summary>
        /// <returns>The folder where the images are located</returns>
        public string GetFolder() { return m_folder; }
        
//----------------------------------------------------------------------------------------------------------------------
        private double LocalTimeToCurveTime(double localTime) {
            AnimationCurve curve = GetAndValidateAnimationCurve();
            return curve.Evaluate((float)localTime);
        }
//----------------------------------------------------------------------------------------------------------------------

        //Calculate the used image index for the passed globalTime
        internal int GlobalTimeToImageIndex(double globalTime) {
            double localTime = m_timelineClip.ToLocalTime(globalTime);
            return LocalTimeToImageIndex(localTime);
        }

//----------------------------------------------------------------------------------------------------------------------

        //Calculate the used image index for the passed localTime
        internal int LocalTimeToImageIndex(double localTime) {
            //Try to check if this frame is "dropped", so that we should use the image in the prev frame
            int frameIndex = (int) (localTime / m_timePerFrame);
            if (frameIndex >= 0 && null!=m_playableFrames && frameIndex < m_playableFrames.Count) {
                while (!m_playableFrames[frameIndex].IsUsed() && frameIndex > 0) {
                    --frameIndex;
                    localTime = frameIndex * m_timePerFrame;
                }
            }

            double imageSequenceTime = LocalTimeToCurveTime(localTime);
            int count = m_imagePaths.Count;
            int index = (int)(count * imageSequenceTime);
            index = Mathf.Clamp(index, 0, count - 1);
            return index;
        }

//----------------------------------------------------------------------------------------------------------------------

        internal int GetVersion() { return m_version; }
        internal IList<string> GetImagePaths() { return m_imagePaths; }
        internal ImageDimensionInt GetResolution() { return m_resolution; }
        internal System.Collections.IList GetImagePathsNonGeneric() { return m_imagePaths; }
        internal Texture2D GetTexture() { return m_texture; }
        internal TimelineClip GetTimelineClip() { return m_timelineClip; }

        //This method must only be called from the track that owns this PlayableAsset, or during deserialization
        internal void SetTimelineClip(TimelineClip clip) {  m_timelineClip = clip; }

        internal bool GetUseImageMarkerVisibility() {  return m_useImageMarkerVisibility; }

        internal void SetUseImageMarkerVisibility(bool show) { m_useImageMarkerVisibility = show; }

//----------------------------------------------------------------------------------------------------------------------        
        internal float GetOrUpdateDimensionRatio() {
            if (Mathf.Approximately(0.0f, m_dimensionRatio)) {
                ForceUpdateResolution();
            }
            return m_dimensionRatio;
        }

//----------------------------------------------------------------------------------------------------------------------        

        internal string GetImagePath(int index) {
            if (null == m_imagePaths || index >= m_imagePaths.Count)
                return null;

            return m_imagePaths[index];
        }

//----------------------------------------------------------------------------------------------------------------------        

        internal bool HasImages() {
            return (!string.IsNullOrEmpty(m_folder) && null != m_imagePaths && m_imagePaths.Count > 0);
        }
        
        
//----------------------------------------------------------------------------------------------------------------------        
        internal void Reset() {
            m_loadingIndex = -1;
            m_lastIndex = -1;
            m_loadRequested = null;
            if (null != m_texture) {
                ResetTexture();
            }

            m_resolution = new ImageDimensionInt();
        }
        
        
//----------------------------------------------------------------------------------------------------------------------        
        
        /// <inheritdoc/>
        public ClipCaps clipCaps {
#if TIMELINE_NEWER_THAN_1_4_0            
            get { return ClipCaps.ClipIn | ClipCaps.AutoScale; }
#else            
            get { return ClipCaps.ClipIn | ClipCaps.SpeedMultiplier; }
#endif            
        }
        
//----------------------------------------------------------------------------------------------------------------------        

        internal bool Verified
        {
            get
            {
                if (!m_verified)
                {
                    m_verified = !string.IsNullOrEmpty(m_folder) && 
                                 m_folder.StartsWith("Assets/StreamingAssets") &&
                                 Directory.Exists(m_folder) && 
                                 m_imagePaths != null && 
                                 m_imagePaths.Count > 0;
                }
                
                return m_verified;
            }
        }
        
//---------------------------------------------------------------------------------------------------------------------

#region PlayableAsset functions override
        /// <inheritdoc/>
        public sealed override Playable CreatePlayable(PlayableGraph graph, GameObject go) {
            //Dummy. We just need to implement this from PlayableAsset because folder D&D support. See notes below
            return Playable.Null;
        }
        
        /// <inheritdoc/>
        public sealed override double duration {  get {  return (null!=m_timelineClip) ? m_timelineClip.duration : 0;  }  }
       
#endregion         
//---------------------------------------------------------------------------------------------------------------------

        internal void LoadRequest(bool isDirectorIdle) {
            if (null == m_imagePaths)
                return;

            int numPictures = m_imagePaths.Count;
            if (m_loadRequested == null && numPictures > 0) {
                m_loadRequested = new bool[numPictures];
            }

            // request loading while editor is idle.
            if (isDirectorIdle)
            {
                LoadStep(4);
            }
            else
            {
                LoadStep(2);
            }
        }

        private void LoadStep(int step)
        {
            if (UpdateManager.IsPluginResetting())
            {
                return;
            }
            int loadRequestMax = m_loadingIndex + step;
            if (loadRequestMax > m_imagePaths.Count)
            {
                loadRequestMax = m_imagePaths.Count;
            }
            for (int ii = m_loadingIndex; ii <= loadRequestMax - 1; ii++)
            {
                if (ii == -1)
                {
                    continue;
                }

                LoadRequest(ii, false, out ReadResult readResult);

            }
            m_loadingIndex = loadRequestMax;

            
        }

        internal bool IsLoadRequested(int index)
        {
            string filename = m_imagePaths[index];
            filename = GetCompleteFilePath(filename);
            StreamingImageSequencePlugin.GetNativeTextureInfo(filename, out ReadResult readResult, 
                StreamingImageSequenceConstants.TEXTURE_TYPE_FULL
            );
            return (readResult.ReadStatus != 0);

        }

//----------------------------------------------------------------------------------------------------------------------        
        internal string LoadRequest(int index, bool isBlocking, out ReadResult readResult) {
            const int TEX_TYPE = StreamingImageSequenceConstants.TEXTURE_TYPE_FULL;
            string filename = m_imagePaths[index];
            filename = GetCompleteFilePath(filename);
            if (m_loadRequested == null) {
                m_loadRequested = new bool[m_imagePaths.Count];
            }

            StreamingImageSequencePlugin.GetNativeTextureInfo(filename, out readResult, TEX_TYPE);
            //Debug.Log("readResult.readStatus " + readResult.readStatus + "Loading " + filename);
            if (readResult.ReadStatus == StreamingImageSequenceConstants.READ_RESULT_NONE) {
                ImageLoadBGTask.Queue(filename);
            }
            if ( isBlocking ) {
                while (readResult.ReadStatus != StreamingImageSequenceConstants.READ_RESULT_SUCCESS) {
                    StreamingImageSequencePlugin.GetNativeTextureInfo(filename, out readResult, TEX_TYPE);
                }
            }
#if false //UNITY_EDITOR
            if ( readResult.readStatus == 1 )
            {
                Util.Log("Already requestd:" + filename);
            }
#endif
            return filename;
        }
//----------------------------------------------------------------------------------------------------------------------        
        

        internal bool RequestLoadImage(int index, bool isBlocking)
        {
            if (null == m_imagePaths || index < 0 || index >= m_imagePaths.Count || string.IsNullOrEmpty(m_imagePaths[index])) {
                return false;
            }
           
            string filename = LoadRequest(index,isBlocking, out ReadResult readResult);

            if (null == m_texture &&  readResult.ReadStatus == StreamingImageSequenceConstants.READ_RESULT_SUCCESS) {

                m_texture = StreamingImageSequencePlugin.CreateTexture(ref readResult);

                IntPtr ptr =  m_texture.GetNativeTexturePtr();
                int texInstanceID = m_texture.GetInstanceID();
                
                UpdateResolution(ref readResult);
                StreamingImageSequencePlugin.SetNativeTexturePtr(ptr, (uint)readResult.Width, (uint)readResult.Height, texInstanceID);
            }

            //Update the texture
			if (readResult.ReadStatus == StreamingImageSequenceConstants.READ_RESULT_SUCCESS && m_lastIndex != index) {
                int texInstanceID = m_texture.GetInstanceID();
                StreamingImageSequencePlugin.SetLoadedTexture (filename, texInstanceID);
                GL.IssuePluginEvent(StreamingImageSequencePlugin.GetRenderEventFunc(), texInstanceID);
			}

			m_lastIndex = index;
            return null!=m_texture;
        }
//----------------------------------------------------------------------------------------------------------------------        

        internal string GetCompleteFilePath(string filePath)
        {
            string strOverridePath = m_folder;

            if (!string.IsNullOrEmpty(strOverridePath)) {
                filePath = Path.Combine(strOverridePath, filePath).Replace("\\", "/");
            }

            if (Path.IsPathRooted(filePath)) {
                filePath = Path.Combine(UpdateManager.GetProjectFolder(), filePath).Replace("\\", "/");
            }
            return filePath;
        }
//----------------------------------------------------------------------------------------------------------------------

        int CalculateIdealNumPlayableFrames() {
            //Recalculate the number of frames and create the marker's ground truth data
            float fps = m_timelineClip.parentTrack.timelineAsset.editorSettings.fps;
            int numFrames = Mathf.RoundToInt((float)(m_timelineClip.duration * fps));
            return numFrames;
            
        }
//----------------------------------------------------------------------------------------------------------------------

        private void UpdatePlayableFramesSize(int playableFramesSize) {

            //Resize m_playableFrames
            while (m_playableFrames.Count < playableFramesSize) {
                PlayableFrame playableFrame = ObjectUtility.CreateScriptableObjectInstance<PlayableFrame>();
                m_playableFrames.Add(playableFrame);
#if UNITY_EDITOR                    
                AssetDatabase.AddObjectToAsset(playableFrame, this);
#endif                    
            }
            while (m_playableFrames.Count > playableFramesSize) {
                int index = m_playableFrames.Count - 1;
                PlayableFrame lastFrame = m_playableFrames[index];
                m_playableFrames.RemoveAt(index);
                ObjectUtility.Destroy(lastFrame);
            }
            
            for (int i = 0; i < playableFramesSize; ++i) {
                m_playableFrames[i].Init(this, m_timePerFrame * i);
            }
            
            
        }
        
//---------------------------------------------------------------------------------------------------------------------

        internal void ResetPlayableFrames() {
            // TrackAsset track = m_timelineClip.parentTrack;
            // List<UseImageMarker> markersToDelete = new List<UseImageMarker>();
            // foreach (IMarker m in track.GetMarkers()) {
            //     UseImageMarker marker = m as UseImageMarker;
            //     if (null == marker)
            //         continue;
            //     
            //     PlayableFrame owner = marker.GetOwner();
            //     if (null == owner || this == owner.GetPlayableAsset()) {
            //         markersToDelete.Add(marker);
            //     }
            //
            // }
            // //Delete all markers in the parent track that has the assigned PlayableAsset set to this object
            // foreach (UseImageMarker marker in markersToDelete) {
            //     track.DeleteMarker(marker);
            // }
            // markersToDelete.Clear();
#if UNITY_EDITOR
            Undo.RegisterCompleteObjectUndo(this, "StreamingImageSequencePlayableAsset: Resetting Use Image Markers");
#endif

            if (null != m_playableFrames) {
                foreach (PlayableFrame frame in m_playableFrames) {
                    if (!frame)
                        continue;
                    ObjectUtility.Destroy(frame); //This will remove from AssetDatabase in UnityEditor
                }
            }

            //Recalculate the number of frames and create the marker's ground truth data
            int numFrames = CalculateIdealNumPlayableFrames();
            m_playableFrames =  new List<PlayableFrame>(numFrames);
            UpdatePlayableFramesSize(numFrames);
            
            
#if UNITY_EDITOR //Add to AssetDatabase
            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved );
#endif            
           
        }

        
//---------------------------------------------------------------------------------------------------------------------
        void ResetTexture() {
            StreamingImageSequencePlugin.ResetLoadedTexture(m_texture.GetInstanceID());
            m_texture = null;
        }

//---------------------------------------------------------------------------------------------------------------------
        void UpdateResolution(ref ReadResult readResult) {
            m_resolution.Width  = readResult.Width;
            m_resolution.Height = readResult.Height;
            m_dimensionRatio = m_resolution.CalculateRatio();
        }
//---------------------------------------------------------------------------------------------------------------------
        

        void ForceUpdateResolution() {
            if (null!=m_imagePaths && m_imagePaths.Count <= 0)
                return;

            //Load the first image to update the resolution.
            LoadRequest(0, false, out ReadResult readResult);
            if (readResult.ReadStatus == StreamingImageSequenceConstants.READ_RESULT_SUCCESS) {               
                UpdateResolution(ref readResult);
            }
            
        }
//----------------------------------------------------------------------------------------------------------------------
        internal void OnAfterTrackDeserialize(TimelineClip clip) {
            SetTimelineClip(clip);
        }
        
//----------------------------------------------------------------------------------------------------------------------

#region ISerializationCallbackReceiver implementation
        /// <inheritdoc/>
        public void OnBeforeSerialize() {
        }

        /// <inheritdoc/>
        public void OnAfterDeserialize() {
            ForceUpdateResolution();
        }
#endregion
        

//----------------------------------------------------------------------------------------------------------------------
        internal void ResetAnimationCurve() {
            AnimationCurve animationCurve = new AnimationCurve();
            ValidateAnimationCurve(ref animationCurve);
            RefreshAnimationCurveInTimelineClip(animationCurve);
            m_timelineClip.clipIn = 0;
            m_timelineClip.timeScale = 1.0;
        }

//----------------------------------------------------------------------------------------------------------------------
        internal void ValidateAnimationCurve() {
            AnimationCurve curve = GetAndValidateAnimationCurve();
            RefreshAnimationCurveInTimelineClip(curve);
        }

//----------------------------------------------------------------------------------------------------------------------
        //Get the animation curve from the TimelineClip.  
        private AnimationCurve GetAndValidateAnimationCurve() {
            AnimationCurve animationCurve = null;
#if UNITY_EDITOR
            animationCurve = AnimationUtility.GetEditorCurve(m_timelineClip.curves, m_timelineEditorCurveBinding);
#endif
            if (null == animationCurve)
                animationCurve = new AnimationCurve();
            
            ValidateAnimationCurve(ref animationCurve);
            return animationCurve;
        }

//----------------------------------------------------------------------------------------------------------------------
        //Validate: make sure we have at least two keys
        private void ValidateAnimationCurve(ref AnimationCurve animationCurve) {
            int numKeys = animationCurve.keys.Length;
            switch (numKeys) {
                case 0: {
                    animationCurve = AnimationCurve.Linear(0, 0, (float) m_timelineClip.duration,1 );
                    break;
                }
                case 1: {
                    animationCurve.keys[0] = new Keyframe(0.0f,0.0f);
                    animationCurve.AddKey((float)m_timelineClip.duration, 1.0f);
                    break;
                }
                default: break;
            }
        }
        
//----------------------------------------------------------------------------------------------------------------------

        private void  RefreshAnimationCurveInTimelineClip(AnimationCurve curve) {
            m_timelineClip.curves.SetCurve("", typeof(StreamingImageSequencePlayableAsset), "m_time", curve);
#if UNITY_EDITOR            
            //[TODO-sin: 2019-12-25] Is there a way to make this smoother ?
            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved );
#endif            
        }


//----------------------------------------------------------------------------------------------------------------------        

#region Unity Editor code

#if UNITY_EDITOR         
        internal void SetParam(StreamingImageSequencePlayableAssetParam param) {
            if (m_resolution.Width > 0 && m_resolution.Height > 0) {
                m_resolution = param.Resolution;
                m_dimensionRatio = m_resolution.CalculateRatio();
            }
            m_imagePaths = param.Pictures;
            m_folder = param.Folder;
            if (null!=m_folder && m_folder.StartsWith("Assets")) {
                m_timelineDefaultAsset = AssetDatabase.LoadAssetAtPath<UnityEditor.DefaultAsset>(m_folder);
            } else {
                m_timelineDefaultAsset = null;
            }
            m_texture = null;
            EditorUtility.SetDirty(this);
        }
        
        internal UnityEditor.DefaultAsset GetTimelineDefaultAsset() { return m_timelineDefaultAsset; }
        
#endif        
        
#endregion
        
//----------------------------------------------------------------------------------------------------------------------

        [SerializeField] private string m_folder = null;
        [SerializeField] List<string> m_imagePaths = null;
        
        //The ground truth for using/dropping an image in a particular frame. See the notes below
        [SerializeField] List<PlayableFrame> m_playableFrames = null;

        [SerializeField] private int m_version = STREAMING_IMAGE_SEQUENCE_PLAYABLE_ASSET_VERSION;        
        [SerializeField] double m_time;

        private ImageDimensionInt  m_resolution;        
        private float m_dimensionRatio;



#if UNITY_EDITOR
        [SerializeField] private UnityEditor.DefaultAsset m_timelineDefaultAsset = null; //Folder D&D. See notes below
        private EditorCurveBinding m_timelineEditorCurveBinding;
#endif
        private bool[] m_loadRequested;
        [SerializeField] [HideInInspector] private bool m_useImageMarkerVisibility = true;
        
        //[Note-sin: 2020-2-13] TimelineClip has to be setup every time (after deserialization, etc) to ensure that
        //we are referring to the same instance rather than having a newly created one
        TimelineClip m_timelineClip  = null; 

        //[TODO-sin: 2020-1-30] Turn this to a non-public var
        [SerializeField] internal int m_loadingIndex;

        private int m_lastIndex;
        private bool m_verified;
        private double m_timePerFrame = 0;

        Texture2D m_texture = null;

        private const int STREAMING_IMAGE_SEQUENCE_PLAYABLE_ASSET_VERSION = 1;

    }
}

//---------------------------------------------------------------------------------------------------------------------
//[Note-Sin: 2019-12-23] We need two things, in order to enable folder drag/drop to the timeline Window
//1. Derive this class from PlayableAsset
//2. Declare UnityEditor.DefaultAsset variable 

//[Note-Sin: 2020-2-17] PlayableFrame
//StreamingImageSequencePlayableAsset owns PlayableFrame, which owns UseImageMarker.
//PlayableFrame is a ScriptableObject, which is stored inside StreamingImageSequencePlayableAsset using
//AssetDatabase.AddObjectToAsset

