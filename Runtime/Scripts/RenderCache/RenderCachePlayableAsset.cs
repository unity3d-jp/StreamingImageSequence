using System.Collections.Generic;
using System.IO;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {

/// <summary>
/// A PlayableAsset that is used to capture the render results of BaseRenderCapturer in Timeline.
/// Implements the following interfaces:
/// - ITimelineClipAsset: for defining clip capabilities (ClipCaps) 
/// </summary>
[System.Serializable]
internal class RenderCachePlayableAsset : BaseTimelineClipSISDataPlayableAsset, ITimelineClipAsset {
   
//----------------------------------------------------------------------------------------------------------------------


    public ClipCaps clipCaps {
        get {
            return ClipCaps.None;
        }
    }
   
    internal float GetOrUpdateDimensionRatio() {
        if (Mathf.Approximately(0.0f, m_dimensionRatio)) {
            ForceUpdateResolution();
        }
            
        return m_dimensionRatio;
    }
    
    
//----------------------------------------------------------------------------------------------------------------------
    internal string GetFolder() { return m_folder;}
    internal void SetFolder(string folder) { m_folder = folder;}
    internal void SetImageFileNames(List<string> imageFileNames) { m_imageFileNames = imageFileNames;} 
    
//----------------------------------------------------------------------------------------------------------------------
    
    /// <inheritdoc/>
    public sealed override Playable CreatePlayable(PlayableGraph graph, GameObject go) {
        return Playable.Null;
    }

//----------------------------------------------------------------------------------------------------------------------    
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
    
    void UpdateResolution(ref ImageData imageData) {
        ImageDimensionInt resolution;
        resolution.Width  = imageData.Width;
        resolution.Height = imageData.Height;
        m_dimensionRatio    = resolution.CalculateRatio();
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
    [SerializeField] private string m_folder = null;
    [HideInInspector][SerializeField] List<string> m_imageFileNames = null; //file names, not paths
    
//----------------------------------------------------------------------------------------------------------------------    
    private float m_dimensionRatio = 0;

}

} //end namespace

