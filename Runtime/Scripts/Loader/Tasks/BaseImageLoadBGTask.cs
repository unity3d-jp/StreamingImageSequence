namespace Unity.StreamingImageSequence {


internal abstract class BaseImageLoadBGTask : ITask {


//----------------------------------------------------------------------------------------------------------------------
    internal  BaseImageLoadBGTask( string imagePath) {
        m_imagePath = imagePath;
    }
    
//----------------------------------------------------------------------------------------------------------------------
    public abstract void Execute();
    
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
    private int m_requestFrame = 0;

}

} //end namespace
