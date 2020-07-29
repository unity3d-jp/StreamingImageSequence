
using System;

namespace UnityEngine.StreamingImageSequence
{

internal static class MathUtility {

    internal static int GetNumDigits(int number) {
        //Check 0 and negative
        number = (0 == number) ? 1 : number;
                
        double logResult = Math.Log10(Math.Abs(number)) + 1;        
        return (int) Math.Floor(logResult);        
    }       

}

} //end namespace
