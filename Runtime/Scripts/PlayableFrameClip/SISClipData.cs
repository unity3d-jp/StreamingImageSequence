using System;
using System.Collections.Generic;
using Unity.FilmInternalUtilities;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Unity.StreamingImageSequence {

[Serializable]
internal class SISClipData : PlayableFrameClipData {

    public SISClipData() : base() { }

    internal SISClipData(TimelineClip clipOwner) : base (clipOwner) {}

    internal SISClipData(TimelineClip owner, SISClipData other) : base(owner, other) {     
    }
    
}

} //end namespace


