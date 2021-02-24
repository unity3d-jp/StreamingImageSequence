using System;
using UnityEngine.Timeline;


namespace Unity.StreamingImageSequence {

[Serializable]
internal class SISClipData : PlayableFrameClipData {

    public SISClipData() : base() { }

    internal SISClipData(TimelineClip clipOwner) : base (clipOwner) {}

    internal SISClipData(TimelineClip owner, SISClipData other) : base(owner, other) {}
    
}

} //end namespace


