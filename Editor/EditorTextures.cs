using UnityEngine;

namespace UnityEditor.StreamingImageSequence {

internal static class EditorTextures {

    internal static Texture GetCheckedTexture() {
        if (null == m_checkedTexture) {
            LoadTextures();
        }
        return m_checkedTexture;
    }
    
//----------------------------------------------------------------------------------------------------------------------

    [InitializeOnLoadMethod]
    static void OnLoad() {
        LoadTextures();
    }

    
//----------------------------------------------------------------------------------------------------------------------

    static void LoadTextures() {
        const string FULL_PATH = "Packages/com.unity.streaming-image-sequence/Editor/Textures/Checked.png";
        m_checkedTexture = AssetDatabase.LoadAssetAtPath<Texture>(FULL_PATH);
    }
    
//----------------------------------------------------------------------------------------------------------------------

    private static Texture m_checkedTexture;

}

} //end namespace