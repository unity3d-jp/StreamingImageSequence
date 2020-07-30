using UnityEngine.Playables;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.StreamingImageSequence  {

/// <summary>
/// A track which clip type is RenderCachePlayableAsset.
/// </summary>
[TrackClipType(typeof(RenderCachePlayableAsset))]
[TrackBindingType(typeof(BaseRenderCapturer))]
[TrackColor(0.263f, 0.776f, 0.09f)]
internal class RenderCacheTrack : BaseTimelineClipSISDataTrack<RenderCachePlayableAsset> {

#if UNITY_EDITOR        
    [InitializeOnLoadMethod]
    static void Onload() {
        Undo.undoRedoPerformed += RenderCacheTrack_OnUndoRedoPerformed;
    }
    
    static void RenderCacheTrack_OnUndoRedoPerformed() {
        m_undoRedo = true;
    } 
    
#endif
    
    private void OnDestroy() {
        m_trackMixer?.Destroy();
    }

//----------------------------------------------------------------------------------------------------------------------

    protected override Playable CreateTrackMixerInternal(PlayableGraph graph, GameObject go, int inputCount) {

        ScriptPlayable<RenderCachePlayableMixer> mixer = ScriptPlayable<RenderCachePlayableMixer>.Create(graph, inputCount);
        PlayableDirector director = go.GetComponent<PlayableDirector>();
        m_trackMixer = mixer.GetBehaviour();
        
        if (director != null) {
            Object             boundObject = director.GetGenericBinding(this);
            BaseRenderCapturer capturer    = boundObject as BaseRenderCapturer;
            m_trackMixer.Init(null == capturer ? null : capturer.gameObject, director, GetClips());
        }
        return mixer;
    }     
    
    
//----------------------------------------------------------------------------------------------------------------------    
   
    private RenderCachePlayableMixer m_trackMixer = null;

}

}