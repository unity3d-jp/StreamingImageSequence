using JetBrains.Annotations;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Assertions;

namespace Unity.StreamingImageSequence.Editor {

[CustomTimelineEditor(typeof(RenderCachePlayableAsset)), UsedImplicitly]
internal class RenderCachePlayableAssetEditor : ImageFolderPlayableAssetEditor<RenderCacheClipData>
{


//----------------------------------------------------------------------------------------------------------------------

    /// <inheritdoc/>
    public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom) {
        RenderCachePlayableAsset asset = clip.asset as RenderCachePlayableAsset;
        Assert.IsNotNull(asset);

        clip.TryMoveToTrack(track); 
        
        RenderCacheClipData sisData = new RenderCacheClipData(clip);        
        asset.BindClipData(sisData);
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    protected override void DrawPreviewImageV(ref PreviewDrawInfo drawInfo, TimelineClip clip, 
        ImageFolderPlayableAsset<RenderCacheClipData> renderCachePlayableAsset) 
    {        
        double        normalizedLocalTime = drawInfo.LocalTime / clip.duration;
        int           numImages           = renderCachePlayableAsset.GetNumImages();
        Assert.IsTrue(numImages > 0);
        
        //Can't round up, because if the time for the next frame hasn't been reached, then we should stick 
        int index = Mathf.FloorToInt(numImages * (float) normalizedLocalTime);
        index = Mathf.Clamp(index, 0, numImages - 1);        
                        
        //Draw
        string imagePath = renderCachePlayableAsset.GetImageFilePath(index);
        PreviewUtility.DrawPreviewImage(ref drawInfo, imagePath);
        
        
    }
    
}

}
