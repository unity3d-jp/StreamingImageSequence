using UnityEngine;

namespace Unity.StreamingImageSequence {

[System.Serializable]
internal class RenderCachePlayableAssetEditorConfig {
    
//----------------------------------------------------------------------------------------------------------------------

    internal void  SetUpdateBGColor(Color color) { m_updateBGColor = color; }
    internal Color GetUpdateBGColor()            { return m_updateBGColor; }
         
//----------------------------------------------------------------------------------------------------------------------
    
    [HideInInspector][SerializeField] private Color m_updateBGColor = Color.black;
}


} //end namespace

