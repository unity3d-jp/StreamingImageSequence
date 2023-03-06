using Unity.FilmInternalUtilities;

namespace Unity.StreamingImageSequence {

internal class FaderTrackMixerEvent : AnalyticsEvent<FaderTrackMixerEvent.EventData> {

    internal FaderTrackMixerEvent(int clips) : base(new EventData { numClips = clips, }) { }
    
    internal struct EventData {
        public int numClips;
    }

//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    internal override string eventName => "streamingimagesequence_fadertrack_mixer";
    internal override int    maxItems  => 1;

    
}

} //end namespace