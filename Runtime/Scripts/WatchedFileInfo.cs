using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Assertions;

namespace UnityEngine.StreamingImageSequence {

[Serializable]
internal class WatchedFileInfo {
    internal WatchedFileInfo(string name, long size) {
        m_name = name;
        m_size = size;

    }
//----------------------------------------------------------------------------------------------------------------------    
    
    internal string GetName() { return m_name; }
    internal long GetSize() { return m_size; }
    
//----------------------------------------------------------------------------------------------------------------------

    internal static List<WatchedFileInfo> CreateList(string folder, List<string> fileNames) {

        int                   numFiles       = fileNames.Count;
        List<WatchedFileInfo> watchedFiles   = new List<WatchedFileInfo>(numFiles);
        foreach (string fileName in fileNames) {
            string fullPath = Path.Combine(folder, fileName);
            long   fileSize = 0;
            if (File.Exists(fullPath)) {
                FileInfo fileInfo =new FileInfo(fullPath);
                fileSize  = fileInfo.Length;
            }
            watchedFiles.Add(new WatchedFileInfo(fileName, fileSize));
        }

        return watchedFiles;

    }
    
    
    
//----------------------------------------------------------------------------------------------------------------------    
    
    [SerializeField] private string m_name;
    [SerializeField] private long   m_size;
}
} //end namespace


