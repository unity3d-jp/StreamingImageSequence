using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

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
    
    internal static void SetFPS(StreamingImageSequencePlayableAsset sisPlayableAsset, float newFPS) {
        SISClipData sisClipData = sisPlayableAsset.GetBoundClipData();
        Assert.IsNotNull(sisClipData);
        TimelineClip clip = sisClipData.GetOwner();
        Assert.IsNotNull(clip);

        int numImages = sisPlayableAsset.GetNumImages();
        if (numImages <= 0) {
            Debug.LogWarning("[SIS] There are no images in folder: " + sisPlayableAsset.GetFolder());
            return;            
        }

        double prevTimeScale = clip.timeScale;        
        float  clipTimeScale = (newFPS * sisClipData.CalculateCurveDuration() / numImages);
        clip.timeScale = clipTimeScale;
        clip.duration  = clip.duration * (prevTimeScale / clipTimeScale);
    }


    
}

} //ena namespace
