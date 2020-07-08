namespace UnityEngine.StreamingImageSequence {


internal abstract class BaseImageLoadBGTask : BackGroundTask{


//----------------------------------------------------------------------------------------------------------------------
    internal  BaseImageLoadBGTask( string imagePath, int requestFrame) {
        m_imagePath = imagePath;
        m_requestFrame = requestFrame;
    }
//----------------------------------------------------------------------------------------------------------------------
    
    public override int GetHashCode() {
        return m_imagePath.GetHashCode();
    }

//----------------------------------------------------------------------------------------------------------------------
    internal string GetImagePath()                     { return m_imagePath; }
    
    internal void SetRequestFrame(int requestFrame)    { m_requestFrame = requestFrame; } 
    internal int GetRequestFrame()                     { return m_requestFrame; } 

    
//----------------------------------------------------------------------------------------------------------------------
    private readonly string m_imagePath;
    private int m_requestFrame;

}

} //end namespace
