using System.IO;
using UnityEngine;

namespace UnityEditor.StreamingImageSequence {

internal static class AssetEditorUtility {
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


//---------------------------------------------------------------------------------------------------------------------
    
    internal static void DeleteAssets(string path, string searchPattern) {
        if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            return;


        bool isUnityAsset = path.StartsWith(Application.dataPath);        
        DirectoryInfo di    = new DirectoryInfo(path);
        FileInfo[]    files = di.GetFiles(searchPattern);
        foreach (FileInfo fi in files) {
            string filePath = Path.Combine(path, fi.Name);
            if (isUnityAsset) {
                AssetDatabase.DeleteAsset(NormalizeAssetPath(filePath));
            } else {
                File.Delete(filePath);
            }
        }

    }
    
}

} //end namespace