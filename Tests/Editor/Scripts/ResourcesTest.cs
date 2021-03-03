using System.IO;
using NUnit.Framework;
using Unity.StreamingImageSequence.Editor;
using UnityEditor;
using UnityEngine;

namespace Unity.StreamingImageSequence.EditorTests {

internal class ResourcesTest {
    
    [Test]
    public void VerifyShaders() {
        Assert.IsTrue(IsShaderValid(StreamingImageSequenceConstants.TRANSPARENT_BG_COLOR_SHADER_PATH));
        Assert.IsTrue(IsShaderValid(StreamingImageSequenceConstants.LINEAR_TO_GAMMA_SHADER_PATH));
    }
    
//----------------------------------------------------------------------------------------------------------------------
    private bool IsShaderValid(string path) {
        if (!File.Exists(path)) {
            return false;
        }
        
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
        return (null != shader);        
    }
}

} //end namespace
