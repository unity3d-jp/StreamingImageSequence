using System;
using System.Collections.Generic;


namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
internal class TimelineClipSISData {

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
}


} //end namespace


//A structure to store if we should use the image at a particular frame