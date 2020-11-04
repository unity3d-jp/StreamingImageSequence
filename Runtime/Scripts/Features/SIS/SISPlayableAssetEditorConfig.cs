using UnityEngine;

namespace Unity.StreamingImageSequence {

[System.Serializable]
internal class SISPlayableAssetEditorConfig  {

    internal void  SetTimelineBGColor(Color color) { m_timelineBGColor = color; }
    internal Color GetTimelineBGColor()            { return m_timelineBGColor; }
    
//----------------------------------------------------------------------------------------------------------------------
    
    [HideInInspector][SerializeField] private Color m_timelineBGColor = Color.gray;
    

}

} //end namespace


