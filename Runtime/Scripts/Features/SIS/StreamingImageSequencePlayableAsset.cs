﻿using System;
using System.IO;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Assertions;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.FilmInternalUtilities;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
#if UNITY_EDITOR
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
internal class StreamingImageSequencePlayableAsset : ImageFolderPlayableAsset<SISClipData>, ITimelineClipAsset
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
        
        bool        showFrameMarkers = false;
        SISClipData clipData         = GetBoundClipData();
        if (null != clipData) {
            showFrameMarkers = clipData.AreFrameMarkersRequested();
        }
        ImageDimensionInt res           = GetResolution();
        int               numImageFiles = null != m_imageFiles ? m_imageFiles.Count : 0;
        AnalyticsSender.SendEventInEditor(new SISClipEnableEvent(duration, showFrameMarkers, numImageFiles, res.Width, res.Height));
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

//--------------------------------------------------------------------------------------------------------------------------------------------------------------

    void OnEnable() {
        m_texture              = null;
        m_lastCopiedImageIndex = -1;
        
        m_regularAssetMipmapCheckLogger = new OneTimeLogger(() => {
            Assert.IsNotNull(m_texture);
            return m_texture.mipmapCount != 1;
        },$"Textures should not have mipmap. Folder: ");
        
    }


    private void OnDisable() {
        ResetTexture();
    }
       
//--------------------------------------------------------------------------------------------------------------------------------------------------------------

    //[Note-sin: 2020-7-17] This is also called when the TimelineClip in TimelineWindow is deleted, instead of just
    //The TimelineClipAsset (on file, for example) is deleted
    protected override void OnDestroyInternalV() {
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

        SISClipData clipData = GetBoundClipData();
        if (null == clipData) {
            return 0;
        }


        {
            //drop disabled frames
            double scaledTimePerFrame = TimelineUtility.CalculateTimePerFrame(clip) * clip.timeScale;            
      
            //Try to check if this frame is "dropped", so that we should use the image in the prev frame
            int playableFrameIndex = Mathf.RoundToInt((float) (localTime - clip.clipIn) / (float)scaledTimePerFrame);
            if (playableFrameIndex < 0)
                return 0;
                
            SISPlayableFrame playableFrame      = clipData.GetPlayableFrame(playableFrameIndex);
            while (playableFrameIndex > 0 && !playableFrame.IsUsed()) {
                --playableFrameIndex;
                playableFrame = clipData.GetPlayableFrame(playableFrameIndex);
                localTime     = playableFrameIndex * scaledTimePerFrame;
            }                
        }

        AnimationCurve curve = clipData.GetAnimationCurve();
        double imageSequenceTime = curve.Evaluate((float) localTime);

        int count = m_imageFiles.Count;
        
        double floatingIndex = count * imageSequenceTime;       
        int index = HalfRoundDown(floatingIndex);
        index = Mathf.Clamp(index, 0, count - 1);        
         return index;
    }

    internal static int HalfRoundDown(double val) {
        
        //Half Round down. Subtract with 0.01, and use MidpointRounding.AwayFromZero
        //Only works for positive integers, which is ok for SISPlayableAsset
        return (int) Math.Round(val - 0.01,0, MidpointRounding.AwayFromZero);
        
    }

    
//----------------------------------------------------------------------------------------------------------------------        
    private void Reset() {
        
        m_primaryImageIndex         = 0;
        m_forwardPreloadImageIndex  = 0;
        m_backwardPreloadImageIndex = 0;
        
        m_lastCopiedImageIndex = -1;
        ResetTexture();
        ResetResolution();

#if UNITY_EDITOR
        m_editorCachedTextureLoader.UnloadAll();
#endif
    }

//----------------------------------------------------------------------------------------------------------------------        
    
    /// <inheritdoc/>
    public ClipCaps clipCaps {
        get { return ClipCaps.ClipIn | ClipCaps.SpeedMultiplier | ClipCaps.Extrapolation; }
    }
            
//---------------------------------------------------------------------------------------------------------------------

#region PlayableAsset functions override
    /// <inheritdoc/>
    public sealed override Playable CreatePlayable(PlayableGraph graph, GameObject go) {
        return Playable.Create(graph);
    }

    public override double duration {
        get {
            SISClipData clipData = GetBoundClipData();
            if (null == clipData)
                return 0;

            return clipData.CalculateCurveDuration();
        }
    }

    #endregion    
    
   
//----------------------------------------------------------------------------------------------------------------------

    internal bool IsRequestedImageReady() { return m_primaryImageIndex == m_lastCopiedImageIndex;}

    [CanBeNull]
    internal Texture2D GetTexture() { return m_texture;}

    internal FilterMode GetTextureFilterMode() {
        return m_textureFilterMode;
    }

    internal void SetTextureFilterMode(FilterMode filterMode) {
        m_textureFilterMode = filterMode;
    }
    
