using JetBrains.Annotations;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using UnityEngine;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence {
[CustomTimelineEditor(typeof(RenderCachePlayableAsset)), UsedImplicitly]
internal class RenderCachePlayableAssetEditor : ClipEditor {


//----------------------------------------------------------------------------------------------------------------------

    /// <inheritdoc/>
    public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
    {
        RenderCachePlayableAsset asset = clip.asset as RenderCachePlayableAsset;
        if (null == asset) {
            Debug.LogError("Asset is not a RenderCachePlayableAsset: " + clip.asset);
            return;
        }

        clip.parentTrack = track; 
        asset.SetTimelineClip(clip);
    }


//----------------------------------------------------------------------------------------------------------------------

    /// <inheritdoc/>
    public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region) {
        base.DrawBackground(clip, region);
        
        //[TODO-sin: 2020-5-27]
        //Show preview icons
        
    }

//----------------------------------------------------------------------------------------------------------------------
    // readonly Dictionary<TimelineClip, StreamingImageSequencePreview> m_persistentPreviews 
    //     = new Dictionary<TimelineClip, StreamingImageSequencePreview>();
    //
    // ColorSpace m_colorSpace = ColorSpace.Uninitialized;

}

}
