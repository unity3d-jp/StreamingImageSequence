using System;
using System.IO;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Assertions;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.FilmInternalUtilities;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor.Timeline;
using UnityEditor;

#endif

namespace Unity.StreamingImageSequence {

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
        //[Note-sin: 2020-9-8] OnPlayableDestroy() will be called when TimelineEditor is refreshed
        //(ContentsAddedOrRemoved or ContentsModified) 
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

    void OnEnable() {
        m_texture              = null;
        m_lastCopiedImageIndex = -1;
    }

    private void OnDisable() {
        ResetTexture();
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

        TimelineClipSISData timelineSISData = GetBoundClipData();

        if (null != timelineSISData) {
            double scaledTimePerFrame = TimelineUtility.CalculateTimePerFrame(clip) * clip.timeScale;            
      
            //Try to check if this frame is "dropped", so that we should use the image in the prev frame
            int playableFrameIndex = Mathf.RoundToInt((float) localTime / (float)scaledTimePerFrame);
            if (playableFrameIndex < 0)
                return 0;
                
            SISPlayableFrame playableFrame      = timelineSISData.GetPlayableFrame(playableFrameIndex);
            while (playableFrameIndex > 0 && !playableFrame.IsUsed()) {
                --playableFrameIndex;
                playableFrame = timelineSISData.GetPlayableFrame(playableFrameIndex);
                localTime     = playableFrameIndex * scaledTimePerFrame;
            }                
        }


        double imageSequenceTime = LocalTimeToCurveTime(clip, localTime);
        int count = m_imageFiles.Count;
        
        //Can't round up, because if the time for the next frame hasn't been reached, then we should stick 
        int index = Mathf.FloorToInt(count * (float) imageSequenceTime);
        index = Mathf.Clamp(index, 0, count - 1);
        return index;
    }

//----------------------------------------------------------------------------------------------------------------------
    private static double LocalTimeToCurveTime(TimelineClip clip, double localTime) {
        GetAndValidateAnimationCurve(clip, out AnimationCurve curve);                       
        return curve.Evaluate((float)(localTime));
    }
    
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
    protected override void ReloadInternalV() {
        m_lastCopiedImageIndex = -1;
        ResetResolution();
        RequestLoadImage(m_primaryImageIndex);
        
    }        
//----------------------------------------------------------------------------------------------------------------------        
    
    /// <inheritdoc/>
    public ClipCaps clipCaps {
#if AT_USE_TIMELINE_GE_1_4_0            
        get { return ClipCaps.ClipIn | ClipCaps.AutoScale | ClipCaps.Extrapolation; }
#else            
        get { return ClipCaps.ClipIn | ClipCaps.SpeedMultiplier | ClipCaps.Extrapolation; }
#endif            
    }
            
//---------------------------------------------------------------------------------------------------------------------

#region PlayableAsset functions override
    /// <inheritdoc/>
    public sealed override Playable CreatePlayable(PlayableGraph graph, GameObject go) {
        return Playable.Create(graph);
    }
   
#endregion    
    
   
//----------------------------------------------------------------------------------------------------------------------
    [CanBeNull]
    internal Texture2D GetTexture() { return m_primaryImageIndex == m_lastCopiedImageIndex ? m_texture : null;}
    
//----------------------------------------------------------------------------------------------------------------------

    internal void RequestLoadImage(int index) {
        int numImages = m_imageFiles.Count;
        
        if (null == m_imageFiles || index < 0 || index >= numImages 
            || string.IsNullOrEmpty(m_imageFiles[index].GetName())) {
            return;
        }

        m_primaryImageIndex = index;

        if (QueueImageLoadTask(index, out ImageData readResult)) {
            m_forwardPreloadImageIndex  = Mathf.Min(m_primaryImageIndex + 1, numImages - 1);
            m_backwardPreloadImageIndex = Mathf.Max(m_primaryImageIndex - 1, 0);                
        } else {
            //If we can't queue, try from the primary index again
            m_forwardPreloadImageIndex = m_backwardPreloadImageIndex = index;
        }

        if (StreamingImageSequenceConstants.READ_STATUS_SUCCESS == readResult.ReadStatus) {
            UpdateTexture(readResult, index);
        }

    }
    
//----------------------------------------------------------------------------------------------------------------------
           