//----------------------------------------------------------------------------------------------------------------------

    internal void RequestLoadImage(int index) {
        int numImages = m_imageFiles.Count;
        
        if (null == m_imageFiles || index < 0 || index >= numImages) 
            return;

        string fullPath = GetImageFilePath(index);
        if (string.IsNullOrEmpty(fullPath))
            return;

        m_primaryImageIndex = index;
        
        if (QueueImageLoadTask(fullPath, out ImageData readResult, out Texture2D tex)) {
            m_forwardPreloadImageIndex  = Mathf.Min(m_primaryImageIndex + 1, numImages - 1);
            m_backwardPreloadImageIndex = Mathf.Max(m_primaryImageIndex - 1, 0);
        } else {
            //If we can't queue, try from the primary index again
            m_forwardPreloadImageIndex = m_backwardPreloadImageIndex = index;
        }

        switch (readResult.ReadStatus) {
            case StreamingImageSequenceConstants.READ_STATUS_SUCCESS: {
                UpdateTexture(readResult, index);
                break;
            }
#if UNITY_EDITOR
            case StreamingImageSequenceConstants.READ_STATUS_USE_EDITOR_API: {
                UpdateTexture(tex, m_primaryImageIndex);
                break;
            }
#endif
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

#if UNITY_EDITOR        
        if (UpdateTextureAsRegularAssetInEditor(fullPath, m_primaryImageIndex)) {
            return true;
        }
#endif        
        
        
        ImageLoader.GetImageDataInto(fullPath,StreamingImageSequenceConstants.IMAGE_TYPE_FULL,out ImageData imageData);
        if (StreamingImageSequenceConstants.READ_STATUS_SUCCESS != imageData.ReadStatus)
            return false;
        
        UpdateTexture(imageData, m_primaryImageIndex);
        return true;
    }
        
//--------------------------------------------------------------------------------------------------------------------------------------------------------------

    internal void ContinuePreloadingImages(int numNeighboringImagesToLoad) {

        if (null == m_imageFiles || 0== m_imageFiles.Count)
            return;

        //forward
        int maxForwardPreloadIndex   = Mathf.Min(m_forwardPreloadImageIndex + numNeighboringImagesToLoad, m_imageFiles.Count) -1;
        int startForwardPreloadIndex = m_forwardPreloadImageIndex;
        for (int i = startForwardPreloadIndex; i <= maxForwardPreloadIndex; ++i) {
            if (QueueImageLoadTask(i, out _, out _)) {
                ++m_forwardPreloadImageIndex;
            } else {
                break;
            }
        }
        
        //backward
        int minBackwardPreloadIndex   = Mathf.Max((m_backwardPreloadImageIndex - numNeighboringImagesToLoad)+1, 0);
        int startBackwardPreloadIndex = m_backwardPreloadImageIndex;
        for (int i = startBackwardPreloadIndex; i >=minBackwardPreloadIndex; --i) {
            if (QueueImageLoadTask(i, out _, out _)) {
                --m_backwardPreloadImageIndex;
            } else {
                break;
            }
        }
    }

//--------------------------------------------------------------------------------------------------------------------------------------------------------------
   
    //return true if we should continue preloading the next image. False otherwise
    private bool QueueImageLoadTask(int index, out ImageData imageData, out Texture2D tex) {
        string fullPath = GetImageFilePath(index);
        return QueueImageLoadTask(fullPath, out imageData, out tex);
    }


    //return true if we should continue preloading the next image. False otherwise
    private bool QueueImageLoadTask(string fullPath, out ImageData imageData, out Texture2D tex) {

        tex = null;
        
#if UNITY_EDITOR
        if (m_editorCachedTextureLoader.GetOrLoad(fullPath, out imageData, out tex))
            return true;
#endif
        
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
    
//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    Texture2D UpdateTexture(ImageData imageData, int index) {
        bool textureRecreated = false;
        if (m_texture.IsNullRef() || !imageData.IsTextureCompatible(m_texture) || m_texture.filterMode != m_textureFilterMode) {
            m_texture = imageData.CreateCompatibleTexture(HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor,m_textureFilterMode);
            textureRecreated = true;
        }

        if (!textureRecreated && m_lastCopiedImageIndex == index)
            return m_texture;

        m_texture.name = "Full: " + m_imageFiles[index].GetName();
        imageData.CopyBufferToTexture(m_texture);
        UpdateResolution(imageData);                
        m_lastCopiedImageIndex = index;
        return m_texture;
    }
    
    Texture2D UpdateTexture(Texture2D srcTex, int index) {
        bool textureRecreated = false;
        if (m_texture.IsNullRef() || !m_texture.AreSizeAndFormatEqual(srcTex) || m_texture.filterMode != m_textureFilterMode) {        
            m_texture = new Texture2D(srcTex.width, srcTex.height, srcTex.graphicsFormat, 
                mipCount: srcTex.mipmapCount, TextureCreationFlags.MipChain) 
            {
                filterMode = m_textureFilterMode,
                hideFlags  = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor,
            };
            textureRecreated = true;
        }

        m_regularAssetMipmapCheckLogger.Update("[SIS]",m_folder);        

        if (!textureRecreated && m_lastCopiedImageIndex == index)
            return m_texture;
        
        m_texture.name = "Full: " + m_imageFiles[index].GetName();
        Graphics.CopyTexture(src: srcTex, dst:m_texture);
        UpdateResolution(m_texture) ;
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
    internal void InitFolderInEditor(string folder, List<WatchedFileInfo> imageFiles, ImageDimensionInt res = new ImageDimensionInt()) 
    {
        m_folder     = folder;
        m_imageFiles = imageFiles;
        m_editorCachedTextureLoader.UnloadAll();
        UpdateResolution(res);
        
        if (null!=m_folder && m_folder.StartsWith("Assets")) {
            m_timelineDefaultAsset = AssetDatabase.LoadAssetAtPath<UnityEditor.DefaultAsset>(m_folder);
        } else {
            m_timelineDefaultAsset = null;
        }

        ResetTexture();
        EditorUtility.SetDirty(this);
    }

    protected override void ReloadInternalInEditorV() {
        m_lastCopiedImageIndex = -1;
        m_editorCachedTextureLoader.UnloadAll();
        ResetResolution();
        RequestLoadImage(m_primaryImageIndex);
    }        
    
    internal UnityEditor.DefaultAsset GetTimelineDefaultAsset() { return m_timelineDefaultAsset; }
    
    protected override string[] GetSupportedImageFilePatternsV() { return m_imageFilePatterns; }

    internal static string[] GetSupportedImageFilePatterns() { return m_imageFilePatterns;}

    internal static EditorCurveBinding GetTimeCurveBinding() { return m_timeCurveBinding; }


    private bool UpdateTextureAsRegularAssetInEditor(string fullPath, int imageIndex) {
        if (!m_editorCachedTextureLoader.GetOrLoad(fullPath, out _, out Texture2D tex)) {
            return false;
        }
        
        UpdateTexture(tex, imageIndex);
        return true;
    }
    
#endif //end #if UNITY_EDITOR        
    
#endregion
    
//----------------------------------------------------------------------------------------------------------------------

    [HideInInspector][SerializeField] private int m_version = (int) SISPlayableAssetVersion.INITIAL;        
    
    [SerializeField] private double     m_time;
    [SerializeField] private FilterMode m_textureFilterMode = FilterMode.Bilinear;

    
    //[TODO-Sin: 2021-2-3] Obsolete. This is put here to deserialize old versions of SIS (MovieProxy)
    [SerializeField] private string Folder; 
    

#if UNITY_EDITOR
    [SerializeField] private UnityEditor.DefaultAsset m_timelineDefaultAsset = null; //Folder D&D. See notes below
    private static EditorCurveBinding m_timeCurveBinding =  
        new EditorCurveBinding() {
            path         = "",
            type         = typeof(StreamingImageSequencePlayableAsset),
            propertyName = "m_time"
        };

    private static readonly string[] m_imageFilePatterns = {
        "*.png",
        "*.exr",
        "*.tga"             
    };
    
#endif  
    
    private int m_lastCopiedImageIndex; //the index of the image copied to m_texture

    private int m_primaryImageIndex         = 0;
    private int m_forwardPreloadImageIndex  = 0;
    private int m_backwardPreloadImageIndex = 0;

    Texture2D    m_texture       = null;

    private OneTimeLogger m_regularAssetMipmapCheckLogger;


#if UNITY_EDITOR
    private EditorCachedTextureLoader m_editorCachedTextureLoader = new EditorCachedTextureLoader();
#endif
    
//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    private const int CUR_SIS_PLAYABLE_ASSET_VERSION = (int) SISPlayableAssetVersion.WATCHED_FILE_0_4;

    enum SISPlayableAssetVersion {
        INITIAL        = 1, //initial
        FOLDER_MD5_0_3,       //For version 0_3.0-preview, (obsolete)
        WATCHED_FILE_0_4,     //For version 0.4.0-preview, with watched file, instead of folder

    }
}

} //end namespace

//--------------------------------------------------------------------------------------------------------------------------------------------------------------
//[Note-Sin: 2019-12-23] We need two things, in order to enable folder drag/drop to the timeline Window
//1. Derive this class from PlayableAsset
//2. Declare UnityEditor.DefaultAsset variable 

