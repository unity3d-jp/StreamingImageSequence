using UnityEngine.Playables;

namespace UnityEngine.StreamingImageSequence
{

    [System.Serializable]

    internal class BlackFaderPlayableAsset : FaderPlayableAsset
    {

        public override Playable CreatePlayable(PlayableGraph graph, GameObject go)
        {
            m_color = Color.black;
            var bh = new FaderPlayableBehaviour();
            return ScriptPlayable<FaderPlayableBehaviour>.Create(graph, bh);
        }

    }




}