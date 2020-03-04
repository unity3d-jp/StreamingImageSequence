using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {

[System.Serializable] 
internal class FaderPlayableAsset : PlayableAsset, ITimelineClipAsset {
    public ClipCaps clipCaps {
        get {
            return ClipCaps.None;
        }
    }

    public override Playable CreatePlayable(PlayableGraph graph, GameObject go) {
        var bh = new FaderPlayableBehaviour();
        return ScriptPlayable<FaderPlayableBehaviour>.Create(graph, bh);
    }

    internal Color GetColor() { return m_color;}
    internal FadeType GetFadeType() { return m_fadeType;}

//----------------------------------------------------------------------------------------------------------------------
    [SerializeField] private Color m_color = Color.black;
    [SerializeField] private FadeType m_fadeType = FadeType.FADE_IN;

//----------------------------------------------------------------------------------------------------------------------

}


}