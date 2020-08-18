using System.IO;

namespace UnityEngine.StreamingImageSequence {

internal static class PathUtility {
    
    internal static string GetPath(string folder, string fileName) {
        string filePath = !string.IsNullOrEmpty(folder) ? Path.Combine(folder, fileName) : fileName;
        return filePath;
    }
    
    
}

}