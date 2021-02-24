using System;
using UnityEngine.Timeline;


namespace Unity.StreamingImageSequence {

[Serializable]
internal class RenderCacheClipData : PlayableFrameClipData {

    public RenderCacheClipData() : base() { }

    internal RenderCacheClipData(TimelineClip clipOwner) : base (clipOwner) {}

    internal RenderCacheClipData(TimelineClip owner, SISClipData other) : base(owner, other) {}
    
}

} //end namespace


