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


    }

}