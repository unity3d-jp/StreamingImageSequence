
namespace UnityEngine.StreamingImageSequence {

internal class PreviewImageLoadBGTask : ImageLoadInfo, IBackGroundTask {

    internal static void Queue(string imagePath, int width, int height, int frame) {
        PreviewImageLoadBGTask task = new PreviewImageLoadBGTask(imagePath, frame, width, height);
        UpdateManager.QueueBackGroundTask(task);
        
    }

//----------------------------------------------------------------------------------------------------------------------
    private PreviewImageLoadBGTask( string imagePath, int frame, int width, int height) : base(imagePath,frame){
        m_width = width;
        m_height = height;
    }

//----------------------------------------------------------------------------------------------------------------------

    public void Execute() {
        const int TEX_TYPE = StreamingImageSequenceConstants.IMAGE_TYPE_PREVIEW;
        string imagePath    = GetImagePath();
        int    requestFrame = GetRequestFrame();
        
        StreamingImageSequencePlugin.GetImageDataInto(imagePath, TEX_TYPE, requestFrame, out ImageData tResult);
        switch (tResult.ReadStatus) {
            case StreamingImageSequenceConstants.READ_STATUS_LOADING: 
            case StreamingImageSequenceConstants.READ_STATUS_SUCCESS: {
#if UNITY_EDITOR
                LogUtility.LogDebug("Already requested:" + imagePath);
#endif
                break;
            }
            default: {
                //Debug.Log("Loading: " + m_fileName);
                StreamingImageSequencePlugin.LoadAndAllocPreviewImage(imagePath, m_width, m_height, requestFrame);
                break;
            }
        }

    }

//----------------------------------------------------------------------------------------------------------------------

    private readonly int m_width;
    private readonly int m_height;

}
} //end namespace
