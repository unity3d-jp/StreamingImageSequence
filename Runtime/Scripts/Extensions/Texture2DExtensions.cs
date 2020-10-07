using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.StreamingImageSequence {

//[TODO-sin: 2020-10-7] Move to anime-toolbox
internal static class Texture2DExtensions {
    public static void SetPixelsWithColor(this Texture2D tex, Color color) {
        
        Color[] pixels =  tex.GetPixels();
 
        for(var i = 0; i < pixels.Length; ++i) {
            pixels[i] = color;
        }  
        tex.SetPixels( pixels );  
        tex.Apply();                
    }    
}

} //end namespace
