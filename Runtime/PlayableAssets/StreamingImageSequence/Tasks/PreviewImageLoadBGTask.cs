namespace UnityEngine.StreamingImageSequence {

    internal class PreviewImageLoadBGTask : BackGroundTask {

    internal static void Queue(string strFileName, int width, int height, int frame) {
        PreviewImageLoadBGTask task = new PreviewImageLoadBGTask(strFileName, width, height, frame);
        UpdateManager.QueueBackGroundTask(task);
        
    }

//----------------------------------------------------------------------------------------------------------------------
    private PreviewImageLoadBGTask( string fileName, int width, int height, int frame) {
        m_fileName = fileName;
        m_width = width;
        m_height = height;
        m_frame = frame;
    }

//----------------------------------------------------------------------------------------------------------------------

    public override void Execute() {
        const int TEX_TYPE = StreamingImageSequenceConstants.IMAGE_TYPE_PREVIEW;
        StreamingImageSequencePlugin.GetImageDataInto(m_fileName, TEX_TYPE, m_frame, out ImageData tResult);
        switch (tResult.ReadStatus) {
            case StreamingImageSequenceConstants.READ_STATUS_NONE: {
                //Debug.Log("Loading: " + m_fileName);
                StreamingImageSequencePlugin.LoadAndAllocPreviewImage(m_fileName, m_width, m_height, m_frame);
                break;
            }
            case StreamingImageSequenceConstants.READ_STATUS_LOADING: {
#if UNITY_EDITOR
                LogUtility.LogDebug("Already requested:" + m_fileName);
#endif
                break;
            }
            default: break;
        }

    }

//----------------------------------------------------------------------------------------------------------------------

    readonly string m_fileName;
    private readonly int m_width;
    private readonly int m_height;
    private readonly int m_frame;

}
} //end namespace
