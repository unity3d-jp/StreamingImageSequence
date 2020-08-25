using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence
{
    
internal abstract class BaseSISTrack : TrackAsset {
    internal virtual SISTrackCaps GetCapsV() { return SISTrackCaps.NONE; }    
}

} //end namespace
