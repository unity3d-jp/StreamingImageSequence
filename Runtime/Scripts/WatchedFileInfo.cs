using System;

namespace UnityEngine.StreamingImageSequence {

[Serializable]
internal class WatchedFileInfo {
    internal WatchedFileInfo(string name, long size) {
        m_name = name;
        m_size = size;

    }
//----------------------------------------------------------------------------------------------------------------------    
    
    internal string GetName() { return m_name; }
    internal long GetSize() { return m_size; }
    
//----------------------------------------------------------------------------------------------------------------------
    
    
    
//----------------------------------------------------------------------------------------------------------------------    
    
    [SerializeField] private string m_name;
    [SerializeField] private long   m_size;
}
} //end namespace


