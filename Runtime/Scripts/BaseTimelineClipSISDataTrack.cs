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
/// A track which requires its TimelineClip to store TimelineClipSISData as an extension
/// </summary>
internal abstract class BaseTimelineClipSISDataTrack<T> : TrackAsset where T: BaseTimelineClipSISDataPlayableAsset {
    
#if UNITY_EDITOR        
    [InitializeOnLoadMethod]
    static void Onload() {
        Undo.undoRedoPerformed += BaseTimelineClipSISDataTrack_OnUndoRedoPerformed;
    }
    
    static void BaseTimelineClipSISDataTrack_OnUndoRedoPerformed() {
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

            if (null != m_sisDataCollection && m_sisDataCollection.ContainsKey(clip)) {                
                sisData =   m_sisDataCollection[clip];
            } else {                
                T sisPlayableAsset = clip.asset as T;
                Assert.IsNotNull(sisPlayableAsset);                 
                sisData = sisPlayableAsset.GetBoundTimelineClipSISData();
            }

            if (null == sisData) {
                sisData = new TimelineClipSISData(clip);
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
    public sealed override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
               
        if (null == m_sisDataCollection) {
            m_sisDataCollection = new Dictionary<TimelineClip, TimelineClipSISData>();
        }
        RefreshTimelineClipSISData();
        RefreshMarkers();
                

        Playable mixer = CreateTrackMixerInternal(graph, go, inputCount);

#if UNITY_EDITOR        
        RefreshTimelineEditorAfterUndoRedo();
#endif        
        
        return mixer;
    }

    protected abstract Playable CreateTrackMixerInternal(PlayableGraph graph, GameObject go, int inputCount);
    

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

        TimelineClipSISData sisData = new TimelineClipSISData(clip);
        m_sisDataCollection[clip] = sisData;
        return sisData;
    }
        
//----------------------------------------------------------------------------------------------------------------------
    private void RefreshTimelineClipSISData() {
        //Initialize PlayableAssets and TimelineClipSISData       
        foreach (TimelineClip clip in GetClips()) {
            T sisPlayableAsset = clip.asset as T;
            Assert.IsNotNull(sisPlayableAsset);               
            
            TimelineClipSISData timelineClipSISData = sisPlayableAsset.GetBoundTimelineClipSISData();
            if (null == timelineClipSISData) {
                timelineClipSISData = GetOrCreateTimelineClipSISData(clip);                
            } else {
                if (!m_sisDataCollection.ContainsKey(clip)) {
                    m_sisDataCollection.Add(clip, timelineClipSISData);;            
                }                
            }
            
            //Make sure that this track, and the clip are the owners
            timelineClipSISData.SetOwner(clip);

            sisPlayableAsset.BindTimelineClipSISData(timelineClipSISData);            
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
        }

        foreach (UseImageMarker marker in m_markersToDelete) {
            DeleteMarker(marker);
        }
    }

    
//----------------------------------------------------------------------------------------------------------------------

    [HideInInspector][SerializeField] List<TimelineClipSISData> m_serializedSISDataCollection = null;

    private Dictionary<TimelineClip, TimelineClipSISData> m_sisDataCollection = null;
    
    private BasePlayableMixer<T> m_trackMixer = null;
    
    private readonly List<UseImageMarker> m_markersToDelete = new List<UseImageMarker>();

#if UNITY_EDITOR    
    private static bool m_undoRedo = false;
#endif    

}

} //end namespace

//----------------------------------------------------------------------------------------------------------------------

