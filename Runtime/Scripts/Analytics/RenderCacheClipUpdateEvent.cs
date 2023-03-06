using Unity.FilmInternalUtilities;

namespace Unity.StreamingImageSequence  {

internal class RenderCacheClipUpdateEvent : AnalyticsEvent<RenderCacheClipUpdateEvent.EventData> {

    internal RenderCacheClipUpdateEvent(double duration, bool frameMarkers, int updatedFrames, int formatType) : base(
        new EventData {
            clipDuration     = duration, 
            showFrameMarkers = frameMarkers, 
            numUpdatedFrames = updatedFrames, 
            outputFormatType = formatType, 
        }) 
    { }
    
    internal struct EventData {
        public double clipDuration;
        public bool   showFrameMarkers;
        public int    numUpdatedFrames;
        public int    outputFormatType;
    }
    
//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    internal override string eventName => "streamingimagesequence_rendercacheclip_update";
    internal override int    maxItems  => 1;

    
}

} //end namespace