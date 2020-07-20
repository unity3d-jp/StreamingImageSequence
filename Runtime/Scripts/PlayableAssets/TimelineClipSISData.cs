using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

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
        
        m_useImageMarkerVisibility = other.m_useImageMarkerVisibility;
        
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
    
    internal bool GetUseImageMarkerVisibility() {  return m_useImageMarkerVisibility; }

    internal void SetUseImageMarkerVisibility(bool show) { m_useImageMarkerVisibility = show; }

    internal void SetOwner(TimelineClip clip) { m_clipOwner = clip;}
    
    internal TimelineClip GetOwner() { return m_clipOwner; }
    
//----------------------------------------------------------------------------------------------------------------------    
    private void CreatePlayableFrame(int index) {
        Assert.IsTrue(null!=m_playableFrames && index < m_playableFrames.Count);

        SISPlayableFrame playableFrame = new SISPlayableFrame(this);
        double timePerFrame = TimelineUtility.CalculateTimePerFrame(m_clipOwner);
        playableFrame.Init(this, timePerFrame * index, m_useImageMarkerVisibility);
        m_playableFrames[index] = playableFrame;
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
    private void DestroyPlayableFrames() {
        if (null == m_playableFrames)
            return;
        
        foreach (SISPlayableFrame frame in m_playableFrames) {
            frame?.Destroy();
        }        
    } 
    
//----------------------------------------------------------------------------------------------------------------------
    
    //Resize PlayableFrames and used the previous values
    internal void ResizePlayableFrames() {
        int numIdealNumPlayableFrames = TimelineUtility.CalculateNumFrames(m_clipOwner);
      
        //Change the size of m_playableFrames and reinitialize if necessary
        int prevNumPlayableFrames = m_playableFrames.Count;
        if (numIdealNumPlayableFrames != prevNumPlayableFrames) {
#if UNITY_EDITOR
//            Undo.RegisterCompleteObjectUndo(playableAsset, "StreamingImageSequencePlayableAsset: Updating SISPlayableFrame List");
#endif                
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
        int numPlayableFrames = m_playableFrames.Count;
        for (int i = 0; i < numPlayableFrames; ++i) {                
            m_playableFrames[i].Refresh(m_useImageMarkerVisibility);                
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

        //Resize m_playableFrames
        if (m_playableFrames.Count < reqPlayableFramesSize) {
            int             numNewPlayableFrames = (reqPlayableFramesSize - m_playableFrames.Count);
            SISPlayableFrame[] newPlayableFrames    = new SISPlayableFrame[numNewPlayableFrames];
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

        double timePerFrame = TimelineUtility.CalculateTimePerFrame(m_clipOwner);
            
        for (int i = 0; i < reqPlayableFramesSize; ++i) {
            SISPlayableFrame curPlayableFrame = m_playableFrames[i];
                
            if (null == curPlayableFrame) {
                CreatePlayableFrame(i);
            }
            else {
                m_playableFrames[i].Init(this, timePerFrame * i, m_useImageMarkerVisibility);
                
            }
        }
            
            
    }
   
    
//----------------------------------------------------------------------------------------------------------------------    
    
    //The ground truth for using/dropping an image in a particular frame. See the notes below
    [SerializeField] private List<SISPlayableFrame> m_playableFrames;
    [SerializeField] [HideInInspector] private bool m_useImageMarkerVisibility = false;

    [NonSerialized] private TimelineClip  m_clipOwner = null;

}


} //end namespace


//[Note-Sin: 2020-7-15] SISPlayableFrame
//StreamingImageSequenceTrack owns SISPlayableFrame, which is associated with a TimelineClip.
//SISPlayableFrame is a ScriptableObject and owns UseImageMarker.
