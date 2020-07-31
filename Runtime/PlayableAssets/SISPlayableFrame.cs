using System;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using UnityEditor.SceneManagement;
#endif

namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
internal class SISPlayableFrame : ISerializationCallbackReceiver {

    internal SISPlayableFrame(TimelineClipSISData owner) {
        m_timelineClipSISDataOwner = owner;        
    }

    internal SISPlayableFrame(TimelineClipSISData owner, SISPlayableFrame otherFrame) {
        m_timelineClipSISDataOwner = owner;
        m_useImage = otherFrame.m_useImage;
        m_localTime = otherFrame.m_localTime;
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

    internal void SetUsed(bool used) {
#if UNITY_EDITOR
        if (m_useImage != used) {
            EditorSceneManager.MarkAllScenesDirty();            
        }
#endif        
        m_useImage = used;
        
    }
    internal double GetLocalTime() { return m_localTime; }
    internal void SetLocalTime(double localTime) {  m_localTime = localTime;}

    internal TimelineClip GetClipOwner() {
        TimelineClip clip = m_timelineClipSISDataOwner?.GetOwner();
        return clip;
    }
    
//----------------------------------------------------------------------------------------------------------------------
    internal void Refresh(bool frameMarkerVisibility) {
        TrackAsset trackAsset = m_timelineClipSISDataOwner.GetOwner()?.parentTrack;
        //Delete Marker first if it's not in the correct track (e.g: after the TimelineClip was moved)
        if (null!= m_marker && m_marker.parent != trackAsset) {
            DeleteMarker();
        }

        //Show/Hide the marker
        if (null != m_marker && !frameMarkerVisibility) {
            DeleteMarker();
        } else if (null == m_marker && null!=trackAsset && frameMarkerVisibility) {
            CreateMarker();
        }

        if (m_marker) {
            TimelineClip clipOwner = m_timelineClipSISDataOwner.GetOwner();
            m_marker.Init(this, clipOwner.start + m_localTime);
        }
    }
//----------------------------------------------------------------------------------------------------------------------

    void CreateMarker() {
        TimelineClip clipOwner = m_timelineClipSISDataOwner.GetOwner();
        TrackAsset trackAsset = clipOwner?.parentTrack;
                       
        Assert.IsNotNull(trackAsset);
        Assert.IsNull(m_marker);
               
        m_marker = trackAsset.CreateMarker<FrameMarker>(m_localTime);
    }

    void DeleteMarker() {
        Assert.IsNotNull(m_marker);
        
        TrackAsset track = m_marker.parent;
        track.DeleteMarker(m_marker);
        
    }
    
//----------------------------------------------------------------------------------------------------------------------

    [SerializeField] private bool m_useImage = true;
    [SerializeField] private double m_localTime;    
    [SerializeField] private FrameMarker m_marker = null; 
    
    [NonSerialized] private TimelineClipSISData m_timelineClipSISDataOwner = null; 

    
}

} //end namespace


//A structure to store if we should use the image at a particular frame