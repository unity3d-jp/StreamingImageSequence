using System;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine.Assertions;
using UnityEngine.Timeline;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
internal class TimelineClipSISData {

    internal void Init(StreamingImageSequenceTrack track) {
        m_trackOwner = track;
    }

//----------------------------------------------------------------------------------------------------------------------
    internal void Destroy() {
    }
    
    
//----------------------------------------------------------------------------------------------------------------------    
    //Should be TimelineClip instead of StreamingImageSequencePlayableAsset
    internal void CreatePlayableFrame(StreamingImageSequencePlayableAsset playableAsset, int index) {
        Assert.IsTrue(null!=m_playableFrames && index < m_playableFrames.Count);

        PlayableFrame playableFrame = ObjectUtility.CreateScriptableObjectInstance<PlayableFrame>();
#if UNITY_EDITOR                    
        AssetDatabase.AddObjectToAsset(playableFrame, playableAsset);
#endif
        double timePerFrame = TimelineUtility.CalculateTimePerFrame(playableAsset.GetBoundTimelineClip());
        playableFrame.Init(playableAsset, timePerFrame * index, playableAsset.GetUseImageMarkerVisibility());
        m_playableFrames[index] = playableFrame;
    }

//----------------------------------------------------------------------------------------------------------------------    
    
    internal void ResetPlayableFrames(StreamingImageSequencePlayableAsset playableAsset) {

        DestroyPlayableFrames();

        //Recalculate the number of frames and create the marker's ground truth data
        int numFrames = TimelineUtility.CalculateNumFrames(playableAsset.GetBoundTimelineClip());
        m_playableFrames = new List<PlayableFrame>(numFrames);
        UpdatePlayableFramesSize(playableAsset, numFrames);
    }

//----------------------------------------------------------------------------------------------------------------------
    private void DestroyPlayableFrames() {
        if (null == m_playableFrames)
            return;
        
        foreach (PlayableFrame frame in m_playableFrames) {
            if (null == frame)
                continue;
            ObjectUtility.Destroy(frame);
        }        
    }
    
//----------------------------------------------------------------------------------------------------------------------
    internal PlayableFrame GetPlayableFrame(int index) {
        Assert.IsTrue(null!=m_playableFrames && index < m_playableFrames.Count);
        return m_playableFrames[index];
    }


//----------------------------------------------------------------------------------------------------------------------
    
    private void UpdatePlayableFramesSize(StreamingImageSequencePlayableAsset playableAsset, int reqPlayableFramesSize) {

        //Resize m_playableFrames
        if (m_playableFrames.Count < reqPlayableFramesSize) {
            int             numNewPlayableFrames = (reqPlayableFramesSize - m_playableFrames.Count);
            PlayableFrame[] newPlayableFrames    = new PlayableFrame[numNewPlayableFrames];
            m_playableFrames.AddRange(newPlayableFrames);                
        }

        if (m_playableFrames.Count > reqPlayableFramesSize) {
            int numLastPlayableFrames = m_playableFrames.Count;
            for (int i = reqPlayableFramesSize; i < numLastPlayableFrames; ++i) {
                PlayableFrame curFrame = m_playableFrames[i];
                if (null == curFrame)
                    continue;
                ObjectUtility.Destroy(curFrame);                
            }
            m_playableFrames.RemoveRange(reqPlayableFramesSize, numLastPlayableFrames - reqPlayableFramesSize);
        }
            
        Assert.IsTrue(m_playableFrames.Count == reqPlayableFramesSize);

        double timePerFrame = TimelineUtility.CalculateTimePerFrame(playableAsset.GetBoundTimelineClip());
            
        for (int i = 0; i < reqPlayableFramesSize; ++i) {
            PlayableFrame curPlayableFrame = m_playableFrames[i];
                
            if (null == curPlayableFrame) {
                CreatePlayableFrame(playableAsset, i);
            }
            else {
                m_playableFrames[i].Init(playableAsset, timePerFrame * i, playableAsset.GetUseImageMarkerVisibility());
                
            }
        }
            
            
    }

//----------------------------------------------------------------------------------------------------------------------    
    
    //Need to split the PlayableFrames which are currently shared by both this and m_clonedFromAsset
    internal void TrySplitPlayableFrames(TimelineClipSISData otherSISData, double otherClipStart, double otherClipDuration) {

        // TimelineClip clip = m_playableAsset.GetBoundTimelineClip();
        // int numIdealFrames = TimelineUtility.CalculateNumFrames(clip);
        //
        // List<PlayableFrame> prevPlayableFrames = m_playableFrames;
        // m_playableFrames = new List<PlayableFrame>(numIdealFrames);
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
        // List<PlayableFrame> prevPlayableFrames = m_playableFrames;
        // if (startIndex + count > prevPlayableFrames.Count) {
        //     Debug.LogWarning("StreamingImageSequencePlayableAsset::ReassignPlayableFrames() Invalid params. "
        //         + " StartIndex: " + startIndex +  ", Count: " + count                
        //     );
        //     return;
        // }
        //     
        // m_playableFrames = new List<PlayableFrame>(numIdealFrames);
        // m_playableFrames.AddRange(prevPlayableFrames.GetRange(startIndex,numIdealFrames));
        //
        // double timePerFrame = CalculateTimePerFrame(m_boundTimelineClip);
        //     
        // //Reinitialize to set the time
        // for (int i = 0; i < numIdealFrames; ++i) {
        //     m_playableFrames[i].Init(m_playableAsset, timePerFrame * i, m_playableAsset.GetUseImageMarkerVisibility());
        // }
            
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    //The ground truth for using/dropping an image in a particular frame. See the notes below
    [SerializeField] private List<PlayableFrame> m_playableFrames;

    private StreamingImageSequenceTrack m_trackOwner = null;

}


} //end namespace


//[Note-Sin: 2020-7-15] PlayableFrame
//StreamingImageSequenceTrack owns PlayableFrame, which is associated with a TimelineClip.
//PlayableFrame is a ScriptableObject and owns UseImageMarker.
