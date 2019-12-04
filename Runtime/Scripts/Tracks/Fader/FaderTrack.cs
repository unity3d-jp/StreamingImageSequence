using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace Unity.StreamingImageSequence
{

    [TrackClipType(typeof(FaderPlayableAsset))]
    [TrackClipType(typeof(WhiteFaderPlayableAsset))]
    [TrackClipType(typeof(BlackFaderPlayableAsset))]
    [TrackBindingType(typeof(Image))]
    [TrackColor(0.263f, 0.09f, 0.263f)]

    public class FaderTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            var mixer = ScriptPlayable<FaderPlayableMixer>.Create(graph, inputCount);
            var director = go.GetComponent<PlayableDirector>();
            if ( director != null )
            {
                var outputGo = director.GetGenericBinding(this) as Image;
                FaderPlayableMixer bh = mixer.GetBehaviour();
                bh.m_clips = GetClips();
                if ( outputGo != null )
                {
                    bh.boundGameObject = outputGo.gameObject;
                    bh.m_initialColor = outputGo.color;
                }
                bh.m_PlayableDirector = director;
            }
            return mixer;
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            var ps = director.GetGenericBinding(this) as Image;
            if (ps == null) return;

            var go = ps.gameObject;
            driver.AddFromName<Image>(go, "m_Color");

        }
    }

}