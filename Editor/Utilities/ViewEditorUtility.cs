using NUnit.Framework;
using UnityEngine;

namespace UnityEditor.StreamingImageSequence {

internal static class ViewEditorUtility 
{
    internal static Vector2 GetMainGameViewSize() {
        System.Type                  t                     = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        Assert.IsNotNull(t);
        System.Reflection.MethodInfo getSizeOfMainGameView = t.GetMethod("GetSizeOfMainGameView",System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        Assert.IsNotNull(getSizeOfMainGameView);
        System.Object                res                   = getSizeOfMainGameView.Invoke(null,null);
        Assert.IsNotNull(res);
        return (Vector2)res;
    }    
}

} //end namespace

