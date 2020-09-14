using NUnit.Framework;

namespace UnityEditor.StreamingImageSequence.Tests {

internal class UnityEditorReflectionTest {

    [Test]
    public void VerifyReflectedMethods() {
        
        Assert.IsNotNull(UnityEditorReflection.SCROLLABLE_TEXT_AREA_METHOD);
        

    }


}

} //end namespace
