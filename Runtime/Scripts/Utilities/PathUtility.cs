using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Assertions;

namespace UnityEngine.StreamingImageSequence {

internal static class PathUtility {
    
    internal static string GetPath(string folder, string fileName) {
        string filePath = !string.IsNullOrEmpty(folder) ? Path.Combine(folder, fileName) : fileName;
        return filePath;
    }

//----------------------------------------------------------------------------------------------------------------------
    
    internal static byte[] CalculateFolderMD5ByFileSize(string folder, 
        ICollection<string> fileSearchPatterns, 
        IComparer<string> fileNameComparer) 
    {
        Assert.IsTrue(Directory.Exists(folder));

        List<string> fileNames = new List<string>();
        foreach (string pattern in fileSearchPatterns) {
            IEnumerable<string> files = Directory.EnumerateFiles(folder, pattern, SearchOption.TopDirectoryOnly);
            foreach (string filePath in files) {                    
                fileNames.Add(Path.GetFileName(filePath));
            }
        }
        fileNames.Sort(fileNameComparer);

        MD5 md5 = MD5.Create();
        
        int numFiles = fileNames.Count;
        for (int i = 0; i < numFiles - 1; ++i) {
            //filename
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileNames[i].ToLower());
            md5.TransformBlock(fileNameBytes, 0, fileNameBytes.Length, fileNameBytes, 0);

            //hash the size of the file
            string path         = Path.Combine(folder, fileNames[i]);
            long   length       = new System.IO.FileInfo(path).Length;
            byte[] contentBytes = BitConverter.GetBytes(length);
            md5.TransformBlock(contentBytes, 0, contentBytes.Length, contentBytes, 0);            
        }
        
        //Final
        {
            int lastIndex = numFiles - 1;
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileNames[lastIndex].ToLower());
            md5.TransformBlock(fileNameBytes, 0, fileNameBytes.Length, fileNameBytes, 0);

            //hash the size of the file
            string path         = Path.Combine(folder, fileNames[lastIndex]);
            long   length       = new System.IO.FileInfo(path).Length;
            byte[] contentBytes = BitConverter.GetBytes(length);
            md5.TransformFinalBlock(contentBytes, 0, contentBytes.Length);
        }

        return md5.Hash;

    }
    
    
}

}