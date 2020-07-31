using JetBrains.Annotations;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using UnityEngine.Assertions;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence {

[CustomTimelineEditor(typeof(RenderCachePlayableAsset)), UsedImplicitly]
internal class RenderCachePlayableAssetEditor : ClipEditor {


//----------------------------------------------------------------------------------------------------------------------

    /// <inheritdoc/>
    public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom) {
        RenderCachePlayableAsset asset = clip.asset as RenderCachePlayableAsset;
        Assert.IsNotNull(asset);

        clip.parentTrack = track; 
        
        TimelineClipSISData sisData = new TimelineClipSISData(clip);        
        asset.BindTimelineClipSISData(sisData);
    }


//----------------------------------------------------------------------------------------------------------------------
    
    /// <inheritdoc/>
    public override void OnClipChanged(TimelineClip clip) {
        base.OnClipChanged(clip);
                        
        RenderCachePlayableAsset renderCachePlayableAsset = clip.asset as RenderCachePlayableAsset;
        Assert.IsNotNull(renderCachePlayableAsset);
        renderCachePlayableAsset.RefreshPlayableFrames();            
    }
    
//----------------------------------------------------------------------------------------------------------------------

    /// <inheritdoc/>
    public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region) {
        base.DrawBackground(clip, region);
        
        //[TODO-sin: 2020-5-27]
        //Show preview icons
        
    }

//----------------------------------------------------------------------------------------------------------------------

}

}
