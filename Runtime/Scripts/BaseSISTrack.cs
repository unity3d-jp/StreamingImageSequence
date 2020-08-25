using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence
{
    
internal abstract class BaseSISTrack : TrackAsset {
    protected virtual SISTrackCaps GetCapsV() { return SISTrackCaps.NONE; }    
}

} //end namespace
