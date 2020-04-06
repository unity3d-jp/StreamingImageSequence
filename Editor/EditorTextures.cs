using System.IO;
using UnityEditor;

namespace UnityEngine.StreamingImageSequence {

internal static class EditorTextures {

    internal static Texture GetCheckedTexture() { return m_checkedTexture;}
    
//----------------------------------------------------------------------------------------------------------------------

    [InitializeOnLoadMethod]
    static void OnLoad() {
        const string TEX_PATH = "Packages/com.unity.streaming-image-sequence/Editor/Textures/Checked.png";
        string fullTexPath = Path.GetFullPath(TEX_PATH);
        m_checkedTexture = AssetDatabase.LoadAssetAtPath<Texture>(fullTexPath);
    }
    
//----------------------------------------------------------------------------------------------------------------------

    private static Texture m_checkedTexture;

}

} //end namespace