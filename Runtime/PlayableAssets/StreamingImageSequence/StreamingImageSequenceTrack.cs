using UnityEngine.Playables;
using UnityEngine.Timeline;
using System.Collections.Generic;
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
public class StreamingImageSequenceTrack : TrackAsset {
    
#if UNITY_EDITOR        
    [InitializeOnLoadMethod]
    static void Onload() {
        Undo.undoRedoPerformed += StreamingImageSequenceTrack_OnUndoRedoPerformed;
    }
    
    static void StreamingImageSequenceTrack_OnUndoRedoPerformed() {
        m_undoRedo = true;
    } 
    

    void RefreshTimelineEditorAfterUndoRedo() {        
        if (m_undoRedo && null!= TimelineEditor.inspectedDirector) {
            //After undo, the UseImageMarkers are not refreshed
            TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
            m_undoRedo = false;
        }
        
    }
    
#endif
    
//----------------------------------------------------------------------------------------------------------------------
    private void OnDestroy() {
        m_trackMixer?.Destroy();
    }

//----------------------------------------------------------------------------------------------------------------------
    /// <inheritdoc/>
    protected override void OnBeforeTrackSerialize() {
        base.OnBeforeTrackSerialize();
        m_serializedSISDataCollection = new List<TimelineClipSISData>();
        
        
        foreach (TimelineClip clip in GetClips()) {
            TimelineClipSISData sisData = null;

            if (m_sisDataCollection.ContainsKey(clip)) {                
                sisData =   m_sisDataCollection[clip];
            } else {                
                StreamingImageSequencePlayableAsset sisPlayableAsset = clip.asset as StreamingImageSequencePlayableAsset;
                Assert.IsNotNull(sisPlayableAsset);                 
                sisData = sisPlayableAsset.GetBoundTimelineClipSISData();
            }

            if (null == sisData) {
                sisData = new TimelineClipSISData();
            }
            
                       
            m_serializedSISDataCollection.Add(sisData);
        }
    }

    /// <inheritdoc/>
    protected override  void OnAfterTrackDeserialize() {
        base.OnAfterTrackDeserialize();
        m_sisDataCollection = new Dictionary<TimelineClip, TimelineClipSISData>();
        
        
        IEnumerator<TimelineClip> clipEnumerator = GetClips().GetEnumerator();
        List<TimelineClipSISData>.Enumerator sisEnumerator = m_serializedSISDataCollection.GetEnumerator();
        while (clipEnumerator.MoveNext() && sisEnumerator.MoveNext()) {
            TimelineClip clip = clipEnumerator.Current;
            Assert.IsNotNull(clip);

            TimelineClipSISData timelineClipSISData = sisEnumerator.Current;
            Assert.IsNotNull(timelineClipSISData);           
            
            m_sisDataCollection[clip] = timelineClipSISData;
            
        }
        clipEnumerator.Dispose();
        sisEnumerator.Dispose();
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    /// <inheritdoc/>
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
               
        if (null == m_sisDataCollection) {
            m_sisDataCollection = new Dictionary<TimelineClip, TimelineClipSISData>();
        }

        RefreshTimelineClip();
        RefreshMarkers();
                
        var              mixer    = ScriptPlayable<StreamingImageSequencePlayableMixer>.Create(graph, inputCount);
        PlayableDirector director = go.GetComponent<PlayableDirector>();
        m_trackMixer = mixer.GetBehaviour();

        if (director != null) {
            var boundGo = director.GetGenericBinding(this);
            StreamingImageSequenceRenderer renderer = boundGo as StreamingImageSequenceRenderer;
            m_trackMixer.Init(null == renderer ? null : renderer.gameObject, director, GetClips());
        }

#if UNITY_EDITOR        
        RefreshTimelineEditorAfterUndoRedo();
#endif        
        
        return mixer;
    }

//----------------------------------------------------------------------------------------------------------------------

    /// <inheritdoc/>
    public override string ToString() { return name; }
    
//----------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Get the currently active PlayableAsset in the track according to the PlayableDirector's time
    /// </summary>
    /// <returns>The TimelineClip's asset as StreamingImageSequencePlayableAsset. Returns null if there is no active
    /// PlayableAsset.
    /// </returns>
    public StreamingImageSequencePlayableAsset GetActivePlayableAsset() {
        double time = (null != m_trackMixer ) ? m_trackMixer.GetDirectorTime() : 0;
        StreamingImageSequencePlayableMixer.GetActiveTimelineClipInto(m_Clips, time,
            out TimelineClip clip, out StreamingImageSequencePlayableAsset asset
        );
        return asset;
    }
    
    

//----------------------------------------------------------------------------------------------------------------------
    private TimelineClipSISData GetOrCreateTimelineClipSISData(TimelineClip clip) {
        Assert.IsNotNull(m_sisDataCollection);
        
        if (m_sisDataCollection.ContainsKey(clip)) {
            return m_sisDataCollection[clip];            
        }

        TimelineClipSISData sisData = new TimelineClipSISData();
        m_sisDataCollection[clip] = sisData;
        return sisData;
    }
        
//----------------------------------------------------------------------------------------------------------------------
    private void RefreshTimelineClip() {
        //Initialize PlayableAssets and TimelineClipSISData       
        foreach (TimelineClip clip in GetClips()) {
            StreamingImageSequencePlayableAsset sisPlayableAsset = clip.asset as StreamingImageSequencePlayableAsset;
            Assert.IsNotNull(sisPlayableAsset);               
            sisPlayableAsset.BindTimelineClip(clip);
            
            TimelineClipSISData timelineClipSISData = sisPlayableAsset.GetBoundTimelineClipSISData();
            if (null == timelineClipSISData) {
                timelineClipSISData = GetOrCreateTimelineClipSISData(clip);                
                sisPlayableAsset.BindTimelineClipSISData(timelineClipSISData);
            } else {
                if (!m_sisDataCollection.ContainsKey(clip)) {
                    m_sisDataCollection.Add(clip, timelineClipSISData);;            
                }                
            }
            
            //Make sure that this track, and the clip are the owners
            timelineClipSISData.SetOwner(clip);
            
        }
        
    }

//----------------------------------------------------------------------------------------------------------------------
    private void RefreshMarkers() {
        foreach(IMarker m in GetMarkers()) {
            UseImageMarker marker = m as UseImageMarker;
            if (null == marker)
                continue;

            if (!marker.Refresh()) {
                m_markersToDelete.Add(marker);                
            }
            else {
                Debug.Log("Refreshing Marker: " + marker.name);
            }           
        }

        foreach (UseImageMarker marker in m_markersToDelete) {
            DeleteMarker(marker);
        }
    }

    
//----------------------------------------------------------------------------------------------------------------------

    [HideInInspector][SerializeField] List<TimelineClipSISData> m_serializedSISDataCollection = null;

    private Dictionary<TimelineClip, TimelineClipSISData> m_sisDataCollection = null;
    
    private StreamingImageSequencePlayableMixer m_trackMixer = null;
    private readonly List<UseImageMarker> m_markersToDelete = new List<UseImageMarker>();

#if UNITY_EDITOR    
    private static bool m_undoRedo = false;
#endif    

}

} //end namespace

//----------------------------------------------------------------------------------------------------------------------

