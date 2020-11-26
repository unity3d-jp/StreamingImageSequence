using System.IO;
using Unity.AnimeToolbox;
using UnityEditor;
using UnityEngine;

namespace Unity.StreamingImageSequence.Editor {

internal static class EditorTextures {
    
//----------------------------------------------------------------------------------------------------------------------

    internal static Texture GetCheckedTexture() {
        if (!m_checkedTexture.IsNullRef()) {
            return m_checkedTexture;
        }
        const string CHECKED_TEX_FULL_PATH = "Packages/com.unity.streaming-image-sequence/Editor/Textures/Checked.png";
        m_checkedTexture = AssetDatabase.LoadAssetAtPath<Texture>(CHECKED_TEX_FULL_PATH);            
        return m_checkedTexture;
    }

    internal static Texture GetInactiveCheckedTexture() {
        if (!m_inactiveCheckedTexture.IsNullRef()) {
            return m_inactiveCheckedTexture;
        }
        const string TEX_FULL_PATH = "Packages/com.unity.streaming-image-sequence/Editor/Textures/InactiveChecked.png";
        m_inactiveCheckedTexture = AssetDatabase.LoadAssetAtPath<Texture>(TEX_FULL_PATH);            
        return m_inactiveCheckedTexture;
    }

    internal static Texture GetLockTexture() {
        if (!m_lockTexture.IsNullRef()) {
            return m_lockTexture;
        }
        
        const string STYLESHEET_IMAGE_PATH = "Packages/com.unity.streaming-image-sequence/Editor/StyleSheets/Images";
        string       skin                  = EditorGUIUtility.isProSkin ? "DarkSkin" : "LightSkin";
        string       lockTexFullPath       = Path.Combine(STYLESHEET_IMAGE_PATH, skin, "FrameMarkerLock.png");
        m_lockTexture = AssetDatabase.LoadAssetAtPath<Texture>(lockTexFullPath);                    
        return m_lockTexture;
    }

//----------------------------------------------------------------------------------------------------------------------

    internal static Texture2D GetOrCreatePreviewBGTexture() {
        if (!m_previewBGTexture.IsNullRef())
            return m_previewBGTexture;
        
        m_previewBGTexture           = new Texture2D(1,1, TextureFormat.ARGB32,false);
        m_previewBGTexture.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
        return m_previewBGTexture;
    }
    
//----------------------------------------------------------------------------------------------------------------------

    private static Texture m_checkedTexture         = null;
    private static Texture m_inactiveCheckedTexture = null;
    private static Texture m_lockTexture            = null;
    
    private static Texture2D m_previewBGTexture = null;

}

} //end namespace