using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {

/// <summary>
/// A PlayableAsset that is used to capture the render results of BaseRenderCapturer in Timeline.
/// Implements the following interfaces:
/// - ITimelineClipAsset: for defining clip capabilities (ClipCaps) 
/// </summary>
[System.Serializable]
internal class RenderCachePlayableAsset : BaseTimelineClipSISDataPlayableAsset, ITimelineClipAsset {
   
//----------------------------------------------------------------------------------------------------------------------


    public ClipCaps clipCaps {
        get {
            return ClipCaps.None;
        }
    }
   
    
//----------------------------------------------------------------------------------------------------------------------
    internal string GetFolder() { return m_folder;}
    internal void SetFolder(string folder) { m_folder = folder;}

   
    
//----------------------------------------------------------------------------------------------------------------------
    
    /// <inheritdoc/>
    public sealed override Playable CreatePlayable(PlayableGraph graph, GameObject go) {
        return Playable.Null;
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    [SerializeField] private string m_folder = null;


}

} //end namespace

