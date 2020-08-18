using System.IO;

namespace UnityEngine.StreamingImageSequence {

internal static class PathUtility {
    
    internal static string GetPath(string folder, string fileName) {
        string filePath = null;
            
        if (!string.IsNullOrEmpty(folder)) {
            filePath = Path.Combine(folder, fileName);
        } else {
            filePath = fileName;
        }

        return filePath;
    }
}

}