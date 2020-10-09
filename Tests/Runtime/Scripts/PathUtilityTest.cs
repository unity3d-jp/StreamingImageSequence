using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace Unity.StreamingImageSequence.Tests {

internal class PathUtilityTest {
    
    [Test]
    public void GenerateUniqueFolder() {
        string    path = Path.Combine(Application.streamingAssetsPath, "GenerateUniqueFolderTest");
        const int NUM_GENS = 10;
        for (int i = 0; i < NUM_GENS; ++i) {
            PathUtility.GenerateUniqueFolder(path);            
        }
        
        Assert.IsTrue(Directory.Exists(Path.Combine(Application.streamingAssetsPath, "GenerateUniqueFolderTest")));
        for (int i = 1; i < NUM_GENS; ++i) {
            string uniquePath = Path.Combine(Application.streamingAssetsPath, $"GenerateUniqueFolderTest {i}");
            Assert.IsTrue(Directory.Exists(uniquePath));
            Directory.Delete(uniquePath);
        }
        
        Directory.Delete(path);
    }


}

} //end namespace
