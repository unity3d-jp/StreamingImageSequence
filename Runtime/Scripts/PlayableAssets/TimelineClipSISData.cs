using System;
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
        
        m_frameMarkersVisibility = other.m_frameMarkersVisibility;
        
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
    
    internal bool AreFrameMarkersVisible() {  return m_frameMarkersVisibility; }

    internal void ShowFrameMarkers(bool show) {

        if (show == m_frameMarkersVisibility)
            return;
        
#if UNITY_EDITOR        
        Undo.RegisterFullObjectHierarchyUndo( m_clipOwner.parentTrack, "StreamingImageSequence Show/Hide FrameMarker");
#endif        
        m_frameMarkersVisibility = show;        
        RefreshPlayableFrames();        
    }

    internal void SetOwner(TimelineClip clip) { m_clipOwner = clip;}
    
    internal TimelineClip GetOwner() { return m_clipOwner; }
    
//----------------------------------------------------------------------------------------------------------------------    
    private static SISPlayableFrame CreatePlayableFrame(TimelineClipSISData owner, int index, double timePerFrame) 
    {
        SISPlayableFrame playableFrame = new SISPlayableFrame(owner);
        playableFrame.SetLocalTime(timePerFrame * index);
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
    internal void SetAllPlayableFrames(bool used) {
        foreach (SISPlayableFrame playableFrame in m_playableFrames) {
            playableFrame.SetUsed(used);
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
        double timePerFrame = TimelineUtility.CalculateTimePerFrame(m_clipOwner);                
        int numPlayableFrames = m_playableFrames.Count;
        for (int i = 0; i < numPlayableFrames; ++i) {                
            m_playableFrames[i].SetLocalTime(i * timePerFrame);
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
                
            m_playableFrames[i].SetLocalTime( timePerFrame * i);
            
        }
            
            
    }
   
    
//----------------------------------------------------------------------------------------------------------------------    
    
    //The ground truth for using/dropping an image in a particular frame. See the notes below
    [SerializeField] private List<SISPlayableFrame> m_playableFrames;
    [FormerlySerializedAs("m_useImageMarkerVisibility")] [SerializeField] [HideInInspector] private bool m_frameMarkersVisibility = false;

    [NonSerialized] private TimelineClip  m_clipOwner = null;

}


} //end namespace


//[Note-Sin: 2020-7-15] SISPlayableFrame
//StreamingImageSequenceTrack owns SISPlayableFrame, which is associated with a TimelineClip.
//SISPlayableFrame is a ScriptableObject and owns FrameMarker.
