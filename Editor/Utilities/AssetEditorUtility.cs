using System.IO;
using UnityEngine;

namespace UnityEditor.StreamingImageSequence {

public static class AssetEditorUtility {
    public static void OverwriteAsset(Object asset, string path) {
        if (File.Exists(path)) {
            AssetDatabase.DeleteAsset(path);
        }

        AssetDatabase.CreateAsset(asset, path);
    }

//---------------------------------------------------------------------------------------------------------------------
    //Normalize so that the path is relative to the Unity root project
    public static string NormalizeAssetPath(string path) {
        if (string.IsNullOrEmpty(path))
            return null;

        if (path.StartsWith(Application.dataPath)) {
            return path.Substring(Application.dataPath.Length - "Assets".Length);
        }
        return path;
    }


}

} //end namespace