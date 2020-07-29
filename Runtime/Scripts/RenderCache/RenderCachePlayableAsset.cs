using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {

/// <summary>
/// A PlayableAsset that is used to capture the render results of BaseRenderCapturer in Timeline.
/// Implements the following interfaces:
/// - ITimelineClipAsset: for defining clip capabilities (ClipCaps) 
/// </summary>
[System.Serializable]
internal class RenderCachePlayableAsset : PlayableAsset, ITimelineClipAsset {

    
//----------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Constructor
    /// </summary>
    public RenderCachePlayableAsset() {
        
    }

    public ClipCaps clipCaps {
        get {
            return ClipCaps.None;
        }
    }
    
    internal void Refresh() {
        //Move the timeline to start of the clip until the end.
        //Access the camera, and store the rendered results somewhere
        
    }

    
//----------------------------------------------------------------------------------------------------------------------
    
    internal TimelineClip GetTimelineClip() { return m_timelineClip; }
    internal void BindTimelineClip(TimelineClip clip) { m_timelineClip = clip; }

    
//----------------------------------------------------------------------------------------------------------------------
    internal string GetFolder() { return m_folder;}
    internal void SetFolder(string folder) { m_folder = folder;}

    internal bool GetDeleteImagesBeforeUpdating() { return m_deleteImagesBeforeUpdating;}
    internal void SetDeleteImagesBeforeUpdating(bool deleteImages) { m_deleteImagesBeforeUpdating = deleteImages;}   
    
    
//----------------------------------------------------------------------------------------------------------------------
    
    #region PlayableAsset functions override
    /// <inheritdoc/>
    public sealed override Playable CreatePlayable(PlayableGraph graph, GameObject go) {
        return Playable.Null;
    }
    
    #endregion         
    
    
//----------------------------------------------------------------------------------------------------------------------    
    private TimelineClip m_timelineClip = null;
    [SerializeField] private string m_folder = null;
    [SerializeField] private bool m_deleteImagesBeforeUpdating = true;


}

} //end namespace