    //returns false if the texture is not changed, true otherwise    
    internal bool UpdateTextureWithRequestedImage() {
        if (m_lastCopiedImageIndex == m_primaryImageIndex)
            return false;
        
        string fullPath = GetImageFilePath(m_primaryImageIndex);
        if (string.IsNullOrEmpty(fullPath) || !File.Exists(fullPath))
            return false;

        ImageLoader.GetImageDataInto(fullPath,StreamingImageSequenceConstants.IMAGE_TYPE_FULL,out ImageData imageData);
        if (StreamingImageSequenceConstants.READ_STATUS_SUCCESS != imageData.ReadStatus)
            return false;
        
        UpdateTexture(imageData, m_primaryImageIndex);
        return true;
    }
    
//----------------------------------------------------------------------------------------------------------------------

    internal void ContinuePreloadingImages() {

        if (null == m_imageFiles || 0== m_imageFiles.Count)
            return;

        const int NUM_IMAGES = 2;

        //forward
        int maxForwardPreloadIndex = Mathf.Min(m_forwardPreloadImageIndex + NUM_IMAGES, m_imageFiles.Count) -1;
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
        string fullPath = GetImageFilePath(index);

        if (!File.Exists(fullPath)) {
            imageData = new ImageData(StreamingImageSequenceConstants.READ_STATUS_FAIL);                
            return true;
        }

        ImageLoader.GetImageDataInto(fullPath,StreamingImageSequenceConstants.IMAGE_TYPE_FULL,out imageData);
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

    
//---------------------------------------------------------------------------------------------------------------------
    Texture2D UpdateTexture(ImageData readResult, int index) {
        if (m_texture.IsNullRef()) {
            m_texture = readResult.CreateCompatibleTexture(HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor);                    
        }

        if (m_lastCopiedImageIndex == index)
            return m_texture;

        m_texture.name = "Full: " + m_imageFiles[index].GetName();
        readResult.CopyBufferToTexture(m_texture);
        UpdateResolution(ref readResult);                
        m_lastCopiedImageIndex = index;
        return m_texture;
    }
    

//---------------------------------------------------------------------------------------------------------------------
    void ResetTexture() {
        if (!m_texture.IsNullRef()) {
            ObjectUtility.Destroy(m_texture);
            m_texture = null;
        }

    }

//----------------------------------------------------------------------------------------------------------------------
    //Make sure to set the curve of the TimelineClip 
    internal void InitTimelineClipCurve(TimelineClip clip) {
        Assert.IsNotNull(clip);                        
        bool curveChanged = GetAndValidateAnimationCurve(clip, out AnimationCurve curve);
        if (curveChanged) {
            SetTimelineClipCurve(clip, curve);
        }
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
    //Returns:
    //- true : if the animationCurve of the clip was changed or validated
    //- false: if the animationCurve was already valid        
    private static bool GetAndValidateAnimationCurve(TimelineClip clip, out AnimationCurve animationCurve) {
        
        //[TODO-sin: 2020-7-30] Support getting animation curve in Runtime
        animationCurve = null;
#if UNITY_EDITOR
        if (clip.curves) {
            animationCurve = AnimationUtility.GetEditorCurve(clip.curves, m_timelineEditorCurveBinding);
        } else {            
            clip.CreateCurves("Curves: " + clip.displayName); //[Note-sin: 2021-2-3] for handling older versions of SIS 
        }
#endif        
        
        bool newlyCreated = false;
        if (null == animationCurve) {
            animationCurve = new AnimationCurve();
            newlyCreated   = true;
        }

        bool validated = ValidateAnimationCurve(ref animationCurve, (float) clip.duration);
        return newlyCreated || validated;
        
    }

//----------------------------------------------------------------------------------------------------------------------
    //Validate: make sure we have at least two keys
    //Returns:
    //- true : if the animationCurve was invalid, and has been validated
    //- false: if the animationCurve was already valid        
    private static bool ValidateAnimationCurve(ref AnimationCurve animationCurve, float clipDuration) {
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
            default: 
                return false; 
        }

        return true;
    }
    
//----------------------------------------------------------------------------------------------------------------------

    private static void SetTimelineClipCurve(TimelineClip clip, AnimationCurve curve) {
#if UNITY_EDITOR
        AnimationUtility.SetEditorCurve(clip.curves, m_timelineEditorCurveBinding, curve);
        
#if AT_USE_TIMELINE_GE_1_5_0                    
        TimelineEditor.Refresh(RefreshReason.WindowNeedsRedraw );
#else         
        TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved ); //must use this for Pre- 1.5.0
#endif //AT_USE_TIMELINE_GE_1_5_0            
        
#else         
        clip.curves.SetCurve("", typeof(StreamingImageSequencePlayableAsset), "m_time", curve);
#endif //UNITY_EDITOR            
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
        if (updatedFolder != m_folder)
            return;
        
        Reload();
#endif
    }

#endregion Observer
    
//----------------------------------------------------------------------------------------------------------------------        

#region ISerializationCallbackReceiver

