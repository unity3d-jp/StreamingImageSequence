using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {

/// <summary>
/// A PlayableAsset that is used to capture the render results of BaseRenderCapturer in Timeline.
/// Implements the following interfaces:
/// - ITimelineClipAsset: for defining clip capabilities (ClipCaps) 
/// - ISerializationCallbackReceiver: to perform version upgrade, if necessary
/// </summary>
[System.Serializable]
internal class RenderCachePlayableAsset : ImageFolderPlayableAsset, ITimelineClipAsset, ISerializationCallbackReceiver {
   
    protected override void ResetInternalV() {
        ResetResolution();            
    }        
    
//----------------------------------------------------------------------------------------------------------------------
    public void OnGraphStart(Playable playable) {

#if UNITY_EDITOR
        //Check folder MD5
        if (string.IsNullOrEmpty(m_folder) || !Directory.Exists(m_folder)) 
            return;


        Reload();
#endif
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
        m_version = CUR_RENDER_CACHE_PLAYABLE_ASSET_VERSION;
    }
        
    #endregion ISerializationCallbackReceiver
    
//----------------------------------------------------------------------------------------------------------------------

    internal void SetImageFileNames(List<string> imageFileNames) {
        m_imageFileNames = imageFileNames;
    }

    
//----------------------------------------------------------------------------------------------------------------------
    
#if UNITY_EDITOR
    protected override string[] GetSupportedImageFilePatternsV() { return m_imageFilePatterns; }
    
#endif
    
//----------------------------------------------------------------------------------------------------------------------
    
#pragma warning disable 414
    [HideInInspector][SerializeField] private int m_version = CUR_RENDER_CACHE_PLAYABLE_ASSET_VERSION;
#pragma warning restore 414
    private const int CUR_RENDER_CACHE_PLAYABLE_ASSET_VERSION = (int) RenderCachePlayableAssetVersion.INITIAL_1_0;
    
#if UNITY_EDITOR
    private static readonly string[] m_imageFilePatterns = {
        "*.png",
    };        
#endif
    
    
    enum RenderCachePlayableAssetVersion {
        INITIAL_1_0 = 1, //initial for version 1.0
    
    }
}


} //end namespace

