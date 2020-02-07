using System;

namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
public class ImageAtFrameInfo {

    internal ImageAtFrameInfo(double localTime) {
        m_useImage = true;
        m_localTime = localTime;
    }
    
//----------------------------------------------------------------------------------------------------------------------
    internal bool IsUsed() { return m_useImage; }
    internal void SetUsed(bool used) { m_useImage = used; }
    internal double GetLocalTime() { return m_localTime; }

//----------------------------------------------------------------------------------------------------------------------

    [SerializeField] private bool m_useImage;
    [SerializeField] private double m_localTime;

}


} //end namespace


//A structure to store if we should use the image at a particular frame