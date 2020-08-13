using System;

namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
internal class PlayableFrameBoolProperty{
    
    internal PlayableFrameBoolProperty(string name, bool val) {
        m_propertyName  = name;
        m_propertyValue = val;
    }
//----------------------------------------------------------------------------------------------------------------------

    internal string GetName() { return m_propertyName; }
    internal bool GetValue() { return m_propertyValue;}

//----------------------------------------------------------------------------------------------------------------------
    
    
    [SerializeField] private string m_propertyName = null;
    [SerializeField] private bool m_propertyValue = false;
    
}

} //end namespace

