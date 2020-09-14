using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence {

internal static class PreviewTextureFactory {

    [InitializeOnLoadMethod]
    static void PreviewTextureFactory_OnEditorLoad() {
        m_previewTextures = new Dictionary<string, PreviewTexture>();
        m_obsoleteTextures = new List<string>();
        m_removeObsoleteTextures = false;
        EditorApplication.update += Update;
        
        EditorSceneManager.sceneClosed     += PreviewTextureFactory_OnSceneClosed;
        EditorSceneManager.newSceneCreated += PreviewTextureFactory_OnSceneCreated;

        StreamingImageSequencePlugin.OnImageUnloaded += PreviewTextureFactory_OnImageUnloaded;
    }
    
    static void PreviewTextureFactory_OnSceneClosed(Scene scene) {
        PreviewTextureFactory.Reset();
    }

    static void PreviewTextureFactory_OnSceneCreated( Scene scene, NewSceneSetup setup, NewSceneMode mode) {
        PreviewTextureFactory.Reset();
    }

    static void PreviewTextureFactory_OnImageUnloaded(string imagePath) {
        if (!m_previewTextures.ContainsKey(imagePath))
            return;
                
        PreviewTexture texToRemove = m_previewTextures[imagePath];
        texToRemove.Dispose();
        m_previewTextures.Remove(imagePath);
    }
    
//----------------------------------------------------------------------------------------------------------------------    

    public static void Reset() {        
        foreach (KeyValuePair<string, PreviewTexture> previewTex in m_previewTextures) {
            previewTex.Value.Dispose();
        }        
        m_previewTextures.Clear();
    }
    
//----------------------------------------------------------------------------------------------------------------------    

    public static Texture2D GetOrCreate(string fullPath, ref ImageData imageData) {
        Assert.IsTrue(StreamingImageSequenceConstants.READ_STATUS_SUCCESS == imageData.ReadStatus);
        
        //We only remove obsolete textures if there is any access to PreviewTextures in a particular frame. 
        //For example, if Unity is not in focus, then we don't want to remove them.
        m_removeObsoleteTextures = true;

        if (m_previewTextures.ContainsKey(fullPath)) {
            Assert.IsNotNull(m_previewTextures[fullPath].GetTexture());
            return m_previewTextures[fullPath].GetTexture();
        }

        //This is static. Don't destroy the tex if a new scene is loaded
        Texture2D newTex = imageData.CreateCompatibleTexture(HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor);
        imageData.CopyBufferToTexture(newTex);
        newTex.name = "Preview: " + fullPath;
        m_previewTextures[fullPath] = new PreviewTexture(newTex);
        
        return newTex;
    }

//----------------------------------------------------------------------------------------------------------------------    
    private static void Update() {
        double curTime = EditorApplication.timeSinceStartup;
        if (!m_removeObsoleteTextures)
            return;

        //Remove obsolete textures
        m_obsoleteTextures.Clear();
        foreach (KeyValuePair<string, PreviewTexture> keyValue in m_previewTextures) {
            if (curTime - keyValue.Value.GetLastAccessTime() > OBSOLETE_TIME) {
                m_obsoleteTextures.Add(keyValue.Key);
            }
        }
        foreach (string texFullPath in m_obsoleteTextures) {
            PreviewTexture texToRemove = m_previewTextures[texFullPath];
            texToRemove.Dispose();
            m_previewTextures.Remove(texFullPath);
        }

        m_removeObsoleteTextures = false;
    }

//----------------------------------------------------------------------------------------------------------------------    
    internal static IDictionary<string, PreviewTexture> GetPreviewTextures() {
        return m_previewTextures;
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    private static Dictionary<string, PreviewTexture> m_previewTextures = null;
    private static List<string> m_obsoleteTextures = null;
    
    private const double OBSOLETE_TIME = 10; //seconds 
    static bool m_removeObsoleteTextures;


}

} //end namespace