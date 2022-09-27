#if UNITY_EDITOR

using System.IO;
using System.Collections.Generic;
using Unity.FilmInternalUtilities;
using UnityEngine;
using UnityEditor.SceneManagement;
using UnityEditor;

namespace Unity.StreamingImageSequence {

[System.Serializable]
internal class EditorCachedTextureLoader {

    internal EditorCachedTextureLoader() {
        EditorSceneManager.sceneClosed += OnSceneClosed;
        m_regularAssetLoadLogger = new OneTimeLogger(() => !m_regularAssetLoaded,
            $"Can't load textures. Make sure their import settings are set to Texture2D. Folder: ");
    }

    void OnSceneClosed(UnityEngine.SceneManagement.Scene scene) {
        UnloadAll();
    }
    
//--------------------------------------------------------------------------------------------------------------------------------------------------------------    

    internal bool GetOrLoad(string fullPath, out ImageData imageData, out Texture2D tex) {
        if (fullPath.IsRegularAssetPath()) {
            
            bool isCached   = m_cachedTexturesInEditor.TryGetValue(fullPath, out tex);
            bool isTexReady = null != tex;
            if (!isCached || !isTexReady) {
                m_cachedTexturesInEditor[fullPath] = tex = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
                
                isTexReady = null != tex;
            }
            
            //Log
            m_regularAssetLoaded = isTexReady;
            m_regularAssetLoadLogger.Update("[SIS]", Path.GetDirectoryName(fullPath));
        
            if (!m_regularAssetLoaded) {
                imageData = new ImageData(StreamingImageSequenceConstants.READ_STATUS_FAIL);
                return false;
            }
            
            imageData = new ImageData(StreamingImageSequenceConstants.READ_STATUS_USE_EDITOR_API);
            return true;
        }

        tex       = null;
        imageData = new ImageData(StreamingImageSequenceConstants.READ_STATUS_FAIL);
        return false;
    }
    
//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    internal void UnloadAll() {
        foreach (KeyValuePair<string, Texture2D> kv in m_cachedTexturesInEditor) {
            Resources.UnloadAsset(kv.Value);
        }
        m_cachedTexturesInEditor.Clear();
    }
    

//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    private OneTimeLogger m_regularAssetLoadLogger;
    private bool          m_regularAssetLoaded = false;
    
    Dictionary<string,Texture2D> m_cachedTexturesInEditor = new Dictionary<string, Texture2D>(); //path -> Texture2D
    
}

} //end namespace

#endif
