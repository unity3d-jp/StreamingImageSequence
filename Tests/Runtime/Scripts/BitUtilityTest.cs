using System;
using NUnit.Framework;

namespace Unity.StreamingImageSequence.Tests {

internal class BitUtilityTest {
    
    [Test]
    public void VerifyLongToByteConverter() {
        long[] testValues = new[] {
            123456789L,
            333L,
            987654321L,
            55555L, 
            192837465L,
        };

        const int NUM_LONG_BYTES = sizeof(long);
        byte[] customBytes = new byte[NUM_LONG_BYTES];
        foreach (long v in testValues) {

            BitUtility.ConvertToByte(v, ref customBytes);        
            byte[] bitConverterBytes = BitConverter.GetBytes(v);
        
            Assert.AreEqual(bitConverterBytes.Length, customBytes.Length);
            for (int i = 0; i < NUM_LONG_BYTES; ++i) {
                Assert.AreEqual(bitConverterBytes[i], customBytes[i]);
            }
            
        }
        
        
    }


}

} //end namespace
