using System;

using UnityEngine.Playables;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Timeline;
#endif

namespace UnityEngine.StreamingImageSequence {

internal abstract class BaseTimelineClipSISDataPlayableAsset : PlayableAsset{

    protected virtual void OnDestroy() {          
        m_timelineClipSISData?.Destroy();           
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    //These methods are necessary "hacks" for knowing the PlayableFrames/UseImageMarkers that belong to this
    //this StreamingImageSequencePlayableAssets        
    internal void BindTimelineClipSISData(TimelineClipSISData sisData) { m_timelineClipSISData = sisData;}         
    internal TimelineClipSISData GetBoundTimelineClipSISData() { return m_timelineClipSISData; }
    
//----------------------------------------------------------------------------------------------------------------------
    
    #region PlayableFrames

    internal void ResetPlayableFrames() {
#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(this, "StreamingImageSequencePlayableAsset: Resetting Use Image Markers");
#endif
        m_timelineClipSISData.ResetPlayableFrames();
            
#if UNITY_EDITOR 
        TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved );
#endif            
           
    }

    internal void RefreshPlayableFrames() {
            
        //Haven't been assigned yet. May happen during recompile
        if (null == m_timelineClipSISData)
            return;
                       
        m_timelineClipSISData.RefreshPlayableFrames();            
    }
        
    #endregion
    
//----------------------------------------------------------------------------------------------------------------------
    
    //[Note-sin: 2020-6-30] TimelineClipSISData stores extra data of TimelineClip, because we can't extend
    //TimelineClip at the moment. Ideally, it should not be a property of StreamingImageSequencePlayableAsset, because
    //StreamingImageSequencePlayableAsset is an asset, and should be able to be bound to 2 different TimelineClipsSISData.
    //However, for FrameMarker to work, we need to know which TimelineClipSISData is bound to the
    //StreamingImageSequencePlayableAsset, because Marker is originally designed to be owned by TrackAsset, but not
    //TimelineClip        
    [NonSerialized] private TimelineClipSISData m_timelineClipSISData = null;
    
}

} //end namespace

