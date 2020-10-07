using System.IO;
using NUnit.Framework;
using Unity.StreamingImageSequence.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.StreamingImageSequence.EditorTests {

internal class ResourcesTest {
    
    [Test]
    public void VerifyShaders() {

        string path = SISEditorConstants.TRANSPARENT_BG_COLOR_SHADER_PATH;
        Assert.IsTrue(File.Exists(path));
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
        Assert.IsNotNull(shader);        

    }


}

} //end namespace
