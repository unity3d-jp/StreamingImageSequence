using UnityEngine.Assertions;

namespace Unity.StreamingImageSequence {

internal static class BitUtility {
        
//----------------------------------------------------------------------------------------------------------------------    
    internal static void ConvertToByte(long number, ref byte[] bytes) {
        const int NUM_LONG_BYTES = sizeof(long);
        Assert.AreEqual(NUM_LONG_BYTES, bytes.Length);
        for (int i=0;i<NUM_LONG_BYTES;++i) {
            bytes[i] = (byte)(number >> (8*i) & 0xFF);
        }
    }

//----------------------------------------------------------------------------------------------------------------------    
    internal static bool IsSet(int a, int b) {
        return (a & b) == b;
        
    }
}

} //end namespace