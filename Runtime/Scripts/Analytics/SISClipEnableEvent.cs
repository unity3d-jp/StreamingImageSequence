using Unity.FilmInternalUtilities;

namespace Unity.StreamingImageSequence {

internal class SISClipEnableEvent : AnalyticsEvent<SISClipEnableEvent.EventData> {

    internal SISClipEnableEvent(double duration, bool frameMarkers, int images, int width, int height) : base(
        new EventData {
            clipDuration = duration, 
            showFrameMarkers = frameMarkers, 
            numImages = images, 
            imageResolutionWidth = width, 
            imageResolutionHeight = height
        }) 
    { }
    
    internal struct EventData {
        public double clipDuration;
        public bool   showFrameMarkers;
        public int    numImages;
        public int    imageResolutionWidth;
        public int    imageResolutionHeight;
    }
    
//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    internal override string eventName => "streamingimagesequence_sisclip_enable";
    internal override int    maxItems  => 1;
    
}

} //end namespace