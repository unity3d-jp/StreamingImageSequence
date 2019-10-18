using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace UTJTimelineUtil
{

    [System.Serializable]

    public class FaderPlayableAsset : PlayableAsset, ITimelineClipAsset
    {
        public Color m_color = Color.black;
        public bool m_noFade = false;
        public FadeType m_type = FadeType.FadeIn;

        public ClipCaps clipCaps
        {
            get
            {
                return ClipCaps.None;
            }
        }

        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            var bh = new FaderPlayableBehaviour();
            return ScriptPlayable<FaderPlayableBehaviour>.Create(graph, bh);
        }

    }

    public enum FadeType
    {
        FadeIn,
        FadeOut
    }


}