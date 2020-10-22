using JetBrains.Annotations;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Timeline;
using UnityEngine.Assertions;

namespace Unity.StreamingImageSequence.Editor {

[CustomTimelineEditor(typeof(RenderCachePlayableAsset)), UsedImplicitly]
internal class RenderCachePlayableAssetEditor : ImageFolderPlayableAssetEditor<RenderCachePlayableAsset> 
{


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
        
        DrawBackground(rect, curAsset.GetTimelineBGColor());

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
            TimelineClipSISData timelineClipSISData = curAsset.GetBoundTimelineClipSISData();
            if (null != timelineClipSISData) {                
                timelineClipSISData.UpdateTimelineWidthPerFrame(rect.width, region.endTime-region.startTime, 
                    clipInfo.FramePerSecond, clipInfo.TimeScale);
            }
            
                
        }        
    }
//----------------------------------------------------------------------------------------------------------------------
    void DrawBackground(Rect rect, Color color) {
        Texture2D bgTexture      = GetOrCreateBGTexture();
        bgTexture.SetPixelsWithColor(color);
        Graphics.DrawTexture(rect, bgTexture);
    }

//----------------------------------------------------------------------------------------------------------------------    
    
    void DrawPreviewImage(ref PreviewDrawInfo drawInfo, TimelineClip clip, 
        RenderCachePlayableAsset renderCachePlayableAsset) 
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
    
//----------------------------------------------------------------------------------------------------------------------

    Texture2D GetOrCreateBGTexture() {
        if (null != m_bgTexture)
            return m_bgTexture;
        
        m_bgTexture           = new Texture2D(1,1, TextureFormat.ARGB32,false);
        m_bgTexture.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
        return m_bgTexture;
    }
//----------------------------------------------------------------------------------------------------------------------
    
    private static Texture2D m_bgTexture         = null;

}

}
