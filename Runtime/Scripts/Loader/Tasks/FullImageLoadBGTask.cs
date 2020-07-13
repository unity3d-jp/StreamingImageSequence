using UnityEngine.Assertions;

namespace UnityEngine.StreamingImageSequence {


internal class FullImageLoadBGTask : BaseImageLoadBGTask {

//----------------------------------------------------------------------------------------------------------------------
    internal FullImageLoadBGTask( string imagePath)  : base(imagePath){
    }

//----------------------------------------------------------------------------------------------------------------------

    public override void Execute() {
        string imagePath = GetImagePath();
        int requestFrame = GetRequestFrame();
        
#if DEBUG_FULL_IMAGE

        const int IMAGE_TYPE = StreamingImageSequenceConstants.IMAGE_TYPE_FULL;
        StreamingImageSequencePlugin.GetImageDataInto(imagePath, IMAGE_TYPE, requestFrame, out ImageData imageData);
        Assert.AreNotEqual(StreamingImageSequenceConstants.READ_STATUS_LOADING, imageData.ReadStatus );
        Assert.AreNotEqual(StreamingImageSequenceConstants.READ_STATUS_SUCCESS, imageData.ReadStatus );
#endif
        
        StreamingImageSequencePlugin.LoadAndAllocFullImage(imagePath,requestFrame);
        
    }
    
//----------------------------------------------------------------------------------------------------------------------

}

} //end namespace
