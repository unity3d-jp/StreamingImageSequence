using UnityEngine.Assertions;

namespace UnityEngine.StreamingImageSequence {

internal static class BitUtility {
        
//----------------------------------------------------------------------------------------------------------------------    
    internal static void ConvertToByte(long number, ref byte[] bytes) {
        const int NUM_LONG_BYTES = sizeof(long);
        Assert.AreEqual(NUM_LONG_BYTES, bytes.Length);
        for (int i=0;i<NUM_LONG_BYTES;++i) {
            bytes[i] = (byte)(number >> (8*i) & 0xFF);
        }
    }
}

} //end namespace