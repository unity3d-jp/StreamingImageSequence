using Unity.FilmInternalUtilities;

namespace Unity.StreamingImageSequence {

internal class SISTrackMixerEvent : AnalyticsEvent<SISTrackMixerEvent.EventData> {

    internal SISTrackMixerEvent(int clips, bool query) : base(new EventData { numClips = clips, }) { }
    
    internal struct EventData {
        public int numClips;
    }
    
//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    internal override string eventName => "streamingimagesequence_sistrack_mixer";
    internal override int    maxItems  => 1;

    
}

} //end namespace