using UnityEngine;

namespace Unity.StreamingImageSequence {

internal static class Texture2DExtensions {

    internal static bool AreSizeAndFormatEqual(this Texture2D tex, Texture2D other) {
        return (tex.width == other.width && tex.height == other.height && tex.format == other.format);
    }
    
}

} //end namespace
