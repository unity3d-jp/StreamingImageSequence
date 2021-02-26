using System;
using UnityEngine;

namespace Unity.StreamingImageSequence {


internal static class ImageLoader  {
   
    
    [RuntimeInitializeOnLoadMethod]
    static void ImageLoaderOnRuntimeLoad() {
        StreamingImageSequencePlugin.ResetImageLoadOrder();
    }
//----------------------------------------------------------------------------------------------------------------------   

    internal static void SetImageLoadTaskHandler(Func<int, BaseImageLoadBGTask, bool> taskHandler) {
        m_imageLoadTaskHandler = taskHandler;
    }
    
//----------------------------------------------------------------------------------------------------------------------   

    internal static bool RequestLoadFullImage(string imagePath) {
                
        FullImageLoadBGTask task = new FullImageLoadBGTask(imagePath);
        return RequestLoadImageInternal(StreamingImageSequenceConstants.IMAGE_TYPE_FULL, task);
    }

    internal static bool RequestLoadPreviewImage(string imagePath, int width, int height) {
        PreviewImageLoadBGTask task = new PreviewImageLoadBGTask(imagePath, width, height);
        return RequestLoadImageInternal(StreamingImageSequenceConstants.IMAGE_TYPE_PREVIEW, task);
    }
//----------------------------------------------------------------------------------------------------------------------
    
     
    
    private static bool RequestLoadImageInternal(int imageType, BaseImageLoadBGTask imageLoadBGTask) {
               
        imageLoadBGTask.SetRequestFrame(GetCurrentFrame());

        if (null != m_imageLoadTaskHandler) {
            return m_imageLoadTaskHandler(imageType, imageLoadBGTask);
        }
        
        ThreadManager.QueueBackGroundTask(imageLoadBGTask);
        return true;
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    //Wrappers so that the code to decide the currentFrame is gathered in one place

    internal static void GetImageDataInto(string fileName, int imageType, out ImageData readResult) {
        StreamingImageSequencePlugin.GetImageDataInto(fileName,imageType, GetCurrentFrame(), 
            out readResult);

        //Display not enough memory warning.
        switch (readResult.ReadStatus) {
            case StreamingImageSequenceConstants.READ_STATUS_OUT_OF_MEMORY: {
                if (m_showWarningOnOOM) {
                    Debug.LogWarning("[SIS] Out of memory when loading images");
                    m_showWarningOnOOM = false;
                }
                break;
            }
            case StreamingImageSequenceConstants.READ_STATUS_SUCCESS: {
                m_showWarningOnOOM = true; 
                break;
            }
            default: {
                break;
            };
        }

    }

   
    internal static int GetCurrentFrame() {
        return Time.frameCount; //use Time.frameCount for both playMode and editMode
    }
    
//----------------------------------------------------------------------------------------------------------------------

    private static Func<int, BaseImageLoadBGTask, bool> m_imageLoadTaskHandler = null;
    
    private static bool m_showWarningOnOOM = true;
}

} //end namespace
