using UnityEditor;

namespace UnityEngine.StreamingImageSequence {

internal static class EditorTextures {

    internal static Texture GetCheckedTexture() { return m_checkedTexture;}
    
//----------------------------------------------------------------------------------------------------------------------

    [InitializeOnLoadMethod]
    static void OnLoad() {
        const string FULL_PATH = "Packages/com.unity.streaming-image-sequence/Editor/Textures/Checked.png";
        m_checkedTexture = AssetDatabase.LoadAssetAtPath<Texture>(FULL_PATH);
    }
    
//----------------------------------------------------------------------------------------------------------------------

    private static Texture m_checkedTexture;

}

} //end namespace