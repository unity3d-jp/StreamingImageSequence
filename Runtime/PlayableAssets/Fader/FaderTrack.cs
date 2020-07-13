using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace UnityEngine.StreamingImageSequence
{

    [TrackClipType(typeof(FaderPlayableAsset))]
    [TrackBindingType(typeof(Image))]
    [TrackColor(0.263f, 0.09f, 0.263f)]
    [NotKeyable]
    internal class FaderTrack : TrackAsset {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {

            var mixer = ScriptPlayable<FaderPlayableMixer>.Create(graph, inputCount);
            PlayableDirector director = go.GetComponent<PlayableDirector>();
            if ( director != null ) {
                Image image = director.GetGenericBinding(this) as Image;
                if (null == image) {
                    return mixer;
                }
                FaderPlayableMixer bh = mixer.GetBehaviour();
                bh.Init(image.gameObject, director, GetClips());
            }
            return mixer;
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver) {
            var ps = director.GetGenericBinding(this) as Image;
            if (ps == null) return;

            var go = ps.gameObject;
            driver.AddFromName<Image>(go, "m_Color");

        }
    }

}