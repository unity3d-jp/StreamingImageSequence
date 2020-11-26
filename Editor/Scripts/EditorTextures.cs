using System.IO;
using UnityEditor;
using UnityEngine;

namespace Unity.StreamingImageSequence.Editor {

internal static class EditorTextures {

    [InitializeOnLoadMethod]
    static void EditorTextures_OnLoad() {
        LoadTextures();
    }
    
//----------------------------------------------------------------------------------------------------------------------

    internal static Texture GetCheckedTexture() {
        if (null == m_checkedTexture) {
            LoadTextures();
        }
        return m_checkedTexture;
    }

    internal static Texture GetInactiveCheckedTexture() {
        if (null == m_inactiveCheckedTexture) {
            LoadTextures();
        }
        return m_inactiveCheckedTexture;
    }

    internal static Texture GetLockTexture() {
        if (null == m_lockTexture) {
            LoadTextures();
        }
        return m_lockTexture;
    }

//----------------------------------------------------------------------------------------------------------------------

    internal static Texture2D GetOrCreatePreviewBGTexture() {
        if (null != m_previewBGTexture)
            return m_previewBGTexture;
        
        m_previewBGTexture           = new Texture2D(1,1, TextureFormat.ARGB32,false);
        m_previewBGTexture.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
        return m_previewBGTexture;
    }
    
//----------------------------------------------------------------------------------------------------------------------

    static void LoadTextures() {
        if (null == m_checkedTexture) {
            const string CHECKED_TEX_FULL_PATH = "Packages/com.unity.streaming-image-sequence/Editor/Textures/Checked.png";
            m_checkedTexture = AssetDatabase.LoadAssetAtPath<Texture>(CHECKED_TEX_FULL_PATH);            
        }

        if (null == m_inactiveCheckedTexture) {
            const string TEX_FULL_PATH = "Packages/com.unity.streaming-image-sequence/Editor/Textures/InactiveChecked.png";
            m_inactiveCheckedTexture = AssetDatabase.LoadAssetAtPath<Texture>(TEX_FULL_PATH);            
        }
        
        if (null == m_lockTexture) {
            const string STYLESHEET_IMAGE_PATH = "Packages/com.unity.streaming-image-sequence/Editor/StyleSheets/Images";
            string skin = EditorGUIUtility.isProSkin ? "DarkSkin" : "LightSkin";
            string lockTexFullPath = Path.Combine(STYLESHEET_IMAGE_PATH, skin, "FrameMarkerLock.png");
            m_lockTexture = AssetDatabase.LoadAssetAtPath<Texture>(lockTexFullPath);            
        }

    }
    
//----------------------------------------------------------------------------------------------------------------------

    private static Texture   m_checkedTexture;
    private static Texture   m_inactiveCheckedTexture;
    private static Texture   m_lockTexture;
    private static Texture2D m_previewBGTexture = null;

}

} //end namespace