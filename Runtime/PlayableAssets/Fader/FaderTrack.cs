using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace UnityEngine.StreamingImageSequence
{

    [TrackClipType(typeof(FaderPlayableAsset))]
    [TrackBindingType(typeof(Image))]
    [TrackColor(0.263f, 0.09f, 0.263f)]

    internal class FaderTrack : TrackAsset
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
                if ( outputGo != null ) {
                    bh.BindGameObject(outputGo.gameObject);
                }
                bh.m_playableDirector = director;
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