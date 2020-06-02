using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence
{ 
/// <summary>
/// A track which clip type is RenderCachePlayableAsset.
/// </summary>
[TrackClipType(typeof(RenderCachePlayableAsset))]
[TrackBindingType(typeof(Camera))]
[TrackColor(0.776f, 0.263f, 0.09f)]
public class RenderCacheTrack : TrackAsset
{

    //TODO-sin: 2020-5-27: Factor out common code with StreamingImageSequenceTrack
    protected override void OnAfterTrackDeserialize() {
        //Re-setup the PlayableAsset
        foreach (TimelineClip clip in m_Clips) {
            RenderCachePlayableAsset playableAsset = clip.asset as RenderCachePlayableAsset;
            if (null == playableAsset)
                continue;
                
            playableAsset.OnAfterTrackDeserialize(clip);
        }
    }

}

}