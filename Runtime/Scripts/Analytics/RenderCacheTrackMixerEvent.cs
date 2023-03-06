using Unity.FilmInternalUtilities;

namespace Unity.StreamingImageSequence {

internal class RenderCacheTrackMixerEvent : AnalyticsEvent<RenderCacheTrackMixerEvent.EventData> {

    internal RenderCacheTrackMixerEvent(int clips) : base(new EventData { numClips = clips, }) { }
    
    internal struct EventData {
        public int numClips;
    }
    
//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    internal override string eventName => "streamingimagesequence_rendercachetrack_mixer";
    internal override int    maxItems  => 1;

    
}

} //end namespace