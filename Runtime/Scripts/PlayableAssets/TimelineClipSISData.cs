using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
internal class TimelineClipSISData : ISerializationCallbackReceiver {

    internal void Init(StreamingImageSequenceTrack track, TimelineClip clip) {
        m_trackOwner = track;
        m_clipOwner = clip;
        
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
    }
    

//----------------------------------------------------------------------------------------------------------------------
    
    internal bool GetUseImageMarkerVisibility() {  return m_useImageMarkerVisibility; }

    internal void SetUseImageMarkerVisibility(bool show) { m_useImageMarkerVisibility = show; }

    internal StreamingImageSequenceTrack GetTrackOwner() {
        return m_trackOwner;
    }
    internal TimelineClip GetClipOwner() { return m_clipOwner; }
    
//----------------------------------------------------------------------------------------------------------------------    
    private void CreatePlayableFrame(int index) {
        Assert.IsTrue(null!=m_playableFrames && index < m_playableFrames.Count);

        SISPlayableFrame playableFrame = new SISPlayableFrame();
        double timePerFrame = TimelineUtility.CalculateTimePerFrame(m_trackOwner);
        playableFrame.Init(this, timePerFrame * index, m_useImageMarkerVisibility);
        m_playableFrames[index] = playableFrame;
    }

//----------------------------------------------------------------------------------------------------------------------    
    
    internal void ResetPlayableFrames() {

        //Recalculate the number of frames and create the marker's ground truth data
        int numFrames = TimelineUtility.CalculateNumFrames(m_clipOwner);
        m_playableFrames = new List<SISPlayableFrame>(numFrames);
        UpdatePlayableFramesSize(numFrames);
                
    }

//----------------------------------------------------------------------------------------------------------------------    
    
    //Resize PlayableFrames and used the previous values
    internal void RefreshPlayableFrames() {

        //if this asset was a cloned asset, split the playable frames
        // if (null != m_clonedFromAsset) {
        //     TrySplitPlayableFrames(numIdealNumPlayableFrames);
        //     m_clonedFromAsset = null;
        // }
        
        if (null == m_playableFrames) {
            ResetPlayableFrames();
        }
        else {
            ResizePlayableFrames();
        }        
        
    }        

//----------------------------------------------------------------------------------------------------------------------
    
    //Resize PlayableFrames and used the previous values
    private void ResizePlayableFrames() {
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

        //Resize m_playableFrames
        if (m_playableFrames.Count < reqPlayableFramesSize) {
            int             numNewPlayableFrames = (reqPlayableFramesSize - m_playableFrames.Count);
            SISPlayableFrame[] newPlayableFrames    = new SISPlayableFrame[numNewPlayableFrames];
            m_playableFrames.AddRange(newPlayableFrames);                
        }

        if (m_playableFrames.Count > reqPlayableFramesSize) {
            int numLastPlayableFrames = m_playableFrames.Count;
            m_playableFrames.RemoveRange(reqPlayableFramesSize, numLastPlayableFrames - reqPlayableFramesSize);
        }
            
        Assert.IsTrue(m_playableFrames.Count == reqPlayableFramesSize);

        double timePerFrame = TimelineUtility.CalculateTimePerFrame(m_trackOwner);
            
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
    
    //Need to split the PlayableFrames which are currently shared by both this and m_clonedFromAsset
    internal void TrySplitPlayableFrames(TimelineClipSISData otherSISData, double otherClipStart, double otherClipDuration) {

        // TimelineClip clip = m_playableAsset.GetBoundTimelineClip();
        // int numIdealFrames = TimelineUtility.CalculateNumFrames(clip);
        //
        // List<SISPlayableFrame> prevPlayableFrames = m_playableFrames;
        // m_playableFrames = new List<SISPlayableFrame>(numIdealFrames);
        // int prevNumPlayableFrames = prevPlayableFrames.Count;
        //
        // //Check if this clone is a pure duplicate                   
        // if (Math.Abs(clip.duration - otherClipDuration) < 0.0000001f) {
        //     for (int i = 0; i < prevNumPlayableFrames; ++i) {
        //         m_playableFrames.Add(null);
        //         CreatePlayableFrame( i);
        //         m_playableFrames[i].SetUsed(prevPlayableFrames[i].IsUsed());
        //     }
        //     return;
        // }
        //
        // //Decide which one is on the left side after splitting
        // if (clip.start < otherClipStart) {
        //     m_playableFrames.AddRange(prevPlayableFrames.GetRange(0,numIdealFrames));
        //     m_clonedFromAsset.SplitPlayableFramesFromClonedAsset(numIdealFrames,prevPlayableFrames.Count - numIdealFrames);
        // } else {
        //     int idx = prevPlayableFrames.Count - numIdealFrames;
        //     m_playableFrames.AddRange(prevPlayableFrames.GetRange(idx, idx + numIdealFrames -1));
        //     m_clonedFromAsset.SplitPlayableFramesFromClonedAsset(0,idx);
        // }
        //
        // //Reinitialize to assign the owner
        // double timePerFrame = CalculateTimePerFrame(m_boundTimelineClip);
        // for (int i = 0; i < numIdealFrames; ++i) {
        //     m_playableFrames[i].Init(m_playableAsset, timePerFrame * i, m_useImageMarkerVisibility);
        // }
        
    }

//----------------------------------------------------------------------------------------------------------------------    
    
    //This is called by the cloned asset
    void SplitPlayableFramesFromClonedAsset(int startIndex, int count) {

        // int numIdealFrames = CalculateIdealNumPlayableFrames(m_boundTimelineClip);
        // if (numIdealFrames != count) {
        //     Debug.LogWarning("StreamingImageSequencePlayableAsset::ReassignPlayableFrames() Count: " + count
        //         + " is not ideal: " + numIdealFrames                
        //     );
        //     return;
        // }
        //
        // List<SISPlayableFrame> prevPlayableFrames = m_playableFrames;
        // if (startIndex + count > prevPlayableFrames.Count) {
        //     Debug.LogWarning("StreamingImageSequencePlayableAsset::ReassignPlayableFrames() Invalid params. "
        //         + " StartIndex: " + startIndex +  ", Count: " + count                
        //     );
        //     return;
        // }
        //     
        // m_playableFrames = new List<SISPlayableFrame>(numIdealFrames);
        // m_playableFrames.AddRange(prevPlayableFrames.GetRange(startIndex,numIdealFrames));
        //
        // double timePerFrame = CalculateTimePerFrame(m_boundTimelineClip);
        //     
        // //Reinitialize to set the time
        // for (int i = 0; i < numIdealFrames; ++i) {
        //     m_playableFrames[i].Init(m_playableAsset, timePerFrame * i, m_useImageMarkerVisibility);
        // }
            
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    //The ground truth for using/dropping an image in a particular frame. See the notes below
    [SerializeField] private List<SISPlayableFrame> m_playableFrames;
    [SerializeField] [HideInInspector] private bool m_useImageMarkerVisibility = false;

    private StreamingImageSequenceTrack m_trackOwner = null;
    private TimelineClip  m_clipOwner = null;

}


} //end namespace


//[Note-Sin: 2020-7-15] SISPlayableFrame
//StreamingImageSequenceTrack owns SISPlayableFrame, which is associated with a TimelineClip.
//SISPlayableFrame is a ScriptableObject and owns UseImageMarker.
