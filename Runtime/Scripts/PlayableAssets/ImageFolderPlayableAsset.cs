using System;
using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine.Assertions;

namespace UnityEngine.StreamingImageSequence {

/// <summary>
/// A PlayableAsset that points to a folder that contains images
/// </summary>
[System.Serializable]
internal abstract class ImageFolderPlayableAsset : BaseTimelineClipSISDataPlayableAsset {
   
//----------------------------------------------------------------------------------------------------------------------

    protected abstract void ReloadInternalV();
    
//----------------------------------------------------------------------------------------------------------------------
    
#region Resolution    
    
    //May return uninitialized value during initialization because the resolution hasn't been updated
    internal ImageDimensionInt GetResolution() { return m_resolution; }
   
    internal float GetOrUpdateDimensionRatio() {
        if (Mathf.Approximately(0.0f, m_dimensionRatio)) {
            ForceUpdateResolution();
        }
            
        return m_dimensionRatio;
    }    
    
    void ForceUpdateResolution() {
        if (string.IsNullOrEmpty(m_folder) || !Directory.Exists(m_folder) 
            || null == m_imageFileNames || m_imageFileNames.Count <= 0)
            return;

        //Get the first image to update the resolution.    
        string fullPath = GetFirstImageData(out ImageData imageData);
        switch (imageData.ReadStatus) {
            case StreamingImageSequenceConstants.READ_STATUS_LOADING: {
                break;
            }
            case StreamingImageSequenceConstants.READ_STATUS_SUCCESS: {
                UpdateResolution(ref imageData);
                break;
            }
            default: {
                ImageLoader.RequestLoadFullImage(fullPath);
                break;
            }
        }
    }

    protected void ResetResolution() {
        m_resolution     = new ImageDimensionInt();
        m_dimensionRatio = 0;
    }
    
    protected void UpdateResolution(ref ImageData imageData) {
        m_resolution.Width  = imageData.Width;
        m_resolution.Height = imageData.Height;
        if (m_resolution.Width > 0 && m_resolution.Height > 0) {
            m_dimensionRatio = m_resolution.CalculateRatio();
        }
    }

    protected void UpdateResolution(ImageDimensionInt res) {
        m_resolution = res;
        m_dimensionRatio = 0;
        if (m_resolution.Width > 0 && m_resolution.Height > 0) {
            m_dimensionRatio = m_resolution.CalculateRatio();
        }        
    }
    
#endregion Resolution
    
//----------------------------------------------------------------------------------------------------------------------
    /// <summary>
    /// Get the source folder
    /// </summary>
    /// <returns>The folder where the images are located</returns>
    internal string GetFolder() { return m_folder;}
    internal void SetFolder(string folder) { m_folder = folder;}

    internal int GetNumImages() {
        if (string.IsNullOrEmpty(m_folder))
            return 0;

        return (null == m_imageFileNames) ? 0 : m_imageFileNames.Count;
    }
    internal System.Collections.IList GetImageFileNamesNonGeneric() { return m_imageFileNames; }


//----------------------------------------------------------------------------------------------------------------------

    [CanBeNull]
    internal string GetImageFilePath(int index) {
        if (null == m_imageFileNames)
            return null;

        if (index < 0 || index >= m_imageFileNames.Count)
            return null;
        
        return PathUtility.GetPath(m_folder, m_imageFileNames[index]);            
    }


//----------------------------------------------------------------------------------------------------------------------    
    
    string GetFirstImageData(out ImageData imageData) {
        Assert.IsFalse(string.IsNullOrEmpty(m_folder));
        Assert.IsTrue(Directory.Exists(m_folder));
        Assert.IsTrue(m_imageFileNames.Count > 0);

        const int TEX_TYPE = StreamingImageSequenceConstants.IMAGE_TYPE_FULL;        

        string fullPath = Path.GetFullPath(Path.Combine(m_folder, m_imageFileNames[0]));
        ImageLoader.GetImageDataInto(fullPath,TEX_TYPE, out imageData);
        return fullPath;               
    }
    
//----------------------------------------------------------------------------------------------------------------------    

#if UNITY_EDITOR    

#region Reload/Find images
    /*
     * [Note-sin: 2020-8-25]
     * There are a couple ways in which a folder will be reloaded:
     * 1. When the PlayableGraph in TimelineEditor is created (refreshed)
     * 2. When the inspector explicitly asks to reload the image
     * 3. When the inspector notifies that a particular folder has been updated (observer)
     * 4. When Unity Editor Application becomes active
     */       

    internal void Reload() {
        
        if (UpdateFolderMD5()) {
            ForceReload(m_folderMD5);                    
        }
    }
    
    internal void ForceReload(string folderMD5 = null) {
           
        if (string.IsNullOrEmpty(m_folder))
            return;
            
        //Unload existing images
        int numImages = GetNumImages();
        if (numImages > 0) {
            foreach (string fileName in m_imageFileNames) {
                string imagePath = PathUtility.GetPath(m_folder, fileName);
                StreamingImageSequencePlugin.UnloadImageAndNotify(imagePath);
            }
        }

        m_imageFileNames = FindImages(m_folder); 
        if (!string.IsNullOrEmpty(folderMD5)) {
            m_folderMD5 = folderMD5;
        }
        else {
            UpdateFolderMD5();
        }
        
        ReloadInternalV();
        EditorUtility.SetDirty(this);
        
    }
    
    //Returns true if the MD5 has changed
    protected bool UpdateFolderMD5() {

        string prevFolderMD5 = m_folderMD5; 
        m_folderMD5 = PathUtility.CalculateFolderMD5ByFileSize(m_folder, GetSupportedImageFilePatternsV(), FileNameComparer);
        return (prevFolderMD5 != m_folderMD5);         
    }

    internal static List<string> FindFiles(string path, string[] filePatterns) {
        return FindFilesInternal(path, filePatterns);
    }

    internal List<string> FindImages(string path) {
        return FindFilesInternal(path, GetSupportedImageFilePatternsV());
    }

    //Return FileNames
    private static List<string> FindFilesInternal(string path, string[] filePatterns) {
        Assert.IsFalse(string.IsNullOrEmpty(path));
        Assert.IsTrue(Directory.Exists(path));

        //Convert path to folder here
        string fullSrcPath = Path.GetFullPath(path).Replace("\\", "/");

        //Enumerate all files with the supported extensions and sort
        List<string> fileNames         = new List<string>();
        foreach (string pattern in filePatterns) {
            IEnumerable<string> files = Directory.EnumerateFiles(fullSrcPath, pattern, SearchOption.TopDirectoryOnly);
            foreach (string filePath in files) {                    
                fileNames.Add(Path.GetFileName(filePath));
            }
        }
        fileNames.Sort(FileNameComparer);
        return fileNames;
    }        
        
    private static int FileNameComparer(string x, string y) {
        return string.Compare(x, y, StringComparison.InvariantCultureIgnoreCase);
    }

    protected abstract string[] GetSupportedImageFilePatternsV();
    
#endregion Reload/Find images
    
#endif  //End #if UNITY_EDITOR Editor
    
//----------------------------------------------------------------------------------------------------------------------    
    [HideInInspector][SerializeField] protected string       m_folder         = null;
    [HideInInspector][SerializeField] protected List<string> m_imageFileNames = null; //file names, not paths
    [HideInInspector][SerializeField] protected string       m_folderMD5      = null;

//----------------------------------------------------------------------------------------------------------------------    
    private float m_dimensionRatio = 0;
    private ImageDimensionInt m_resolution;        
    
}

} //end namespace


