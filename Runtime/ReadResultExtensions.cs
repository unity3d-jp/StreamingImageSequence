using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine.Assertions;

namespace UnityEngine.StreamingImageSequence {

internal static class ReadResultExtensions {

    public static Texture2D CreateCompatibleTexture(this ReadResult readResult) {
        Assert.IsTrue(StreamingImageSequenceConstants.READ_RESULT_SUCCESS == readResult.ReadStatus);
        
#if UNITY_STANDALONE_OSX
        const TextureFormat TEXTURE_FORMAT = TextureFormat.RGBA32;
#elif UNITY_STANDALONE_WIN
        const TextureFormat TEXTURE_FORMAT = TextureFormat.BGRA32;
#endif
        
        int length = readResult.Width * readResult.Height * 4;
        Texture2D tex = new Texture2D(readResult.Width, readResult.Height, TEXTURE_FORMAT, false, false) {
            filterMode = FilterMode.Bilinear
        };

        return tex;
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    public static void CopyBufferToTexture(this ReadResult readResult, Texture2D tex) {
        int length = readResult.Width * readResult.Height * 4;
        unsafe {
            void* src = readResult.Buffer.ToPointer();
            NativeArray<float> rawTextureData = tex.GetRawTextureData<float>();
            void* dest = rawTextureData.GetUnsafePtr();
            Buffer.MemoryCopy(src, dest, length, length);
        }
        tex.Apply();
            
    }        
};


} // end namespace
