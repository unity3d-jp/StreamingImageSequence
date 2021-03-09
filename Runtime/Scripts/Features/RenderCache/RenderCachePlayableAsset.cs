using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace Unity.StreamingImageSequence {

/// <summary>
/// A PlayableAsset that is used to capture the render results of BaseRenderCapturer in Timeline.
/// Implements the following interfaces:
/// - ITimelineClipAsset: for defining clip capabilities (ClipCaps) 
/// - ISerializationCallbackReceiver: to perform version upgrade, if necessary
/// </summary>
[System.Serializable]
internal class RenderCachePlayableAsset : ImageFolderPlayableAsset<RenderCacheClipData>, ITimelineClipAsset, ISerializationCallbackReceiver {

    RenderCachePlayableAsset() : base() {
        m_editorConfig = new RenderCachePlayableAssetEditorConfig();                 
    }
//----------------------------------------------------------------------------------------------------------------------
    public void OnGraphStart(Playable playable) {
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    public ClipCaps clipCaps {
        get {
            return ClipCaps.None;
        }
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    /// <inheritdoc/>
    public sealed override Playable CreatePlayable(PlayableGraph graph, GameObject go) {
        return Playable.Null;
    }

    #region ISerializationCallbackReceiver

    public void OnBeforeSerialize() {
        m_version = CUR_RENDER_CACHE_PLAYABLE_ASSET_VERSION;        
            
    }

    public void OnAfterDeserialize() {

        if (null == m_editorConfig) {
            m_editorConfig = new RenderCachePlayableAssetEditorConfig();                 
        }
        

        if (m_version < (int) RenderCachePlayableAssetVersion.WATCHED_FILE_0_4) {

            if (null != m_imageFileNames && m_imageFileNames.Count > 0) {
                m_imageFiles = WatchedFileInfo.CreateList(m_folder, m_imageFileNames);
                m_imageFileNames.Clear();
            }             
        }

        if (m_version < (int) RenderCachePlayableAssetVersion.CONFIG_0_7) {
            m_editorConfig.SetUpdateBGColor(m_updateBGColor);
        }
        
        m_version = CUR_RENDER_CACHE_PLAYABLE_ASSET_VERSION;        

    }
        
    #endregion ISerializationCallbackReceiver
    
//----------------------------------------------------------------------------------------------------------------------

    internal RenderCachePlayableAssetEditorConfig GetEditorConfig() { return m_editorConfig;}
    
//----------------------------------------------------------------------------------------------------------------------
    
#if UNITY_EDITOR
    protected override void ReloadInternalInEditorV() {
        ResetResolution();            
    }        
    protected override string[] GetSupportedImageFilePatternsV() { return m_imageFilePatterns; }
    
#endif //UNITY_EDITOR
    
//----------------------------------------------------------------------------------------------------------------------

    [HideInInspector][SerializeField] private Color m_updateBGColor = Color.black;
    
    [HideInInspector][SerializeField] private int m_version = (int) RenderCachePlayableAssetVersion.INITIAL_0_0;
    [HideInInspector][SerializeField] private RenderCachePlayableAssetEditorConfig m_editorConfig;
    private const int CUR_RENDER_CACHE_PLAYABLE_ASSET_VERSION = (int) RenderCachePlayableAssetVersion.CONFIG_0_7;
    
#if UNITY_EDITOR
    private static readonly string[] m_imageFilePatterns = {
        "*.png",
    };        
#endif
    
    
    enum RenderCachePlayableAssetVersion {
        INITIAL_0_0 = 1, //initial for version 0.0.0-preview (obsolete)
        WATCHED_FILE_0_4,   //watched_file for version 0.4.0-preview
        CONFIG_0_7,         //Config for version 0.7.0-preview
    
        
    }
}


} //end namespace

