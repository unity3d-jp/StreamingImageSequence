namespace Unity.StreamingImageSequence {


internal static class SISPlayableAssetUtility {

//----------------------------------------------------------------------------------------------------------------------

    internal static float CalculateFPS(StreamingImageSequencePlayableAsset sisPlayableAsset) {
        double      clipTimeScale = 1;
        SISClipData sisClipData   = sisPlayableAsset.GetBoundClipData();
        if (null == sisClipData) {
            return 0;
        }
        
        if (null!=sisClipData.GetOwner()) {
            clipTimeScale = sisClipData.GetOwner().timeScale;
        }
        
        int   numImages = sisPlayableAsset.GetNumImages();
        float fps       = (float) (numImages * clipTimeScale / sisClipData.CalculateCurveDuration());
        return fps;
    }
    
}

} //ena namespace
