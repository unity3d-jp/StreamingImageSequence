using System;
using System.Collections.Generic;
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
    internal void CreatePlayableFrame(int index) {
        Assert.IsTrue(null!=m_playableFrames && index < m_playableFrames.Count);

        PlayableFrame playableFrame = ObjectUtility.CreateScriptableObjectInstance<PlayableFrame>();
#if UNITY_EDITOR                    
        AssetDatabase.AddObjectToAsset(playableFrame, m_playableAsset);
#endif
        double timePerFrame = StreamingImageSequencePlayableAsset.CalculateTimePerFrame(m_playableAsset.GetBoundTimelineClip());
        playableFrame.Init(m_playableAsset, timePerFrame * index, m_playableAsset.GetUseImageMarkerVisibility());
        m_playableFrames[index] = playableFrame;
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
    
    [SerializeField] private List<PlayableFrame> m_playableFrames;

    private StreamingImageSequencePlayableAsset m_playableAsset = null;
}


} //end namespace


//A structure to store if we should use the image at a particular frame