﻿using System;
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
    /// </summary>
    [System.Serializable]
    public class StreamingImageSequencePlayableAsset : PlayableAsset, ITimelineClipAsset
                                                     , IPlayableBehaviour
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
            RefreshPlayableFrames();
        }
        
        /// <inheritdoc/>
        public void OnGraphStop(Playable playable){

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
        internal void OnClonedFrom(StreamingImageSequencePlayableAsset otherAsset) {
            m_clonedFromAsset = otherAsset; 
            
        }
        
//----------------------------------------------------------------------------------------------------------------------

        /// <inheritdoc cref="PlayableAsset" />
        private void OnDestroy() {

            Reset();
            
            DestroyPlayableFrames();
        }
//----------------------------------------------------------------------------------------------------------------------

        private void DestroyPlayableFrames() {
            if (null == m_playableFrames)
                return;
            
            foreach (PlayableFrame frame in m_playableFrames) {
                if (null == frame)
                    continue;
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
        /// <summary>
        /// Returns the texture that contains the active image according to the PlayableDirector's time.
        /// </summary>
        /// <returns></returns>
        public Texture2D GetTexture() { return m_texture; }
        

//----------------------------------------------------------------------------------------------------------------------
        //Need to split the PlayableFrames which are currently shared by both this and m_clonedFromAsset
        void TrySplitPlayableFrames(int numIdealFrames) {
            if (null == m_playableFrames) {
                return;
            }

            List<PlayableFrame> prevPlayableFrames = m_playableFrames;
            m_playableFrames = new List<PlayableFrame>(numIdealFrames);
            int prevNumPlayableFrames = prevPlayableFrames.Count;
            
            //Check if this clone is a pure duplicate
            TimelineClip otherTimelineClip = m_clonedFromAsset.GetBoundTimelineClip();
                       
            if (Math.Abs(m_curBoundTimelineClip.duration - otherTimelineClip.duration) < 0.0000001f) {
                for (int i = 0; i < prevNumPlayableFrames; ++i) {
                    m_playableFrames.Add(null);
                    CreatePlayableFrameInList(i);
                    m_playableFrames[i].SetUsed(prevPlayableFrames[i].IsUsed());

                }
                return;
            }

            //Decide which one is on the left side after splitting
            if (m_curBoundTimelineClip.start < m_clonedFromAsset.GetBoundTimelineClip().start) {
                m_playableFrames.AddRange(prevPlayableFrames.GetRange(0,numIdealFrames));
                m_clonedFromAsset.SplitPlayableFramesFromClonedAsset(numIdealFrames,prevPlayableFrames.Count - numIdealFrames);
            } else {
                int idx = prevPlayableFrames.Count - numIdealFrames;
                m_playableFrames.AddRange(prevPlayableFrames.GetRange(idx, idx + numIdealFrames -1));
                m_clonedFromAsset.SplitPlayableFramesFromClonedAsset(0,idx);
            }
            
            //Reinitialize to assign the owner
            double timePerFrame = CalculateTimePerFrame(m_curBoundTimelineClip);
            for (int i = 0; i < numIdealFrames; ++i) {
                m_playableFrames[i].Init(this, timePerFrame * i, m_useImageMarkerVisibility);
            }
            
        }


        //This is called by the cloned asset
        void SplitPlayableFramesFromClonedAsset(int startIndex, int count) {
            if (null == m_playableFrames) {
                return;
            }


            int numIdealFrames = CalculateIdealNumPlayableFrames(m_curBoundTimelineClip);
            if (numIdealFrames != count) {
                Debug.LogWarning("StreamingImageSequencePlayableAsset::ReassignPlayableFrames() Count: " + count
                                 + " is not ideal: " + numIdealFrames                
                );
                return;
            }

            List<PlayableFrame> prevPlayableFrames = m_playableFrames;
            if (startIndex + count > prevPlayableFrames.Count) {
                Debug.LogWarning("StreamingImageSequencePlayableAsset::ReassignPlayableFrames() Invalid params. "
                                 + " StartIndex: " + startIndex +  ", Count: " + count                
                );
                return;
            }
            
            m_playableFrames = new List<PlayableFrame>(numIdealFrames);
            m_playableFrames.AddRange(prevPlayableFrames.GetRange(startIndex,numIdealFrames));

            double timePerFrame = CalculateTimePerFrame(m_curBoundTimelineClip);
            
            //Reinitialize to set the time
            for (int i = 0; i < numIdealFrames; ++i) {
                m_playableFrames[i].Init(this, timePerFrame * i, m_useImageMarkerVisibility);
            }
            
        }
//----------------------------------------------------------------------------------------------------------------------
        private static double LocalTimeToCurveTime(TimelineClip clip, double localTime) {
            AnimationCurve curve = GetAndValidateAnimationCurve(clip);
            return curve.Evaluate((float)localTime);
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

            double timePerFrame = CalculateTimePerFrame(clip);
            
            //Try to check if this frame is "dropped", so that we should use the image in the prev frame
            int frameIndex = (int) (localTime / timePerFrame);
            if (frameIndex >= 0 && null!=m_playableFrames && frameIndex < m_playableFrames.Count) {
                while (null!=m_playableFrames[frameIndex] && !m_playableFrames[frameIndex].IsUsed() && frameIndex > 0) {
                    --frameIndex;
                    localTime = frameIndex * timePerFrame;
                }
            }

            double imageSequenceTime = LocalTimeToCurveTime(clip, localTime);
            int count = m_imagePaths.Count;
            int index = (int)(count * imageSequenceTime);
            index = Mathf.Clamp(index, 0, count - 1);
            return index;
        }

//----------------------------------------------------------------------------------------------------------------------

        internal int GetVersion() { return m_version; }
        internal IList<string> GetImagePaths() { return m_imagePaths; }
        
        //May return uninitialized value during initialization because the resolution hasn't been updated
        internal ImageDimensionInt GetResolution() { return m_resolution; }
        internal System.Collections.IList GetImagePathsNonGeneric() { return m_imagePaths; }


        internal bool GetUseImageMarkerVisibility() {  return m_useImageMarkerVisibility; }

        internal void SetUseImageMarkerVisibility(bool show) { m_useImageMarkerVisibility = show; }

        //These two methods are necessary "hacks" for knowing which TimelineClips currently own
        //this StreamingImageSequencePlayableAssets
        internal void BindTimelineClip(TimelineClip clip) {
            m_curBoundTimelineClip = clip;
            if (null == clip)
                return;
            AnimationCurve curve = GetAndValidateAnimationCurve(clip);
            RefreshTimelineClipCurve(clip, curve);
            
        }
        internal TimelineClip GetBoundTimelineClip()              { return m_curBoundTimelineClip; }
        
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
        private void Reset() {
            m_primaryImageIndex         = 0;
            m_forwardPreloadImageIndex  = 0;
            m_backwardPreloadImageIndex = 0;
            
            m_lastCopiedImageIndex = -1;
            ResetTexture();

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
        public sealed override double duration {  get {  return (null!=m_curBoundTimelineClip) ? m_curBoundTimelineClip.duration : 0;  }  }
       
#endregion         
//---------------------------------------------------------------------------------------------------------------------


        internal void ContinuePreloadingImages() {
            
            if (null == m_imagePaths || 0== m_imagePaths.Count)
                return;

            const int NUM_IMAGES = 2;

            //forward
            int maxForwardPreloadIndex = Mathf.Min(m_forwardPreloadImageIndex + NUM_IMAGES, m_imagePaths.Count) -1;
            for (int i = m_forwardPreloadImageIndex; i <= maxForwardPreloadIndex; ++i) {
                QueueImageLoadTask(i, out _ );
            }
            m_forwardPreloadImageIndex = maxForwardPreloadIndex;
            
            //backward
            int minBackwardPreloadIndex = Mathf.Max((m_backwardPreloadImageIndex - NUM_IMAGES)+1, 0);
            for (int i = m_backwardPreloadImageIndex; i >=minBackwardPreloadIndex; --i) {
                QueueImageLoadTask(i, out _ );
            }
            m_backwardPreloadImageIndex = minBackwardPreloadIndex;
            
        }

//----------------------------------------------------------------------------------------------------------------------        
        private string QueueImageLoadTask(int index, out ImageData imageData) {
            const int TEX_TYPE = StreamingImageSequenceConstants.IMAGE_TYPE_FULL;
            string filename = m_imagePaths[index];
            filename = GetCompleteFilePath(filename);

            StreamingImageSequencePlugin.GetImageDataInto(filename,TEX_TYPE, Time.frameCount, out imageData );
            //Debug.Log("imageData.readStatus " + imageData.readStatus + "Loading " + filename);
            
            if (StreamingImageSequenceConstants.READ_STATUS_LOADING != imageData.ReadStatus ) {
                ImageLoader.RequestLoadFullImage(filename);
            }
            
            return filename;
        }
//----------------------------------------------------------------------------------------------------------------------        
        

        internal bool RequestLoadImage(int index)
        {
            if (null == m_imagePaths || index < 0 || index >= m_imagePaths.Count || string.IsNullOrEmpty(m_imagePaths[index])) {
                return false;
            }

            m_primaryImageIndex         = index;
            m_forwardPreloadImageIndex  = Mathf.Min(m_primaryImageIndex + 1, m_imagePaths.Count - 1);
            m_backwardPreloadImageIndex = Mathf.Max(m_primaryImageIndex - 1, 0);

            
            QueueImageLoadTask(index, out ImageData readResult);

            if (null == m_texture &&  readResult.ReadStatus == StreamingImageSequenceConstants.READ_STATUS_SUCCESS) {

                ResetTexture();
                m_texture = readResult.CreateCompatibleTexture(HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor);
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
//----------------------------------------------------------------------------------------------------------------------        

        internal string GetCompleteFilePath(string filePath)
        {
            string strOverridePath = m_folder;

            if (!string.IsNullOrEmpty(strOverridePath)) {
                filePath = Path.Combine(strOverridePath, filePath).Replace("\\", "/");
            }

            if (Path.IsPathRooted(filePath)) {
                filePath = Path.Combine(PathUtility.GetProjectFolder(), filePath).Replace("\\", "/");
            }
            return filePath;
        }
//----------------------------------------------------------------------------------------------------------------------

        #region PlayableFrames

        private void CreatePlayableFrameInList(int index) {
            PlayableFrame playableFrame = ObjectUtility.CreateScriptableObjectInstance<PlayableFrame>();
#if UNITY_EDITOR                    
            AssetDatabase.AddObjectToAsset(playableFrame, this);
#endif
            double timePerFrame = CalculateTimePerFrame(m_curBoundTimelineClip);
            playableFrame.Init(this, timePerFrame * index, m_useImageMarkerVisibility);
            m_playableFrames[index] = playableFrame;
        }
        
//----------------------------------------------------------------------------------------------------------------------

        internal void ResetPlayableFrames() {
            // TrackAsset track = m_curBoundTimelineClip.parentTrack;
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

            DestroyPlayableFrames();

            //Recalculate the number of frames and create the marker's ground truth data
            int numFrames = CalculateIdealNumPlayableFrames(m_curBoundTimelineClip);
            m_playableFrames =  new List<PlayableFrame>(numFrames);
            UpdatePlayableFramesSize(numFrames);
            
            
#if UNITY_EDITOR //Add to AssetDatabase
            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved );
#endif            
           
        }

        void RefreshPlayableFrames() {
            int numIdealNumPlayableFrames = CalculateIdealNumPlayableFrames(m_curBoundTimelineClip);

            //if this asset was a cloned asset, split the playable frames
            if (null != m_clonedFromAsset) {
                TrySplitPlayableFrames(numIdealNumPlayableFrames);
                m_clonedFromAsset = null;
            }
            
            if (null == m_playableFrames) {
                ResetPlayableFrames();
            }

            //Change the size of m_playableFrames and reinitialize if necessary
            int prevNumPlayableFrames = m_playableFrames.Count;
            if (numIdealNumPlayableFrames != prevNumPlayableFrames) {
#if UNITY_EDITOR
                Undo.RegisterCompleteObjectUndo(this, "StreamingImageSequencePlayableAsset: Updating PlayableFrame List");
#endif                
                //Change the size of m_playableFrames and reinitialize if necessary
                List<bool> prevUsedFrames = new List<bool>(prevNumPlayableFrames);
                foreach (PlayableFrame frame in m_playableFrames) {
                    prevUsedFrames.Add(null == frame || frame.IsUsed()); //if frame ==null, just regard as used.
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
            int numPlayableFrames = m_playableFrames.Count;
            for (int i = 0; i < numPlayableFrames; ++i) {
                
                if (null == m_playableFrames[i]) {
                    CreatePlayableFrameInList(i);
                }
                m_playableFrames[i].Refresh(m_useImageMarkerVisibility);
                
            }
            
        }        
        
//----------------------------------------------------------------------------------------------------------------------

        internal static int CalculateIdealNumPlayableFrames(TimelineClip clip) {
            //Recalculate the number of frames and create the marker's ground truth data
            float fps = clip.parentTrack.timelineAsset.editorSettings.fps;
            int numFrames = Mathf.RoundToInt((float)(clip.duration * fps));
            return numFrames;
            
        }

        private static double CalculateTimePerFrame(TimelineClip clip) {
            float fps = clip.parentTrack.timelineAsset.editorSettings.fps;
            double timePerFrame = 1.0f / fps;
            return timePerFrame;
        }
        
//----------------------------------------------------------------------------------------------------------------------

        private void UpdatePlayableFramesSize(int reqPlayableFramesSize) {

            //Resize m_playableFrames
            if (m_playableFrames.Count < reqPlayableFramesSize) {
                int numNewPlayableFrames  = (reqPlayableFramesSize - m_playableFrames.Count);
                PlayableFrame[] newPlayableFrames = new PlayableFrame[numNewPlayableFrames];
                m_playableFrames.AddRange(newPlayableFrames);                
            }

            if (m_playableFrames.Count > reqPlayableFramesSize) {
                int numLastPlayableFrames = m_playableFrames.Count;
                for (int i = reqPlayableFramesSize; i < numLastPlayableFrames; ++i) {
                    PlayableFrame curFrame = m_playableFrames[i];
                    if (null == curFrame)
                        continue;
                    //[TODO-sin: 2020-7-4] Let's put this in a pool.
                    ObjectUtility.Destroy(curFrame);                
                }
                m_playableFrames.RemoveRange(reqPlayableFramesSize, numLastPlayableFrames - reqPlayableFramesSize);
                
                
            }
            
            Assert.IsTrue(m_playableFrames.Count == reqPlayableFramesSize);

            double timePerFrame = CalculateTimePerFrame(m_curBoundTimelineClip);
            
            for (int i = 0; i < reqPlayableFramesSize; ++i) {
                PlayableFrame curPlayableFrame = m_playableFrames[i];
                
                if (null == curPlayableFrame) {
                    CreatePlayableFrameInList(i);
                }
                m_playableFrames[i].Init(this, timePerFrame * i, m_useImageMarkerVisibility);
            }
            
            
        }
        
        #endregion

//---------------------------------------------------------------------------------------------------------------------
        void ResetTexture() {
            if (null != m_texture) {
                ObjectUtility.Destroy(m_texture);
                m_texture = null;
            }

        }

//---------------------------------------------------------------------------------------------------------------------
        void UpdateResolution(ref ImageData imageData) {
            m_resolution.Width  = imageData.Width;
            m_resolution.Height = imageData.Height;
            m_dimensionRatio = m_resolution.CalculateRatio();
        }
//---------------------------------------------------------------------------------------------------------------------
        

        void ForceUpdateResolution() {
            if (null!=m_imagePaths && m_imagePaths.Count <= 0)
                return;

            //Load the primary image to update the resolution.
            QueueImageLoadTask(m_primaryImageIndex,  out ImageData readResult);
            if (readResult.ReadStatus == StreamingImageSequenceConstants.READ_STATUS_SUCCESS) {               
                UpdateResolution(ref readResult);
            }
            
        }


//----------------------------------------------------------------------------------------------------------------------
        //Get the animation curve from the TimelineClip.  
        private static AnimationCurve GetAndValidateAnimationCurve(TimelineClip clip) {
            AnimationCurve animationCurve = null;
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

        internal static void RefreshTimelineClipCurve(TimelineClip clip, AnimationCurve curve) {
            clip.curves.SetCurve("", typeof(StreamingImageSequencePlayableAsset), "m_time", curve);
#if UNITY_EDITOR            
            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved );
#endif            
        }


//----------------------------------------------------------------------------------------------------------------------        

#region Unity Editor code

#if UNITY_EDITOR         
        internal void SetParam(StreamingImageSequencePlayableAssetParam param) {
            if (param.Resolution.Width > 0 && param.Resolution.Height > 0) {
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
        
        //[TODO-sin: 2020-6-29] PlayableFrames needs to be stored inside the track/TimelineClip instead of this asset
        //directly
        //The ground truth for using/dropping an image in a particular frame. See the notes below
        [SerializeField] List<PlayableFrame> m_playableFrames = null;

        [SerializeField] private int m_version = STREAMING_IMAGE_SEQUENCE_PLAYABLE_ASSET_VERSION;        
        [SerializeField] double m_time;

        private ImageDimensionInt  m_resolution;        
        private float m_dimensionRatio;



#if UNITY_EDITOR
        [SerializeField] private UnityEditor.DefaultAsset m_timelineDefaultAsset = null; //Folder D&D. See notes below
        private static EditorCurveBinding m_timelineEditorCurveBinding =  
            new EditorCurveBinding() {
                path         = "",
                type         = typeof(StreamingImageSequencePlayableAsset),
                propertyName = "m_time"
            };
        
#endif
        [SerializeField] [HideInInspector] private bool m_useImageMarkerVisibility = false;
        
        //[Note-sin: 2020-6-30] TimelineClip should not be a property of StreamingImageSequencePlayableAsset, because
        //StreamingImageSequencePlayableAsset is an asset, and should be able to be bound to 2 different TimelineClips.
        //However, for UseImageMarker to work, we need to know which TimelineClip is bound to the
        //StreamingImageSequencePlayableAsset, because Marker is originally designed to be owned by TrackAsset, but not
        //TimelineAsset
        TimelineClip m_curBoundTimelineClip  = null; 


        private int m_lastCopiedImageIndex; //the index of the image copied to m_texture

        private int m_primaryImageIndex         = 0;
        private int m_forwardPreloadImageIndex  = 0;
        private int m_backwardPreloadImageIndex = 0;
        
        private bool m_verified;
        private StreamingImageSequencePlayableAsset m_clonedFromAsset = null;

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

