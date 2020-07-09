using System.Collections.Generic;

namespace UnityEngine.StreamingImageSequence {


internal class ImageLoadController  {

    ImageLoadController GetInstance(int imageType) {
        return m_imageLoadControllers[imageType];
    } 

//----------------------------------------------------------------------------------------------------------------------
    internal void Queue(string imagePath, ITask task) {
        
        if (Application.isPlaying)        
            ThreadManager.QueueBackGroundTask(task);
        else {
            
        }
        
    }

//----------------------------------------------------------------------------------------------------------------------

    internal void UpdateInEditor() {
        
        
    }

//----------------------------------------------------------------------------------------------------------------------
    // private readonly Queue<PreviewImageLoadInfo> m_loadQueue = new Queue<PreviewImageLoadInfo>();
    //
    // private readonly Dictionary<string, PreviewImageLoadInfo> m_loadDictionary =
    //     new Dictionary<string, PreviewImageLoadInfo>();
    
//----------------------------------------------------------------------------------------------------------------------

    private static readonly ImageLoadController[] m_imageLoadControllers 
        = new ImageLoadController[StreamingImageSequenceConstants.MAX_IMAGE_TYPES];
}

} //end namespace
