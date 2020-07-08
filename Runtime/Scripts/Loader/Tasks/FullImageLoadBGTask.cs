namespace UnityEngine.StreamingImageSequence {


internal class FullImageLoadBGTask : BackGroundTask {

//----------------------------------------------------------------------------------------------------------------------
    internal static void Queue(string imagePath, int frame) {
        FullImageLoadBGTask task = new FullImageLoadBGTask(new ImageLoadInfo(imagePath, frame));
        UpdateManager.QueueBackGroundTask(task);
        
    }

//----------------------------------------------------------------------------------------------------------------------
    private FullImageLoadBGTask( ImageLoadInfo imageLoadInfo) {
        m_imageLoadInfo = imageLoadInfo; 
    }

//----------------------------------------------------------------------------------------------------------------------

    public override void Execute() {
        const int IMAGE_TYPE = StreamingImageSequenceConstants.IMAGE_TYPE_FULL;
        string imagePath = m_imageLoadInfo.GetImagePath();
        int requestFrame = m_imageLoadInfo.GetRequestFrame();
        StreamingImageSequencePlugin.GetImageDataInto(imagePath, IMAGE_TYPE, requestFrame, out ImageData tResult);
        switch (tResult.ReadStatus) {
            case StreamingImageSequenceConstants.READ_STATUS_LOADING: 
            case StreamingImageSequenceConstants.READ_STATUS_SUCCESS: {
#if UNITY_EDITOR
                LogUtility.LogDebug("Already requested:" + imagePath);
#endif
                break;
            }
            default: {
                //Debug.Log("Loading: " + m_strFileName);
                StreamingImageSequencePlugin.LoadAndAllocFullImage(imagePath,requestFrame);
                break;
                
            }
        }
    }
    
//----------------------------------------------------------------------------------------------------------------------
    private readonly ImageLoadInfo m_imageLoadInfo;

}

} //end namespace
