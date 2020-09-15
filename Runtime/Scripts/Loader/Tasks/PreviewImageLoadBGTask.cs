

using UnityEngine.Assertions;

namespace Unity.StreamingImageSequence {

internal class PreviewImageLoadBGTask : BaseImageLoadBGTask {
    
    internal PreviewImageLoadBGTask( string imagePath,int width, int height) : base(imagePath){
        m_width = width;
        m_height = height;
    }

//----------------------------------------------------------------------------------------------------------------------

    public override void Execute() {
        string imagePath    = GetImagePath();
        int    requestFrame = GetRequestFrame();
        
#if DEBUG_PREVIEW_IMAGE
        const int TEX_TYPE = StreamingImageSequenceConstants.IMAGE_TYPE_PREVIEW;
        StreamingImageSequencePlugin.GetImageDataInto(imagePath, TEX_TYPE, requestFrame, out ImageData imageData);
        Assert.AreNotEqual(StreamingImageSequenceConstants.READ_STATUS_LOADING, imageData.ReadStatus );
        Assert.AreNotEqual(StreamingImageSequenceConstants.READ_STATUS_SUCCESS, imageData.ReadStatus );
#endif
        
        StreamingImageSequencePlugin.LoadAndAllocPreviewImage(imagePath, m_width, m_height, requestFrame);
    }

//----------------------------------------------------------------------------------------------------------------------

    private readonly int m_width;
    private readonly int m_height;

}
} //end namespace
