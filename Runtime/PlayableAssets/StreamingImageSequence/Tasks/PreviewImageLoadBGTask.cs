namespace UnityEngine.StreamingImageSequence {

    internal class PreviewImageLoadBGTask : BackGroundTask {

    internal static void Queue(string strFileName, int width, int height) {
        PreviewImageLoadBGTask task = new PreviewImageLoadBGTask(strFileName, width, height);
        UpdateManager.QueueBackGroundTask(task);
        
    }

//----------------------------------------------------------------------------------------------------------------------
    private PreviewImageLoadBGTask( string fileName, int width, int height) {
        m_fileName = fileName;
        m_width = width;
        m_height = height;
    }

//----------------------------------------------------------------------------------------------------------------------

    public override void Execute() {
        const int TEX_TYPE = StreamingImageSequenceConstants.IMAGE_TYPE_PREVIEW;
        StreamingImageSequencePlugin.GetImageData(m_fileName, TEX_TYPE, out ImageData tResult);
        switch (tResult.ReadStatus) {
            case StreamingImageSequenceConstants.READ_STATUS_NONE: {
                //Debug.Log("Loading: " + m_fileName);
                StreamingImageSequencePlugin.LoadAndAllocPreviewImage(m_fileName, m_width, m_height);
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

    string m_fileName;
    private int m_width;
    private int m_height;

}
} //end namespace
