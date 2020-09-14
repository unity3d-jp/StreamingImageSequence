﻿using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.Serialization;
using UnityEngine.Timeline;

#if UNITY_EDITOR
using UnityEditor;
#endif



namespace UnityEngine.StreamingImageSequence {

[Serializable]
internal class TimelineClipSISData : ISerializationCallbackReceiver {

    internal TimelineClipSISData(TimelineClip owner) {
        m_clipOwner = owner;
        int numFrames = TimelineUtility.CalculateNumFrames(m_clipOwner);
        m_playableFrames = new List<SISPlayableFrame>(numFrames);
    }

    internal TimelineClipSISData(TimelineClip owner, TimelineClipSISData other) : this(owner){
        Assert.IsNotNull(m_playableFrames);
        
        foreach (SISPlayableFrame otherFrame in other.m_playableFrames) {
            SISPlayableFrame newFrame = new SISPlayableFrame(this, otherFrame);
            m_playableFrames.Add(newFrame);
        }
        
        m_frameMarkersRequested = other.m_frameMarkersRequested;
        
    }
    
//----------------------------------------------------------------------------------------------------------------------
    #region ISerializationCallbackReceiver
    public void OnBeforeSerialize() {
    }

    public void OnAfterDeserialize() {
        foreach (SISPlayableFrame playableFrame in m_playableFrames) {
            playableFrame.SetOwner(this);
        }
    }    
    #endregion
//----------------------------------------------------------------------------------------------------------------------
    internal void Destroy() {

        foreach (SISPlayableFrame playableFrame in m_playableFrames) {
            playableFrame.Destroy();
        }

    }
    

//----------------------------------------------------------------------------------------------------------------------

    internal bool AreFrameMarkersRequested() {
        return m_frameMarkersRequested;
    }

    internal void RequestFrameMarkers(bool req, bool forceShow = false) {

        if (req == m_frameMarkersRequested)
            return;
        
#if UNITY_EDITOR        
        Undo.RegisterFullObjectHierarchyUndo( m_clipOwner.parentTrack, "StreamingImageSequence Show/Hide FrameMarker");        
        m_forceShowFrameMarkers = forceShow && req;
#endif        
        m_frameMarkersRequested = req;
        if (UpdateFrameMarkersVisibility()) {
            RefreshPlayableFrames();                    
        }
    }

    internal void SetOwner(TimelineClip clip) { m_clipOwner = clip;}
    
    internal TimelineClip GetOwner() { return m_clipOwner; }

#if UNITY_EDITOR
    internal void SetInspectedProperty(PlayableFramePropertyID id) { m_inspectedPropertyID = id; }

    internal PlayableFramePropertyID GetInspectedProperty() { return m_inspectedPropertyID; }

    internal void UpdateTimelineWidthPerFrame(float visibleRectWidth, double visibleTime, float fps, double timeScale) {
        int numFrames = Mathf.RoundToInt((float)
            ((visibleTime) * fps / timeScale) 
        );
                
        double widthPerFrame       = visibleRectWidth / numFrames;
        m_timelineWidthPerFrame = widthPerFrame;
        if (UpdateFrameMarkersVisibility()) {
            RefreshPlayableFrames();
            
        }                
    }
    
#endif    
    
//----------------------------------------------------------------------------------------------------------------------    
    private static SISPlayableFrame CreatePlayableFrame(TimelineClipSISData owner, int index, double timePerFrame) 
    {
        SISPlayableFrame playableFrame = new SISPlayableFrame(owner);
        playableFrame.SetIndexAndLocalTime(index, timePerFrame * index);
        return playableFrame;
    }

//----------------------------------------------------------------------------------------------------------------------    
    
    internal void ResetPlayableFrames() {
        DestroyPlayableFrames();

        //Recalculate the number of frames and create the marker's ground truth data
        int numFrames = TimelineUtility.CalculateNumFrames(m_clipOwner);
        m_playableFrames = new List<SISPlayableFrame>(numFrames);
        UpdatePlayableFramesSize(numFrames);                
    }

//----------------------------------------------------------------------------------------------------------------------
    internal void SetAllPlayableFramesProperty(PlayableFramePropertyID id, bool val) {
        foreach (SISPlayableFrame playableFrame in m_playableFrames) {
            playableFrame.SetBoolProperty(id, val);
        }
    }

//----------------------------------------------------------------------------------------------------------------------
    private void DestroyPlayableFrames() {
        if (null == m_playableFrames)
            return;
        
        foreach (SISPlayableFrame frame in m_playableFrames) {
            frame?.Destroy();
        }        
    } 
    
//----------------------------------------------------------------------------------------------------------------------
    
