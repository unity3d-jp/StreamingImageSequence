using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

namespace Unity.StreamingImageSequence.Editor {

internal static class SISPlayableAssetEditorUtility {

//----------------------------------------------------------------------------------------------------------------------


    
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

        double prevClipDuration  = clip.duration;
        
        float newCurveDuration = numImages / newFPS;
        sisClipData.SetCurveDurationInEditor(newCurveDuration, out float prevCurveDuration);

        //The curve duration might not be same as the clip duration, so we need to consider it when scaling.
        clip.duration = (newCurveDuration / prevCurveDuration) * prevClipDuration;
    }


    
}

} //ena namespace
