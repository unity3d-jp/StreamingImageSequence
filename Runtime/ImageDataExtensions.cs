using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;

namespace UnityEngine.StreamingImageSequence {

internal static class ImageDataExtensions {

    public static Texture2D CreateCompatibleTexture(this ImageData imageData) {
        Assert.IsTrue(StreamingImageSequenceConstants.READ_STATUS_SUCCESS == imageData.ReadStatus);
        
#if UNITY_STANDALONE_OSX
        const TextureFormat TEXTURE_FORMAT = TextureFormat.RGBA32;
#elif UNITY_STANDALONE_WIN
        const TextureFormat TEXTURE_FORMAT = TextureFormat.BGRA32;
#endif
        
        int length = imageData.Width * imageData.Height * 4;
        Texture2D tex = new Texture2D(imageData.Width, imageData.Height, TEXTURE_FORMAT, false, false) {
            filterMode = FilterMode.Bilinear
        };

        return tex;
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