    //Resize PlayableFrames and used the previous values
    internal void RefreshPlayableFrames() {
        
        //Clip doesn't have parent. Might be because the clip is being moved 
        if (null == m_clipOwner.parentTrack) {
            return;
        }        
        
        int numIdealNumPlayableFrames = TimelineUtility.CalculateNumFrames(m_clipOwner);
      
        //Change the size of m_playableFrames and reinitialize if necessary
        int prevNumPlayableFrames = m_playableFrames.Count;
        if (numIdealNumPlayableFrames != prevNumPlayableFrames) {
            
            //Change the size of m_playableFrames and reinitialize if necessary
            List<bool> prevUsedFrames = new List<bool>(prevNumPlayableFrames);
            foreach (SISPlayableFrame frame in m_playableFrames) {
                prevUsedFrames.Add(null == frame || frame.IsUsed()); //if frame ==null, just regard as used.
            }

            UpdatePlayableFramesSize(numIdealNumPlayableFrames);
            
            //Reinitialize 
            if (prevNumPlayableFrames > 0) {
                for (int i = 0; i < numIdealNumPlayableFrames; ++i) {
                    int prevIndex = (int)(((float)(i) / numIdealNumPlayableFrames) * prevNumPlayableFrames);
                    m_playableFrames[i].SetUsed(prevUsedFrames[prevIndex]);
                }
            }
            
        }
        
        //Refresh all markers
        double timePerFrame           = TimelineUtility.CalculateTimePerFrame(m_clipOwner);                
        int    numPlayableFrames      = m_playableFrames.Count;
        for (int i = 0; i < numPlayableFrames; ++i) {                
            m_playableFrames[i].SetIndexAndLocalTime(i, i * timePerFrame);
            m_playableFrames[i].Refresh(m_frameMarkersVisibility);
        }
        
    }        
    
    
//----------------------------------------------------------------------------------------------------------------------
    //may return null
    internal SISPlayableFrame GetPlayableFrame(int index) {
        if (null == m_playableFrames || index >= m_playableFrames.Count)
            return null;
        
        Assert.IsTrue(null!=m_playableFrames && index < m_playableFrames.Count);
        return m_playableFrames[index];
    }


//----------------------------------------------------------------------------------------------------------------------
    
    private void UpdatePlayableFramesSize(int reqPlayableFramesSize) {
        Assert.IsNotNull(m_clipOwner);

        double timePerFrame = TimelineUtility.CalculateTimePerFrame(m_clipOwner);
        //Resize m_playableFrames
        if (m_playableFrames.Count < reqPlayableFramesSize) {
            int             numNewPlayableFrames = (reqPlayableFramesSize - m_playableFrames.Count);
            List<SISPlayableFrame> newPlayableFrames = new List<SISPlayableFrame>(numNewPlayableFrames);           
            for (int i = m_playableFrames.Count; i < reqPlayableFramesSize; ++i) {
                newPlayableFrames.Add(CreatePlayableFrame(this,i, timePerFrame));
            }            
            m_playableFrames.AddRange(newPlayableFrames);                
        }

        if (m_playableFrames.Count > reqPlayableFramesSize) {
            int numLastPlayableFrames = m_playableFrames.Count;
            for (int i = reqPlayableFramesSize; i < numLastPlayableFrames; ++i) {
                SISPlayableFrame curFrame = m_playableFrames[i];
                curFrame?.Destroy();
            }            
            m_playableFrames.RemoveRange(reqPlayableFramesSize, numLastPlayableFrames - reqPlayableFramesSize);
        }
            
        Assert.IsTrue(m_playableFrames.Count == reqPlayableFramesSize);
           
        for (int i = 0; i < reqPlayableFramesSize; ++i) {
            SISPlayableFrame curPlayableFrame = m_playableFrames[i];
            Assert.IsNotNull(curPlayableFrame);                
            m_playableFrames[i].SetIndexAndLocalTime(i, timePerFrame * i);
            
        }                        
    }

//----------------------------------------------------------------------------------------------------------------------

    //return true if the visibility has changed
    private bool  UpdateFrameMarkersVisibility() {
        
        bool prevVisibility = m_frameMarkersVisibility;
#if UNITY_EDITOR        
        const int FRAME_MARKER_WIDTH_THRESHOLD = 20;
        m_frameMarkersVisibility = m_frameMarkersRequested && 
            (m_forceShowFrameMarkers || m_timelineWidthPerFrame > FRAME_MARKER_WIDTH_THRESHOLD);
#else
        m_frameMarkersVisibility = m_frameMarkersRequested;
#endif
        return prevVisibility != m_frameMarkersVisibility;
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    //The ground truth for using/dropping an image in a particular frame. See the notes below
    [SerializeField] private List<SISPlayableFrame> m_playableFrames;
    [FormerlySerializedAs("m_frameMarkersVisibility")] [SerializeField] [HideInInspector] private bool m_frameMarkersRequested = false;

    [NonSerialized] private TimelineClip  m_clipOwner = null;

#pragma warning disable 414    
    [HideInInspector][SerializeField] private int m_version = CUR_TIMELINE_CLIP_SIS_DATA_VERSION;        
#pragma warning restore 414    
    
#if UNITY_EDITOR    
    private PlayableFramePropertyID m_inspectedPropertyID   = PlayableFramePropertyID.USED;
    private double                  m_timelineWidthPerFrame = Int16.MaxValue;
    private bool                    m_forceShowFrameMarkers = false;
#endif

    private       bool   m_frameMarkersVisibility           = false;
    
    private const int    CUR_TIMELINE_CLIP_SIS_DATA_VERSION = 1;
    
}


} //end namespace


//[Note-Sin: 2020-7-15] SISPlayableFrame
//StreamingImageSequenceTrack owns SISPlayableFrame, which is associated with a TimelineClip.
//SISPlayableFrame is a ScriptableObject and owns FrameMarker.
