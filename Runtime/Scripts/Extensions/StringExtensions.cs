namespace Unity.StreamingImageSequence {

internal static class StringExtensions {

    internal static bool IsRegularAssetPath(this string path) {
        return !string.IsNullOrEmpty(path) && path.StartsWith("Assets/") && !path.StartsWith("Assets/StreamingAssets");
    }
    
}

} //end namespace
