using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.StreamingImageSequence {

internal static class ImageDataExtensions {

    public static Texture2D CreateCompatibleTexture(this ImageData imageData,HideFlags hideFlags, 
        FilterMode filterMode = FilterMode.Bilinear) 
    {
        Assert.IsTrue(StreamingImageSequenceConstants.READ_STATUS_SUCCESS == imageData.ReadStatus);
                
        TextureFormat textureFormat 
            = (imageData.Format == StreamingImageSequenceConstants.IMAGE_FORMAT_BGRA32) 
            ? TextureFormat.BGRA32
            : TextureFormat.RGBA32;        
       
        Texture2D tex = new Texture2D(imageData.Width, imageData.Height, textureFormat, false, false) {
            hideFlags = hideFlags,
            filterMode = filterMode, 
        };

        return tex;
    }

//----------------------------------------------------------------------------------------------------------------------
    
    public static bool IsTextureCompatible(this ImageData imageData, Texture2D tex) {
        return imageData.Width == tex.width && imageData.Height == tex.height
            && ((imageData.Format == StreamingImageSequenceConstants.IMAGE_FORMAT_BGRA32 && tex.format == TextureFormat.BGRA32) 
                || tex.format == TextureFormat.RGBA32);

    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    public static void CopyBufferToTexture(this ImageData imageData, Texture2D tex) {
        int length = imageData.Width * imageData.Height * 4;
        unsafe {
            void* src = imageData.RawData.ToPointer();
            NativeArray<float> rawTextureData = tex.GetRawTextureData<float>();
            void* dest = rawTextureData.GetUnsafePtr();
            Buffer.MemoryCopy(src, dest, length, length);
        }
        tex.Apply();
            
    }        
};


} // end namespace
