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
    
    [HideInInspector][SerializeField] private int m_version = CUR_RENDER_CACHE_PLAYABLE_ASSET_VERSION;        
    private const int CUR_RENDER_CACHE_PLAYABLE_ASSET_VERSION = (int) RenderCachePlayableAssetVersion.INITIAL_1_0;
    
    enum RenderCachePlayableAssetVersion {
        INITIAL_1_0 = 1, //initial for version 1.0
    
    }
}


} //end namespace

