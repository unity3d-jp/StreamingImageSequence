using UnityEngine;

namespace Unity.StreamingImageSequence {

internal static class TextureUtility {
    
    internal static Texture2D CreateTexture2D(Texture2D other, HideFlags hFlags = HideFlags.None) {
        Texture2D tex = new Texture2D(other.width, other.height, other.format, false, false) {
            filterMode = FilterMode.Bilinear,
            hideFlags = hFlags,
        };
        return tex;

    }
}

} //end namespace
