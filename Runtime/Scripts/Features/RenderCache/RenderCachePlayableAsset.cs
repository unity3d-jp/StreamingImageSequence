using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Assertions;
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
internal class RenderCachePlayableAsset : ImageFolderPlayableAsset, ITimelineClipAsset, ISerializationCallbackReceiver {
    protected override void ReloadInternalV() {
        ResetResolution();            
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
            
    }

    public void OnAfterDeserialize() {

        if (m_version < (int) RenderCachePlayableAssetVersion.WATCHED_FILE_1_0) {

            if (null != m_imageFileNames && m_imageFileNames.Count > 0) {
                m_imageFiles = WatchedFileInfo.CreateList(m_folder, m_imageFileNames);
                m_imageFileNames.Clear();
            }             
        }
        
        m_version = CUR_RENDER_CACHE_PLAYABLE_ASSET_VERSION;
    }
        
    #endregion ISerializationCallbackReceiver
    
//----------------------------------------------------------------------------------------------------------------------

    internal void SetImageFiles(List<WatchedFileInfo> imageFiles) {
        m_imageFiles = imageFiles;
    }

    
//----------------------------------------------------------------------------------------------------------------------
    
#if UNITY_EDITOR
    protected override string[] GetSupportedImageFilePatternsV() { return m_imageFilePatterns; }
    
#endif
    
//----------------------------------------------------------------------------------------------------------------------
    
#pragma warning disable 414
    [HideInInspector][SerializeField] private int m_version = CUR_RENDER_CACHE_PLAYABLE_ASSET_VERSION;
#pragma warning restore 414
    private const int CUR_RENDER_CACHE_PLAYABLE_ASSET_VERSION = (int) RenderCachePlayableAssetVersion.WATCHED_FILE_1_0;
    
#if UNITY_EDITOR
    private static readonly string[] m_imageFilePatterns = {
        "*.png",
    };        
#endif
    
    
    enum RenderCachePlayableAssetVersion {
        INITIAL_1_0 = 1, //initial for version 1.0.0-preview (obsolete)
        WATCHED_FILE_1_0, //watched_file for version 1.0.0-preview
    
    }
}


} //end namespace

