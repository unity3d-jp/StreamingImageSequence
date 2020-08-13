using System;

namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
internal class PlayableFrameProperty<T> where T: struct{
    
    PlayableFrameProperty(string name, T val) {
        m_propertyName = name;
        m_propertyValue = val;
    }
//----------------------------------------------------------------------------------------------------------------------

    internal string GetName() { return m_propertyName; }
    internal T GetValue() { return m_propertyValue;}
//----------------------------------------------------------------------------------------------------------------------
    
    
    private string m_propertyName = null;
    private T m_propertyValue = default(T);
    
}

} //end namespace

