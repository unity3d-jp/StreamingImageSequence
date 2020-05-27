using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.StreamingImageSequence;
using UnityEngine.Timeline;

namespace UnityEditor.StreamingImageSequence {

internal class RenderCachePreview {

    public RenderCachePreview(RenderCachePlayableAsset playableAsset) {
        m_playableAsset = playableAsset;
    }



//----------------------------------------------------------------------------------------------------------------------
    private readonly RenderCachePlayableAsset m_playableAsset = null;
}

} //end namespace

