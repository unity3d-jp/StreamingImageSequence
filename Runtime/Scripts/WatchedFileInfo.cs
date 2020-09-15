using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.StreamingImageSequence {

[Serializable]
internal class WatchedFileInfo {

    internal WatchedFileInfo(string name, long size) {
        m_name = name;
        m_size = size;
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    public static bool operator== (WatchedFileInfo obj1, WatchedFileInfo obj2) {
               
        return (obj1.m_name == obj2.m_name && obj1.m_size == obj2.m_size); 
    }
    
    public static bool operator!= (WatchedFileInfo obj1, WatchedFileInfo obj2) {
        return (obj1.m_name != obj2.m_name || obj1.m_size != obj2.m_size); 
    }    
    
    public override bool Equals(object obj) {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((WatchedFileInfo) obj);
    }
    
    public override int GetHashCode() {
        unchecked {
            return ((m_name != null ? m_name.GetHashCode() : 0) * 397) ^ m_size.GetHashCode();
        }
    }
//----------------------------------------------------------------------------------------------------------------------    

    public override string ToString() { return m_name;}      
    internal        string GetName()  { return m_name; }
    internal        long   GetSize()  { return m_size; }
    
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


