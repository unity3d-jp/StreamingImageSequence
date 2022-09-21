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

    internal bool Load(string fullPath, out ImageData imageData) {
#if UNITY_EDITOR        
        if (fullPath.IsRegularAssetPath()) {
            imageData                          = new ImageData(StreamingImageSequenceConstants.READ_STATUS_USE_EDITOR_API);
            m_cachedTexturesInEditor[fullPath] = AssetDatabase.LoadAssetAtPath<Texture2D>(fullPath);
            return true;
        }
#endif

        imageData = new ImageData(StreamingImageSequenceConstants.READ_STATUS_FAIL);
        return false;
    }
    
    internal void UnloadAll() {
#if UNITY_EDITOR
        foreach (KeyValuePair<string, Texture2D> kv in m_cachedTexturesInEditor) {
            Resources.UnloadAsset(kv.Value);
        }
        m_cachedTexturesInEditor.Clear();
#endif
    }
    

//--------------------------------------------------------------------------------------------------------------------------------------------------------------    
    Dictionary<string,Texture2D> m_cachedTexturesInEditor = new Dictionary<string, Texture2D>();
    
}

} //end namespace
