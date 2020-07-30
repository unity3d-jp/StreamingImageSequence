namespace UnityEngine.StreamingImageSequence {

internal interface IHasTimelineClipSISData {
    TimelineClipSISData GetBoundTimelineClipSISData();
    void BindTimelineClipSISData(TimelineClipSISData timelineClipSISData);               
}

} //end namespace