    public void OnBeforeSerialize() {
        m_version = CUR_SIS_PLAYABLE_ASSET_VERSION;        
    }

    public void OnAfterDeserialize() {
        if (m_version < (int) SISPlayableAssetVersion.WATCHED_FILE_0_4) {
            if (null != m_imageFileNames && m_imageFileNames.Count > 0) {
                m_imageFiles = WatchedFileInfo.CreateList(m_folder, m_imageFileNames);
                m_imageFileNames.Clear();
            }             
            
            //Use the folder defined in older version if set
            if (string.IsNullOrEmpty(m_folder) && !string.IsNullOrEmpty(Folder)) {
                m_folder = Folder;
            }            
        }
        
        m_version = CUR_SIS_PLAYABLE_ASSET_VERSION;
    }
    
#endregion ISerializationCallbackReceiver
    
//----------------------------------------------------------------------------------------------------------------------        

#region Unity Editor code

#if UNITY_EDITOR         
    internal void InitFolder(string folder, List<WatchedFileInfo> imageFiles, ImageDimensionInt res = new ImageDimensionInt()) 
    {
        m_folder     = folder;
        m_imageFiles = imageFiles;
        UpdateResolution(res);
        
        if (null!=m_folder && m_folder.StartsWith("Assets")) {
            m_timelineDefaultAsset = AssetDatabase.LoadAssetAtPath<UnityEditor.DefaultAsset>(m_folder);
        } else {
            m_timelineDefaultAsset = null;
        }

        ResetTexture();
        EditorUtility.SetDirty(this);
    }
    
    internal UnityEditor.DefaultAsset GetTimelineDefaultAsset() { return m_timelineDefaultAsset; }
    
    protected override string[] GetSupportedImageFilePatternsV() { return m_imageFilePatterns; }

    internal static string[] GetSupportedImageFilePatterns() { return m_imageFilePatterns;}
    
    
#endif //end #if UNITY_EDITOR        
    
#endregion
    
//----------------------------------------------------------------------------------------------------------------------

    [HideInInspector][SerializeField] private int m_version = (int) SISPlayableAssetVersion.INITIAL;        
    
    [SerializeField] double m_time;

    
    //[TODO-Sin: 2021-2-3] Obsolete. This is put here to deserialize old versions of SIS (MovieProxy)
    [SerializeField] private string Folder; 
    

#if UNITY_EDITOR
    [SerializeField] private UnityEditor.DefaultAsset m_timelineDefaultAsset = null; //Folder D&D. See notes below
    private static EditorCurveBinding m_timelineEditorCurveBinding =  
        new EditorCurveBinding() {
            path         = "",
            type         = typeof(StreamingImageSequencePlayableAsset),
            propertyName = "m_time"
        };

    private static readonly string[] m_imageFilePatterns = {
        "*.png",
        "*.tga"             
    };
    
#endif
   
    
    private int m_lastCopiedImageIndex; //the index of the image copied to m_texture

    private int m_primaryImageIndex         = 0;
    private int m_forwardPreloadImageIndex  = 0;
    private int m_backwardPreloadImageIndex = 0;


    Texture2D    m_texture       = null;

//----------------------------------------------------------------------------------------------------------------------
    
    private const int CUR_SIS_PLAYABLE_ASSET_VERSION = (int) SISPlayableAssetVersion.WATCHED_FILE_0_4;
            

    enum SISPlayableAssetVersion {
        INITIAL        = 1, //initial
        FOLDER_MD5_0_3,       //For version 0_3.0-preview, (obsolete)
        WATCHED_FILE_0_4,     //For version 0.4.0-preview, with watched file, instead of folder

    }
}

} //end namespace

//----------------------------------------------------------------------------------------------------------------------
//[Note-Sin: 2019-12-23] We need two things, in order to enable folder drag/drop to the timeline Window
//1. Derive this class from PlayableAsset
//2. Declare UnityEditor.DefaultAsset variable 

