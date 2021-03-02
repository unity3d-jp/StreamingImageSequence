using System.Collections.Generic;
using System.IO;
using JetBrains.Annotations;
using Unity.FilmInternalUtilities;

using UnityEngine;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Timeline;
#endif


namespace Unity.StreamingImageSequence {

/// <summary>
/// A PlayableAsset that points to a folder that contains images
/// </summary>
[System.Serializable]
internal abstract class ImageFolderPlayableAsset<T> : BaseExtendedClipPlayableAsset<T>, IReloader
    where T: PlayableFrameClipData
{
    private void Awake() {
        //Find the used folder in runtime. Unused in the editor        
        const string EDITOR_STREAMING_ASSETS_PATH = "Assets/StreamingAssets/";  
        if (!Application.isEditor && m_folder.StartsWith(EDITOR_STREAMING_ASSETS_PATH)) {
            string relPath = m_folder.Substring(EDITOR_STREAMING_ASSETS_PATH.Length);
            m_runtimeFolderUnderStreamingAssets =Path.Combine(Application.streamingAssetsPath, relPath); 
        }
        
    }
    
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
            || null == m_imageFiles || m_imageFiles.Count <= 0)
            return;

        //Get the first image to update the resolution.    
        string fullPath = GetFirstImageData(out ImageData imageData);
        switch (imageData.ReadStatus) {
            case StreamingImageSequenceConstants.READ_STATUS_LOADING: {
                break;
            }
            case StreamingImageSequenceConstants.READ_STATUS_SUCCESS: {
                UpdateResolution(imageData);
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
    
    protected void UpdateResolution(ImageData imageData) {
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

        return (null == m_imageFiles) ? 0 : m_imageFiles.Count;
    }
    internal System.Collections.IList GetImageFileNamesNonGeneric() { return m_imageFiles; }


    internal void  SetTimelineBGColor(Color color) { m_timelineBGColor = color; }
    internal Color GetTimelineBGColor()            { return m_timelineBGColor; }

    
//----------------------------------------------------------------------------------------------------------------------

    [CanBeNull]
    internal string GetImageFilePath(int index) {
        if (null == m_imageFiles)
            return null;

        if (index < 0 || index >= m_imageFiles.Count)
            return null;

        //For runtime only
        if (!string.IsNullOrEmpty(m_runtimeFolderUnderStreamingAssets)) {
            return PathUtility.GetPath(m_runtimeFolderUnderStreamingAssets, m_imageFiles[index].GetName());
        }
                               
        return PathUtility.GetPath(m_folder, m_imageFiles[index].GetName());            
    }


//----------------------------------------------------------------------------------------------------------------------    
    
    string GetFirstImageData(out ImageData imageData) {
        Assert.IsFalse(string.IsNullOrEmpty(m_folder));
        Assert.IsTrue(Directory.Exists(m_folder));
        Assert.IsTrue(m_imageFiles.Count > 0);

        const int TEX_TYPE = StreamingImageSequenceConstants.IMAGE_TYPE_FULL;        

        string fullPath = GetImageFilePath(0);
        ImageLoader.GetImageDataInto(fullPath,TEX_TYPE, out imageData);
        return fullPath;               
    }
    
//----------------------------------------------------------------------------------------------------------------------    

#if UNITY_EDITOR    

#region Reload/Find images
    /*
     * [Note-sin: 2020-8-25]
     * There are a couple ways in which a folder will be reloaded:
     * 1. When the inspector explicitly asks to reload the image
     * 2. When the inspector notifies that a particular folder has been updated (observer)
     * 3. When Unity Editor Application becomes active
     */       

    public void Reload() {        
        if (string.IsNullOrEmpty(m_folder) || !Directory.Exists(m_folder))
            return;

        List<WatchedFileInfo> newImageFiles  = FindImages(m_folder);
        int                   numNewImages   = newImageFiles.Count;
        int                   numPrevImages  = GetNumImages();
        bool                  changed        = false;

        //Check if we need to unload prev images, and detect change
        for (int i = 0; i < numPrevImages; ++i) {
            if (i >= numNewImages) {
                UnloadImage(i);
                changed = true;
                continue;
            }

            if (newImageFiles[i] == m_imageFiles[i])
                continue;

            UnloadImage(i);
            changed = true;
        }

        //Do nothing if no change is detected
        if (!changed && numNewImages == numPrevImages) {
            return;
        }

        m_imageFiles = newImageFiles;
        ReloadInternalV();
        EditorUtility.SetDirty(this);
        
    }
    

    internal List<WatchedFileInfo> FindImages(string path) {
        return WatchedFileInfo.FindFiles(path, GetSupportedImageFilePatternsV());
    }

    //Return WatchedFileInfos (with file names)
        

    protected abstract string[] GetSupportedImageFilePatternsV();
    
#endregion Reload/Find images

//----------------------------------------------------------------------------------------------------------------------    
    private void UnloadImage(int index) {
        Assert.IsTrue(index < m_imageFiles.Count);
        string imagePath = PathUtility.GetPath(m_folder, m_imageFiles[index].GetName());
        StreamingImageSequencePlugin.UnloadImageAndNotify(imagePath);
    }
    
    
#endif  //End #if UNITY_EDITOR Editor
    
//----------------------------------------------------------------------------------------------------------------------    
    
#region PlayableFrames

    internal void ResetPlayableFrames() {
#if UNITY_EDITOR
        Undo.RegisterCompleteObjectUndo(this, "Resetting PlayableFrames");
#endif
        GetBoundClipData()?.ResetPlayableFrames(); //Null check. the data might not have been bound during recompile
            
#if UNITY_EDITOR 
        TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved );
#endif            
           
    }

    internal void RefreshPlayableFrames() {

        PlayableFrameClipData clipData = GetBoundClipData();               
        clipData?.RefreshPlayableFrames(); //Null check. the data might not have been bound during recompile            
    }
        
#endregion
    
//----------------------------------------------------------------------------------------------------------------------    
    [HideInInspector][SerializeField] protected string       m_folder         = null;
    
    //[TODO-sin: 2020-9-9] Obsolete, and should be removed completely before releasing
    [HideInInspector][SerializeField] protected List<string> m_imageFileNames = null; //file names, not paths
    
    [HideInInspector][SerializeField] protected List<WatchedFileInfo> m_imageFiles = null; //store file names, not paths

    [HideInInspector][SerializeField] private Color m_timelineBGColor = Color.gray;
    
//----------------------------------------------------------------------------------------------------------------------    
    private float             m_dimensionRatio = 0;
    private ImageDimensionInt m_resolution;
    private string            m_runtimeFolderUnderStreamingAssets = null;

}

} //end namespace


