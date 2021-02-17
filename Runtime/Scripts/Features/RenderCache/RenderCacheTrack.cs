using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Timeline;
#endif

namespace Unity.StreamingImageSequence  {

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
        if (null == TimelineEditor.inspectedDirector)
            return;
        TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
    } 
    
#endif
    
    private void OnDestroy() {
        m_trackMixer?.Destroy();
        m_trackMixer = null;
        
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
    
    internal override int GetCapsV() { return (int) SISTrackCaps.IMAGE_FOLDER; }
    
//----------------------------------------------------------------------------------------------------------------------    
   
    private RenderCachePlayableMixer m_trackMixer = null;

}

}