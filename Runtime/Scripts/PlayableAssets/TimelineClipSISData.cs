using System;
using System.Collections.Generic;
using UnityEditor.Timeline;
using UnityEngine.Assertions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
internal class TimelineClipSISData {

    internal void Init(StreamingImageSequencePlayableAsset sisPlayableAsset) {
        m_playableAsset = sisPlayableAsset;
    }

//----------------------------------------------------------------------------------------------------------------------
    
    internal void Reset() {

        Destroy();

        //Recalculate the number of frames and create the marker's ground truth data
        int numFrames = StreamingImageSequencePlayableAsset.CalculateIdealNumPlayableFrames(m_playableAsset.GetBoundTimelineClip());
        m_playableFrames = new List<PlayableFrame>(numFrames);
        UpdatePlayableFramesSize(numFrames);
    }

//----------------------------------------------------------------------------------------------------------------------    
    internal void Destroy() {
        if (null == m_playableFrames)
            return;
        
        foreach (PlayableFrame frame in m_playableFrames) {
            if (null == frame)
                continue;
            ObjectUtility.Destroy(frame);
        }        
    }
    
    
//----------------------------------------------------------------------------------------------------------------------    
    internal void CreatePlayableFrame(int index) {
        Assert.IsTrue(null!=m_playableFrames && index < m_playableFrames.Count);

        PlayableFrame playableFrame = ObjectUtility.CreateScriptableObjectInstance<PlayableFrame>();
#if UNITY_EDITOR                    
        AssetDatabase.AddObjectToAsset(playableFrame, m_playableAsset);
#endif
        double timePerFrame = TimelineUtility.CalculateTimePerFrame(m_playableAsset.GetBoundTimelineClip());
        playableFrame.Init(m_playableAsset, timePerFrame * index, m_playableAsset.GetUseImageMarkerVisibility());
        m_playableFrames[index] = playableFrame;
    }
    
    
//----------------------------------------------------------------------------------------------------------------------
    
    private void UpdatePlayableFramesSize(int reqPlayableFramesSize) {

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

        double timePerFrame = StreamingImageSequencePlayableAsset.CalculateTimePerFrame(m_playableAsset.GetBoundTimelineClip());
            
        for (int i = 0; i < reqPlayableFramesSize; ++i) {
            PlayableFrame curPlayableFrame = m_playableFrames[i];
                
            if (null == curPlayableFrame) {
                CreatePlayableFrame(i);
            }
            else {
                m_playableFrames[i].Init(m_playableAsset, timePerFrame * i, m_playableAsset.GetUseImageMarkerVisibility());
                
            }
        }
            
            
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    [SerializeField] private List<PlayableFrame> m_playableFrames;

    private StreamingImageSequencePlayableAsset m_playableAsset = null;
}


} //end namespace


//A structure to store if we should use the image at a particular frame