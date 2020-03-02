using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence
{

    [System.Serializable]

    internal class FaderPlayableAsset : PlayableAsset, ITimelineClipAsset
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

    internal enum FadeType
    {
        FadeIn,
        FadeOut
    }


}