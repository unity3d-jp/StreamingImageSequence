using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Timeline;

#endif


namespace UnityEngine.StreamingImageSequence { 
/// <summary>
/// A track which clip type is StreamingImageSequencePlayableAsset.
/// It shows the active image from the images assigned to StreamingImageSequencePlayableAsset,
/// into a bound GameObject that has StreamingImageSequenceRenderer component.
/// </summary>
[TrackClipType(typeof(StreamingImageSequencePlayableAsset))]
[TrackBindingType(typeof(StreamingImageSequenceRenderer))]
[TrackColor(0.776f, 0.263f, 0.09f)]
[NotKeyable]
internal class StreamingImageSequenceTrack : BaseTimelineClipSISDataTrack<StreamingImageSequencePlayableAsset> {

#if UNITY_EDITOR        
    [InitializeOnLoadMethod]
    static void Onload() {
        Undo.undoRedoPerformed += StreamingImageSequenceTrack_OnUndoRedoPerformed;
    }
    
    static void StreamingImageSequenceTrack_OnUndoRedoPerformed() {
        if (null == TimelineEditor.inspectedDirector)
            return;
        
        TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
    } 
    
#endif
    
//----------------------------------------------------------------------------------------------------------------------
    private void OnDestroy() {
        m_trackMixer?.Destroy();
    }
    
    
//----------------------------------------------------------------------------------------------------------------------

    protected override Playable CreateTrackMixerInternal(PlayableGraph graph, GameObject go, int inputCount) {

        InitTrackCurves();
        
        var              mixer    = ScriptPlayable<StreamingImageSequencePlayableMixer>.Create(graph, inputCount);
        PlayableDirector director = go.GetComponent<PlayableDirector>();
        m_trackMixer = mixer.GetBehaviour();

        if (director != null) {
            Object                         boundObject = director.GetGenericBinding(this);
            StreamingImageSequenceRenderer renderer = boundObject as StreamingImageSequenceRenderer;
            m_trackMixer.Init(null == renderer ? null : renderer.gameObject, director, GetClips());
        }
        
        return mixer;
    }     

//----------------------------------------------------------------------------------------------------------------------

    /// <inheritdoc/>
    public override string ToString() { return name; }
    
        
//----------------------------------------------------------------------------------------------------------------------
    private void InitTrackCurves() {
        //Initialize the curves of TimelineClips       
        foreach (TimelineClip clip in  GetClips()) {            
            StreamingImageSequencePlayableAsset sisPlayableAsset = clip.asset as StreamingImageSequencePlayableAsset;
            Assert.IsNotNull(sisPlayableAsset);
            sisPlayableAsset.InitTimelineClipCurve(clip);
        }        
    }

//----------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Get the currently active PlayableAsset in the track according to the PlayableDirector's time
    /// </summary>
    /// <returns>The TimelineClip's asset as StreamingImageSequencePlayableAsset. Returns null if there is no active
    /// PlayableAsset.
    /// </returns>
    internal StreamingImageSequencePlayableAsset GetActivePlayableAsset() {
        double time = (null != m_trackMixer ) ? m_trackMixer.GetDirectorTime() : 0;
        StreamingImageSequencePlayableMixer.GetActiveTimelineClipInto(m_Clips, time,
            out TimelineClip clip, out StreamingImageSequencePlayableAsset asset
        );
        return asset;
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    private StreamingImageSequencePlayableMixer m_trackMixer = null;   
    
   
}

} //end namespace

