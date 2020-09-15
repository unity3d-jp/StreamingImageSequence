using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine.Assertions;

namespace Unity.StreamingImageSequence {

internal static class PathUtility {
    
    internal static string GetPath(string folder, string fileName) {
        string filePath = !string.IsNullOrEmpty(folder) ? Path.Combine(folder, fileName) : fileName;
        return filePath;
    }

//----------------------------------------------------------------------------------------------------------------------
    
    internal static string CalculateFolderMD5ByFileSize(string folder, 
        ICollection<string> fileSearchPatterns, 
        Comparison<string> fileNameComparer) 
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
        if (numFiles <= 0)
            return "";
            
        byte[] lengthBytes = new byte[sizeof(long)];
        for (int i = 0; i < numFiles - 1; ++i) {
            //filename
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileNames[i].ToLower());
            md5.TransformBlock(fileNameBytes, 0, fileNameBytes.Length, fileNameBytes, 0);

            //hash the size of the file
            long length = new System.IO.FileInfo(Path.Combine(folder, fileNames[i])).Length;
            BitUtility.ConvertToByte(length, ref lengthBytes);
            md5.TransformBlock(lengthBytes, 0, lengthBytes.Length, lengthBytes, 0);            
        }
        
        //Final
        {
            int lastIndex = numFiles - 1;
            byte[] fileNameBytes = Encoding.UTF8.GetBytes(fileNames[lastIndex].ToLower());
            md5.TransformBlock(fileNameBytes, 0, fileNameBytes.Length, fileNameBytes, 0);

            //hash the size of the file
            long  length = new System.IO.FileInfo(Path.Combine(folder, fileNames[lastIndex])).Length;
            BitUtility.ConvertToByte(length, ref lengthBytes);
            md5.TransformFinalBlock(lengthBytes, 0, lengthBytes.Length);
        }

        return BitConverter.ToString(md5.Hash).Replace("-", "").ToLowerInvariant();
    }
    
}

}