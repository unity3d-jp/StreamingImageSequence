﻿using Unity.FilmInternalUtilities;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

namespace Unity.StreamingImageSequence.Editor {

internal abstract class ImageFolderPlayableAssetEditor<T> : ClipEditor where T: PlayableFrameClipData{

    //Called when a clip is changed by the Editor. (TrimStart, TrimEnd, etc)    
    public override void OnClipChanged(TimelineClip clip) {       
        base.OnClipChanged(clip);
                        
        ImageFolderPlayableAsset<T> imageFolderPlayableAsset = clip.asset as ImageFolderPlayableAsset<T>;
        Assert.IsNotNull(imageFolderPlayableAsset);
        imageFolderPlayableAsset.RefreshPlayableFrames();            
    }

//----------------------------------------------------------------------------------------------------------------------
    
    /// <inheritdoc/>
    public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region) {
        base.DrawBackground(clip, region);

        Rect rect = region.position;
        if (rect.width <= SISEditorConstants.MIN_PREVIEW_REGION_WIDTH)
            return;

        ImageFolderPlayableAsset<T> curAsset = clip.asset as ImageFolderPlayableAsset<T>;
        if (null == curAsset)
            return;
        
        DrawBackgroundTexture(rect, curAsset.GetTimelineBGColor());

        int numImages =curAsset.GetNumImages();        
        if (numImages <= 0) {
            return;
        }        
            
        if (Event.current.type == EventType.Repaint) {
            PreviewClipInfo clipInfo = new PreviewClipInfo() {
                Duration              = clip.duration,
                TimeScale             = clip.timeScale,
                ClipIn                = clip.clipIn,
                FramePerSecond        = clip.GetParentTrack().timelineAsset.editorSettings.GetFPS(),
                ImageDimensionRatio   = curAsset.GetOrUpdateDimensionRatio(),
                VisibleLocalStartTime =  region.startTime,
                VisibleLocalEndTime   = region.endTime,
                VisibleRect           = rect,
            }; 
                
            PreviewUtility.EnumeratePreviewImages(ref clipInfo, (PreviewDrawInfo drawInfo) => {
                DrawPreviewImageV(ref drawInfo, clip, curAsset);
            });
            
            //For hiding frame marker automatically
            PlayableFrameClipData clipData = curAsset.GetBoundClipData();
            if (null != clipData) {                
                clipData.UpdateTimelineWidthPerFrame(rect.width, region.endTime-region.startTime, 
                    clipInfo.FramePerSecond, clipInfo.TimeScale);
            }
            
                
        }        
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    protected abstract void DrawPreviewImageV(ref PreviewDrawInfo drawInfo, TimelineClip clip,
        ImageFolderPlayableAsset<T> playableAsset); 

//----------------------------------------------------------------------------------------------------------------------
    void DrawBackgroundTexture(Rect rect, Color color) {
        Texture2D bgTexture = EditorTextures.GetOrCreatePreviewBGTexture();
        bgTexture.SetPixelsWithColor(color);
        Graphics.DrawTexture(rect, bgTexture);
    }

//----------------------------------------------------------------------------------------------------------------------
    
    
}

}
