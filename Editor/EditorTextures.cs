using System.IO;
using UnityEngine;

namespace UnityEditor.StreamingImageSequence {

internal static class EditorTextures {

    internal static Texture GetCheckedTexture() {
        if (null == m_checkedTexture) {
            LoadTextures();
        }
        return m_checkedTexture;
    }

    internal static Texture GetLockTexture() {
        if (null == m_lockTexture) {
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
        if (null == m_checkedTexture) {
            const string CHECKED_TEX_FULL_PATH = "Packages/com.unity.streaming-image-sequence/Editor/Textures/Checked.png";
            m_checkedTexture = AssetDatabase.LoadAssetAtPath<Texture>(CHECKED_TEX_FULL_PATH);            
        }

        if (null == m_lockTexture) {
            const string STYLESHEET_IMAGE_PATH = "Packages/com.unity.streaming-image-sequence/Editor/StyleSheets/Images";
            string skin = EditorGUIUtility.isProSkin ? "DarkSkin" : "LightSkin";
            string lockTexFullPath = Path.Combine(STYLESHEET_IMAGE_PATH, skin, "FrameMarkerLock.png");
            m_lockTexture = AssetDatabase.LoadAssetAtPath<Texture>(lockTexFullPath);            
        }

    }
    
//----------------------------------------------------------------------------------------------------------------------

    private static Texture m_checkedTexture;
    private static Texture m_lockTexture;

}

} //end namespace