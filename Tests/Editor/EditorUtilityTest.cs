using System.Collections;
using NUnit.Framework;
using System.IO;
using UnityEngine;
using UnityEngine.StreamingImageSequence;
using UnityEngine.TestTools;

namespace UnityEditor.StreamingImageSequence.Tests {

internal class EditorUtilityTest {

    [UnityTest]
    public IEnumerator DeleteAssetsInsideDataPath() {
        
        string uniqueName = Path.GetFileName(FileUtil.GetUniqueTempPathInProject());
        Assert.IsFalse(string.IsNullOrEmpty(uniqueName));
        string destFolder = Path.Combine(Application.dataPath, uniqueName);

        yield return CopyAndDeleteSampleAsset(destFolder);

    }

//----------------------------------------------------------------------------------------------------------------------    
    
    [UnityTest]
    public IEnumerator DeleteAssetsOutsideDataPath() {

        string destFolder = FileUtil.GetUniqueTempPathInProject();
        yield return CopyAndDeleteSampleAsset(destFolder);

    }

//----------------------------------------------------------------------------------------------------------------------    
    
    [Test]
    public void GetMainViewGameSize() {

        ViewEditorUtility.GetMainGameViewSize();        

    }
//----------------------------------------------------------------------------------------------------------------------    
    private IEnumerator CopyAndDeleteSampleAsset(string destFolder) {
        
        //
        Assert.IsTrue(File.Exists(SRC_IMAGE_PATH));
        string uniqueName = Path.GetFileName(FileUtil.GetUniqueTempPathInProject());
        Assert.IsFalse(string.IsNullOrEmpty(uniqueName));


        Directory.CreateDirectory(destFolder);
        int numDuplicates = 10;
        int numDigits     = MathUtility.GetNumDigits(numDuplicates);
        for (int i = 0; i < numDuplicates; ++i) {
            string destFileName = i.ToString($"D{numDigits}") + ".png";
            string destPath     = Path.Combine(destFolder, destFileName);
            File.Copy(SRC_IMAGE_PATH, destPath);
            
            Assert.IsTrue(File.Exists(destPath));            
            
        }
        yield return null;
        AssetEditorUtility.DeleteAssets(destFolder, "*.png");
        
        yield return null;
        string[] files = Directory.GetFiles(destFolder);
        Assert.IsTrue(0 == files.Length);
        
        
        Directory.Delete(destFolder);

    }
//----------------------------------------------------------------------------------------------------------------------    

    const string SRC_IMAGE_PATH = "Packages/com.unity.streaming-image-sequence/Tests/Data/png/A_00000.png";

}

} //end namespace
