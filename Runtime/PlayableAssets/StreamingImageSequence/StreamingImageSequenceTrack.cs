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
public class StreamingImageSequenceTrack : TrackAsset
{
    private void OnDestroy() {
        m_trackMixer?.Destroy();
    }

//----------------------------------------------------------------------------------------------------------------------
    /// <inheritdoc/>
    protected override void OnBeforeTrackSerialize() {
        base.OnBeforeTrackSerialize();
        m_serializedSISDataCollection.Clear();
        
        foreach (TimelineClip clip in GetClips()) {
            TimelineClipSISData sisData = null;
            StreamingImageSequencePlayableAsset clipAsset = clip.asset as StreamingImageSequencePlayableAsset;

            if (null != clipAsset && m_sisDataCollection.ContainsKey(clipAsset)) {                
                sisData =   m_sisDataCollection[clipAsset];
            }
            else {                
                sisData = new TimelineClipSISData();
            }
            
                       
            m_serializedSISDataCollection.Add(sisData);
        }
    }

    /// <inheritdoc/>
    protected override  void OnAfterTrackDeserialize() {
        base.OnAfterTrackDeserialize();
        m_sisDataCollection = new Dictionary<StreamingImageSequencePlayableAsset, TimelineClipSISData>();
        
        IEnumerator<TimelineClip> clipEnumerator = GetClips().GetEnumerator();
        List<TimelineClipSISData>.Enumerator sisEnumerator = m_serializedSISDataCollection.GetEnumerator();
        while (clipEnumerator.MoveNext() && sisEnumerator.MoveNext()) {
            TimelineClip clip = clipEnumerator.Current;
            Assert.IsNotNull(clip);
            StreamingImageSequencePlayableAsset clipAsset = clip.asset as StreamingImageSequencePlayableAsset;
            Assert.IsNotNull(clipAsset);

            TimelineClipSISData timelineClipSISData = sisEnumerator.Current;
            Assert.IsNotNull(timelineClipSISData);
            
            m_sisDataCollection[clipAsset] = timelineClipSISData;
            clipAsset.BindTimelineClipSISData(timelineClipSISData);
            timelineClipSISData.Init(this);
            
        }
        clipEnumerator.Dispose();
        sisEnumerator.Dispose();
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    /// <inheritdoc/>
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount) {
        var mixer = ScriptPlayable<StreamingImageSequencePlayableMixer>.Create(graph, inputCount);
        PlayableDirector director = go.GetComponent<PlayableDirector>();
        m_trackMixer = mixer.GetBehaviour();
        
        if (director != null) {
            var boundGo = director.GetGenericBinding(this);
            StreamingImageSequenceRenderer renderer = boundGo as StreamingImageSequenceRenderer;
            m_trackMixer.Init(null == renderer ? null : renderer.gameObject, director, GetClips());
        }
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

    [HideInInspector][SerializeField] List<TimelineClipSISData> m_serializedSISDataCollection = null;

    private Dictionary<StreamingImageSequencePlayableAsset, TimelineClipSISData> m_sisDataCollection = null;
    
    private StreamingImageSequencePlayableMixer m_trackMixer = null;

}

} //end namespace

//----------------------------------------------------------------------------------------------------------------------

