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
/// </summary>
[System.Serializable]
internal class RenderCachePlayableAsset : ImageFolderPlayableAsset, ITimelineClipAsset {
   
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
    
//----------------------------------------------------------------------------------------------------------------------

    internal void SetImageFileNames(List<string> imageFileNames) {
        m_imageFileNames = imageFileNames;
    }
    
}

} //end namespace

