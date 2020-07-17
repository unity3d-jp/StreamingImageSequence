using System;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
internal class SISPlayableFrame : ISerializationCallbackReceiver {

    internal SISPlayableFrame(TimelineClipSISData owner) {
        m_timelineClipSISDataOwner = owner;        
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    internal void Init(TimelineClipSISData owner, double localTime, bool showMarker) {
        m_timelineClipSISDataOwner = owner;
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

        m_marker.SetOwner(this);
    }    
    #endregion
    

//----------------------------------------------------------------------------------------------------------------------

    internal void Destroy() {
        if (null == m_marker)
            return;

        DeleteMarker();
    }

//----------------------------------------------------------------------------------------------------------------------
    internal void SetOwner(TimelineClipSISData owner) {  m_timelineClipSISDataOwner = owner;}

    internal TimelineClipSISData GetOwner() {  return m_timelineClipSISDataOwner; }
    internal bool IsUsed() { return m_useImage; }
    internal void SetUsed(bool used) { m_useImage = used; }
    internal double GetLocalTime() { return m_localTime; }
    internal void SetLocalTime(double localTime) {  m_localTime = localTime;}

    internal TimelineClip GetClipOwner() {
        TimelineClip clip = m_timelineClipSISDataOwner?.GetOwner();
        return clip;
    }
    
//----------------------------------------------------------------------------------------------------------------------
    internal void Refresh(bool useImageMarkerVisibility) {
        TrackAsset trackAsset = m_timelineClipSISDataOwner.GetOwner()?.parentTrack;
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
            m_marker.SetOwner(this);
        }
    }
//----------------------------------------------------------------------------------------------------------------------

    void CreateMarker() {
        TrackAsset trackAsset = m_timelineClipSISDataOwner.GetOwner()?.parentTrack;        
               
        Assert.IsNotNull(trackAsset);
        Assert.IsNull(m_marker);
        m_marker = trackAsset.CreateMarker<UseImageMarker>(m_localTime);
        m_marker.Init(this, GetClipOwner().start + m_localTime);
    }

    void DeleteMarker() {
        Assert.IsNotNull(m_marker);
        
        TrackAsset track = m_marker.parent;
        track.DeleteMarker(m_marker);
        
    }
    
//----------------------------------------------------------------------------------------------------------------------

    [SerializeField] private bool m_useImage = true;
    [SerializeField] private double m_localTime;    
    [SerializeField] private UseImageMarker m_marker = null; 
    
    [NonSerialized] private TimelineClipSISData m_timelineClipSISDataOwner = null; 

    
}

} //end namespace


//A structure to store if we should use the image at a particular frame