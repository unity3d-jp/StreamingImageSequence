using Unity.AnimeToolbox;
using UnityEngine;

namespace Unity.StreamingImageSequence {

internal static class RuntimeTextures {

   
//----------------------------------------------------------------------------------------------------------------------

    internal static Texture2D GetTransparentTexture() {
        if (!m_transparentTexture.IsNullRef()) {
            return m_transparentTexture;
        }
        m_transparentTexture = new Texture2D (1, 1, TextureFormat.ARGB32, false); 
        m_transparentTexture.SetPixel(0,0, Color.clear);
        m_transparentTexture.Apply();            
        return m_transparentTexture;
    }
    
//----------------------------------------------------------------------------------------------------------------------

    private static Texture2D m_transparentTexture = null;

}

} //end namespace