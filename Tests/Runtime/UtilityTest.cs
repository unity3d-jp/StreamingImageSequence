using NUnit.Framework;

namespace UnityEngine.StreamingImageSequence.Tests {

public class UtilityTest {


//----------------------------------------------------------------------------------------------------------------------    
    
    [Test]
    public void VerifyNumDigits() {
        Assert.AreEqual(1,MathUtility.GetNumDigits(0));
        Assert.AreEqual(1,MathUtility.GetNumDigits(1));
        Assert.AreEqual(1,MathUtility.GetNumDigits(9));

        Assert.AreEqual(2,MathUtility.GetNumDigits(10));
        Assert.AreEqual(2,MathUtility.GetNumDigits(11));
        Assert.AreEqual(2,MathUtility.GetNumDigits(99));
        
        Assert.AreEqual(3,MathUtility.GetNumDigits(100));
        Assert.AreEqual(3,MathUtility.GetNumDigits(101));
        Assert.AreEqual(3,MathUtility.GetNumDigits(199));
        Assert.AreEqual(3,MathUtility.GetNumDigits(999));
        
        //Negative
        Assert.AreEqual(1,MathUtility.GetNumDigits(-1));
        Assert.AreEqual(1,MathUtility.GetNumDigits(-9));

        Assert.AreEqual(2,MathUtility.GetNumDigits(-10));
        Assert.AreEqual(2,MathUtility.GetNumDigits(-11));
        Assert.AreEqual(2,MathUtility.GetNumDigits(-99));
        
        Assert.AreEqual(3,MathUtility.GetNumDigits(-100));
        Assert.AreEqual(3,MathUtility.GetNumDigits(-101));
        Assert.AreEqual(3,MathUtility.GetNumDigits(-199));
        Assert.AreEqual(3,MathUtility.GetNumDigits(-999));
        
    }
    

}

} //end namespace
