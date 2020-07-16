using System;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
internal class SISPlayableFrame : ISerializationCallbackReceiver {

    internal void Init(TimelineClipSISData owner, double localTime, bool showMarker) {
        m_owner = owner;
        m_localTime = localTime;

        if (null == m_marker && showMarker) {
            CreateMarker();
        }
    }
    
//----------------------------------------------------------------------------------------------------------------------
    #region ISerializationCallbackReceiver
    public void OnBeforeSerialize() {
    }

    public void OnAfterDeserialize() {
        if (null == m_marker)
            return;

        m_marker.Init(this);
    }    
    #endregion
    


    ~SISPlayableFrame() {
        if (null == m_marker)
            return;

        DeleteMarker();
    }

//----------------------------------------------------------------------------------------------------------------------
    internal void SetOwner(TimelineClipSISData owner) {  m_owner = owner;}

    internal TimelineClipSISData GetOwner() {  return m_owner; }
    internal bool IsUsed() { return m_useImage; }
    internal void SetUsed(bool used) { m_useImage = used; }
    internal double GetLocalTime() { return m_localTime; }
    internal void SetLocalTime(double localTime) {  m_localTime = localTime;}

//----------------------------------------------------------------------------------------------------------------------
    internal void Refresh(bool useImageMarkerVisibility) {
        TrackAsset trackAsset = m_owner.GetTrackOwner();
        //Delete Marker first if it's not in the correct track (e.g: after the TimelineClip was moved)
        if (null!= m_marker && m_marker.parent != trackAsset) {
            DeleteMarker();
        }

        //Show/Hide the marker
        if (null != m_marker && !useImageMarkerVisibility) {
            DeleteMarker();
        } else if (null == m_marker && null!=trackAsset && useImageMarkerVisibility) {
            CreateMarker();
        }

        if (m_marker) {
            m_marker.Init(this);
            m_marker.Refresh();
        }
    }
//----------------------------------------------------------------------------------------------------------------------

    void CreateMarker() {
        TrackAsset trackAsset = m_owner.GetTrackOwner();        
        Assert.IsNotNull(trackAsset);
        m_marker = trackAsset.CreateMarker<UseImageMarker>(m_localTime);
        m_marker.Init(this);

#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(trackAsset, "SISPlayableFrame: CreateMarker");
#endif        
        
    }

    void DeleteMarker() {
        TrackAsset trackAsset = m_owner.GetTrackOwner();
        
        TrackAsset track = m_marker.parent;
        track.DeleteMarker(m_marker);
#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(trackAsset, "SISPlayableFrame: DeleteMarker");
#endif        
        
    }
    
//----------------------------------------------------------------------------------------------------------------------

    [SerializeField] private bool m_useImage = true;
    [SerializeField] private double m_localTime;
    private UseImageMarker m_marker = null; //ScriptableObject -> Marker -> UseImageMarker

    private TimelineClipSISData m_owner = null; 

}


} //end namespace


//A structure to store if we should use the image at a particular frame