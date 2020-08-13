using System.IO;
using UnityEngine;

namespace UnityEditor.StreamingImageSequence {

internal static class EditorTextures {

    internal static Texture GetCheckedTexture() {
        if (!m_initialized) {
            LoadTextures();
        }
        return m_checkedTexture;
    }

    internal static Texture GetLockTexture() {
        if (!m_initialized) {
            LoadTextures();
        }
        return m_lockTexture;
    }
    
//----------------------------------------------------------------------------------------------------------------------

    [InitializeOnLoadMethod]
    static void EditorTextures_OnLoad() {
        LoadTextures();
    }

    
//----------------------------------------------------------------------------------------------------------------------

    static void LoadTextures() {
        const string CHECKED_TEX_FULL_PATH = "Packages/com.unity.streaming-image-sequence/Editor/Textures/Checked.png";
        m_checkedTexture = AssetDatabase.LoadAssetAtPath<Texture>(CHECKED_TEX_FULL_PATH);
        
        const string STYLESHEET_IMAGE_PATH = "Packages/com.unity.streaming-image-sequence/Editor/StyleSheets/Images";
        string skin = EditorGUIUtility.isProSkin ? "DarkSkin" : "LightSkin";
        string lockTexFullPath = Path.Combine(STYLESHEET_IMAGE_PATH, skin, "FrameMarkerLock.png");
        m_lockTexture = AssetDatabase.LoadAssetAtPath<Texture>(lockTexFullPath);

        m_initialized = true;
    }
    
//----------------------------------------------------------------------------------------------------------------------

    private static bool m_initialized = false;
    private static Texture m_checkedTexture;
    private static Texture m_lockTexture;

}

} //end namespace