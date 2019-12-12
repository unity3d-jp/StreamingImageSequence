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

}

} //end namespace