using System;
using System.Collections.Generic;


namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
internal class TimelineClipSISData {
    [SerializeField] private List<PlayableFrame> m_playableFrames;
}


} //end namespace


//A structure to store if we should use the image at a particular frame