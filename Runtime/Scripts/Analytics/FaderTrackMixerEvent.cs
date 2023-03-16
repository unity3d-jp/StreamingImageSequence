using Unity.FilmInternalUtilities;

namespace Unity.StreamingImageSequence {

internal class FaderTrackMixerEvent : AnalyticsEvent {

    internal FaderTrackMixerEvent(int clips) : base(new EventData { numClips = clips, }) { }
    
    private class EventData : AnalyticsEventData {
        public int numClips;
    }

//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    internal override string eventName => "streamingimagesequence_fadertrack_mixer";
    internal override int    maxItems  => 1;

    
}

} //end namespace