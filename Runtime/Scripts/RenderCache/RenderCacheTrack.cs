using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence  {

/// <summary>
/// A track which clip type is RenderCachePlayableAsset.
/// </summary>
[TrackClipType(typeof(RenderCachePlayableAsset))]
[TrackBindingType(typeof(BaseRenderCapturer))]
[TrackColor(0.776f, 0.263f, 0.09f)]
public class RenderCacheTrack : TrackAsset {

    
    private void OnDestroy() {
        m_trackMixer?.Destroy();
    }

//----------------------------------------------------------------------------------------------------------------------    
    /// <inheritdoc/>
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
        ScriptPlayable<RenderCachePlayableMixer> mixer = ScriptPlayable<RenderCachePlayableMixer>.Create(graph, inputCount);
        PlayableDirector director = go.GetComponent<PlayableDirector>();
        m_trackMixer = mixer.GetBehaviour();
        
        if (director != null) {
            Object    boundObject  = director.GetGenericBinding(this);
            BaseRenderCapturer capturer = boundObject as BaseRenderCapturer;
            m_trackMixer.Init(null == capturer ? null : capturer.gameObject, director, GetClips());
        }
        return mixer;
    }
    
//----------------------------------------------------------------------------------------------------------------------    
   
    private RenderCachePlayableMixer m_trackMixer = null;

}

}