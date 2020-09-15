using NUnit.Framework;
using Unity.StreamingImageSequence.Editor;

namespace Unity.StreamingImageSequence.EditorTests {

internal class UnityEditorReflectionTest {

    [Test]
    public void VerifyReflectedMethods() {
        
        Assert.IsNotNull(UnityEditorReflection.SCROLLABLE_TEXT_AREA_METHOD);
        

    }


}

} //end namespace
