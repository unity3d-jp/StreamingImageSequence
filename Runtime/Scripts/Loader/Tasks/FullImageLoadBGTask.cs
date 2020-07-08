namespace UnityEngine.StreamingImageSequence {


internal class FullImageLoadBGTask : BaseImageLoadBGTask, IBackGroundTask {

//----------------------------------------------------------------------------------------------------------------------
    internal static void Queue(string imagePath, int frame) {
        FullImageLoadBGTask task = new FullImageLoadBGTask(imagePath, frame);
        UpdateManager.QueueBackGroundTask(task);
        
    }

//----------------------------------------------------------------------------------------------------------------------
    private FullImageLoadBGTask( string imagePath, int frame)  : base(imagePath, frame){
    }

//----------------------------------------------------------------------------------------------------------------------

    public void Execute() {
        const int IMAGE_TYPE = StreamingImageSequenceConstants.IMAGE_TYPE_FULL;
        string imagePath = GetImagePath();
        int requestFrame = GetRequestFrame();
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

}

} //end namespace
