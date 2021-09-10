using NUnit.Framework;
using Unity.StreamingImageSequence.Editor;

namespace Unity.StreamingImageSequence.EditorTests {

internal class UnityEditorReflectionTest {

    [Test]
    public void VerifyReflectedMethods() {        
        Assert.IsNotNull(UnityEditorReflection.SCROLLABLE_TEXT_AREA_METHOD);
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    [Test]
    public void VerifyReflectedTypes() {        
        Assert.IsNotNull(UnityEditorReflection.TIMELINE_EDITOR_CLIP_TYPE);
    }
    
//----------------------------------------------------------------------------------------------------------------------

    [Test]
    public void VerifyReflectedProperties() {        
        Assert.IsNotNull(UnityEditorReflection.TIMELINE_EDITOR_CLIP_PROPERTY);
    }
    
}

} //end namespace
