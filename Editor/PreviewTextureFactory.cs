using System.Collections.Generic;
using NUnit.Framework;
using UnityEditor;

namespace UnityEngine.StreamingImageSequence {

internal static class PreviewTextureFactory {

    [InitializeOnLoadMethod]
    static void OnLoad() {
        m_previewTextures = new Dictionary<string, PreviewTexture>();
        m_obsoleteTextures = new List<string>();
        EditorApplication.update += Update;
    }
    
//----------------------------------------------------------------------------------------------------------------------    

    public static void Reset() {
        m_previewTextures.Clear();
    }
    
//----------------------------------------------------------------------------------------------------------------------    

    public static Texture2D GetOrCreate(string fullPath, ref ReadResult readResult) {
        Assert.IsTrue(StreamingImageSequenceConstants.READ_RESULT_SUCCESS == readResult.ReadStatus);

        if (m_previewTextures.ContainsKey(fullPath)) {
            Assert.IsNotNull(m_previewTextures[fullPath].GetTexture());
            return m_previewTextures[fullPath].GetTexture();
        }

        Texture2D newTex = StreamingImageSequencePlugin.CreateTexture(ref readResult);
        newTex.name = fullPath;
        m_previewTextures[fullPath] = new PreviewTexture(newTex);
        newTex.hideFlags = HideFlags.HideAndDontSave; //This is static. Don't destroy the tex if a new scene is loaded
        return newTex;
    }

//----------------------------------------------------------------------------------------------------------------------    
    private static void Update() {
        double curTime = EditorApplication.timeSinceStartup;

        if (StreamingImageSequencePlugin.IsResetting())
            return;

        //Remove obsolete textures
        m_obsoleteTextures.Clear();
        foreach (KeyValuePair<string, PreviewTexture> keyValue in m_previewTextures) {
            if (curTime - keyValue.Value.GetLastAccessTime() > OBSOLETE_TIME) {
                m_obsoleteTextures.Add(keyValue.ToString());
            }
        }
        foreach (string texFullPath in m_obsoleteTextures) {
            m_previewTextures.Remove(texFullPath);
        }
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    private static Dictionary<string, PreviewTexture> m_previewTextures = null;
    private static List<string> m_obsoleteTextures = null;
    
    private const double OBSOLETE_TIME = 10; //seconds 

}

} //end namespace