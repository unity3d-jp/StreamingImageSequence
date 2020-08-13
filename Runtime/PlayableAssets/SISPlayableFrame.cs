﻿using System;
using System.Collections.Generic;
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
        m_boolProperties = new Dictionary<string, PlayableFrameBoolProperty>();  
    }

    internal SISPlayableFrame(TimelineClipSISData owner, SISPlayableFrame otherFrame) {
        m_timelineClipSISDataOwner = owner;
        m_boolProperties = otherFrame.m_boolProperties;
        m_localTime = otherFrame.m_localTime;
    }       
    
    
//----------------------------------------------------------------------------------------------------------------------
    #region ISerializationCallbackReceiver
    public void OnBeforeSerialize() {
        if (null != m_boolProperties) {
            m_serializedBoolProperties = new List<PlayableFrameBoolProperty>(m_boolProperties.Count);
            foreach (KeyValuePair<string, PlayableFrameBoolProperty> kv in m_boolProperties) {
                m_serializedBoolProperties.Add(kv.Value);
            }        
            
        } else {
            m_serializedBoolProperties = new List<PlayableFrameBoolProperty>();            
        }
        
    }

    public void OnAfterDeserialize() {
        m_boolProperties = new Dictionary<string, PlayableFrameBoolProperty>();
        if (null != m_serializedBoolProperties) {
            foreach (PlayableFrameBoolProperty prop in m_serializedBoolProperties) {
                string propName = prop.GetName();
                m_boolProperties[propName] = new PlayableFrameBoolProperty(propName, prop.GetValue());
            }            
        } 
        
        if (null == m_marker)
            return;

        m_marker.SetOwner(this);
    }    
    #endregion //ISerializationCallbackReceiver
    

//----------------------------------------------------------------------------------------------------------------------

    internal void Destroy() {
        if (null == m_marker)
            return;

        DeleteMarker();
    }

//----------------------------------------------------------------------------------------------------------------------
    internal void SetOwner(TimelineClipSISData owner) {  m_timelineClipSISDataOwner = owner;}
    internal TimelineClipSISData GetOwner() {  return m_timelineClipSISDataOwner; }
    internal double GetLocalTime()                 { return m_localTime; }
    internal void   SetLocalTime(double localTime) {  m_localTime = localTime;}

    internal TimelineClip GetClipOwner() {
        TimelineClip clip = m_timelineClipSISDataOwner?.GetOwner();
        return clip;
    }

//----------------------------------------------------------------------------------------------------------------------
    //Property
    internal bool GetBoolProperty(string propertyName) {
        if (null!=m_boolProperties && m_boolProperties.ContainsKey(propertyName)) {
            return m_boolProperties[propertyName].GetValue();
        }

        switch (propertyName) {
            case PlayableFramePropertyName.USED: return true;
            case PlayableFramePropertyName.LOCKED: return false;
                default: return false;
        }        
    }
    
    

    internal void SetBoolProperty(string propertyName, bool val) {
#if UNITY_EDITOR        
        if (GetBoolProperty(propertyName) != val) {
            EditorSceneManager.MarkAllScenesDirty();            
        }
#endif        
        m_boolProperties[propertyName] = new PlayableFrameBoolProperty(propertyName, val);
        
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

    [SerializeField] private List<PlayableFrameBoolProperty> m_serializedBoolProperties;
    [SerializeField] private double m_localTime;    
    [SerializeField] private FrameMarker m_marker = null; 
    
    [NonSerialized] private TimelineClipSISData m_timelineClipSISDataOwner = null;

    
    private Dictionary<string, PlayableFrameBoolProperty> m_boolProperties;




}

} //end namespace


//A structure to store if we should use the image at a particular frame