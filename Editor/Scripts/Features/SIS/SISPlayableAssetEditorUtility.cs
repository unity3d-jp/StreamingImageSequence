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

        float newDuration = numImages / newFPS;
        sisClipData.SetCurveDurationInEditor(newDuration);
        clip.duration = newDuration;
    }


    
}

} //ena namespace
