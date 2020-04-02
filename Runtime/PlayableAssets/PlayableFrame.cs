using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
internal class PlayableFrame : ScriptableObject {

    internal void Init(StreamingImageSequencePlayableAsset asset, double localTime) {
        m_useImage = true;
        m_playableAsset = asset;
        m_localTime = localTime;

        if (null == m_marker) {
            CreateMarker();
        }
    }

    internal StreamingImageSequencePlayableAsset GetPlayableAsset() {  return m_playableAsset; }

    private void OnDestroy() {
        if (null == m_marker)
            return;

        DeleteMarker();
    }

    //----------------------------------------------------------------------------------------------------------------------
    internal bool IsUsed() { return m_useImage; }
    internal void SetUsed(bool used) { m_useImage = used; }
    internal double GetLocalTime() { return m_localTime; }

//----------------------------------------------------------------------------------------------------------------------
    internal void Refresh(bool useImageMarkerVisibility) {
        //Delete Marker first if it's not in the correct track (e.g: after the TimelineClip was moved)
        if (null!= m_marker && m_marker.parent != m_playableAsset.GetTimelineClip().parentTrack) {
            DeleteMarker();
        }

        //Show/Hide the marker
        if (null != m_marker && !useImageMarkerVisibility) {
            DeleteMarker();
        } else if (null == m_marker && useImageMarkerVisibility) {
            CreateMarker();
        }

        if (m_marker) {
            m_marker.Init(this);
            m_marker.Refresh();
        }
    }
//----------------------------------------------------------------------------------------------------------------------

    void CreateMarker() {
#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(this, "PlayableFrame: CreateMarker");
#endif        
        TimelineClip timelineClip = m_playableAsset.GetTimelineClip();
        m_marker = timelineClip.parentTrack.CreateMarker<UseImageMarker>(m_localTime);
        m_marker.Init(this);
    }

    void DeleteMarker() {
#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(this, "PlayableFrame: DeleteMarker");
#endif        
        TrackAsset track = m_marker.parent;
        track.DeleteMarker(m_marker);
    }
    
//----------------------------------------------------------------------------------------------------------------------

    [SerializeField] private bool m_useImage;
    [SerializeField] private double m_localTime;
    [SerializeField] private StreamingImageSequencePlayableAsset m_playableAsset = null; 
    [SerializeField] private UseImageMarker m_marker = null; //ScriptableObject -> Marker -> UseImageMarker

}


} //end namespace


//A structure to store if we should use the image at a particular frame