﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace UTJTimelineUtil
{

    [System.Serializable]

    public class BlackFaderPlayableAsset : FaderPlayableAsset
    {

        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            m_color = Color.black;
            var bh = new FaderPlayableBehaviour();
            return ScriptPlayable<FaderPlayableBehaviour>.Create(graph, bh);
        }

    }




}