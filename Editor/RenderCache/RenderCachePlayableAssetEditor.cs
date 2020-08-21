﻿using JetBrains.Annotations;
using UnityEditor.Timeline;
using UnityEngine;
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

        Rect rect = region.position;
        if (rect.width <= SISEditorConstants.MIN_PREVIEW_REGION_WIDTH)
            return;

        RenderCachePlayableAsset curAsset = clip.asset as RenderCachePlayableAsset;
        if (null == curAsset)
            return;

        int numImages =curAsset.GetNumImages();        
        if (numImages <= 0) {
            return;
        }        
            
        if (Event.current.type == EventType.Repaint) {
            PreviewClipInfo clipInfo = new PreviewClipInfo() {
                Duration              = clip.duration,
                TimeScale             = clip.timeScale,
                ClipIn                = clip.clipIn,
                FramePerSecond        = clip.parentTrack.timelineAsset.editorSettings.fps,
                ImageDimensionRatio   = curAsset.GetOrUpdateDimensionRatio(),
                VisibleLocalStartTime =  region.startTime,
                VisibleLocalEndTime   = region.endTime,
                VisibleRect           = rect,
            }; 
                
            PreviewUtility.EnumeratePreviewImages(ref clipInfo, (PreviewDrawInfo drawInfo) => {
                DrawPreviewImage(ref drawInfo, clip, curAsset);
            });
            
            //For hiding frame marker automatically
            int numFrames = Mathf.RoundToInt((float)
                ((region.endTime - region.startTime) * clipInfo.FramePerSecond / clipInfo.TimeScale) 
            );
                
            double              widthPerFrame       = rect.width / numFrames;
            TimelineClipSISData timelineClipSISData = curAsset.GetBoundTimelineClipSISData();
            if (null != timelineClipSISData) {
                timelineClipSISData.SetTimelineWidthPerFrame(widthPerFrame);
            }
            
                
        }        
    }

//----------------------------------------------------------------------------------------------------------------------    
    
    void DrawPreviewImage(ref PreviewDrawInfo drawInfo, TimelineClip clip, 
        RenderCachePlayableAsset renderCachePlayableAsset) 
    {        
        double        normalizedLocalTime = drawInfo.LocalTime / clip.duration;
        int           numImages           = renderCachePlayableAsset.GetNumImages();
        Assert.IsTrue(numImages > 0);
        
        int index = Mathf.RoundToInt(numImages * (float) normalizedLocalTime);
        index = Mathf.Clamp(index, 0, numImages - 1);        
                        
        //Draw
        string imagePath = renderCachePlayableAsset.GetImageFilePath(index);
        PreviewUtility.DrawPreviewImage(ref drawInfo, imagePath);
        
        
    }
    
//----------------------------------------------------------------------------------------------------------------------

}

}
