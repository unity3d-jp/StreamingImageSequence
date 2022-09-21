using System;
using System.IO;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Assertions;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.FilmInternalUtilities;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
#if UNITY_EDITOR
using UnityEditor.SceneManagement;
using UnityEditor;
#endif

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
#if UNITY_EDITOR        
        if (fullPath.IsRegularAssetPath()) {
            
            bool isCached   = m_cachedTexturesInEditor.TryGetValue(fullPath, out tex);
            bool isTexReady = null != tex;
            if (!isCached || !isTexReady) {
                m_cachedTexturesInEditor[fullPath] = tex = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
            }
            
            m_regularAssetLoaded = isTexReady;
            m_regularAssetLoadLogger.Update("[SIS]", Path.GetDirectoryName(fullPath));
        
            if (!m_regularAssetLoaded) {
                imageData = new ImageData(StreamingImageSequenceConstants.READ_STATUS_FAIL);
                return false;
            }
            
            imageData = new ImageData(StreamingImageSequenceConstants.READ_STATUS_USE_EDITOR_API);
            return true;
        }
#endif

        tex       = null;
        imageData = new ImageData(StreamingImageSequenceConstants.READ_STATUS_FAIL);
        return false;
    }
    
//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    internal void UnloadAll() {
#if UNITY_EDITOR
        foreach (KeyValuePair<string, Texture2D> kv in m_cachedTexturesInEditor) {
            Resources.UnloadAsset(kv.Value);
        }
        m_cachedTexturesInEditor.Clear();
#endif
    }
    

//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    private OneTimeLogger m_regularAssetLoadLogger;
    private bool          m_regularAssetLoaded = false;
    
    Dictionary<string,Texture2D> m_cachedTexturesInEditor = new Dictionary<string, Texture2D>(); //path -> Texture2D
    
}

} //end namespace
