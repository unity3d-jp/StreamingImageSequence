using Unity.AnimeToolbox;
using UnityEngine;

namespace Unity.StreamingImageSequence {

internal static class RuntimeTextures {

   
//----------------------------------------------------------------------------------------------------------------------

    internal static Texture GetTransparentTexture() {
        if (m_transparentTexture.IsNullRef()) {
            LoadTextures();
        }
        return m_transparentTexture;
    }
    
    
//----------------------------------------------------------------------------------------------------------------------

    static void LoadTextures() {
        
        m_transparentTexture = new Texture2D (1, 1, TextureFormat.ARGB32, false); 
        m_transparentTexture.SetPixel(0,0, Color.clear);
        m_transparentTexture.Apply();            
    }
    
//----------------------------------------------------------------------------------------------------------------------

    private static Texture2D m_transparentTexture = null;

}

} //end namespace