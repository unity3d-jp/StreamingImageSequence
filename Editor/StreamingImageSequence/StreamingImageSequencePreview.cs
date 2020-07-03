using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.StreamingImageSequence;
using UnityEngine.Timeline;

namespace UnityEditor.StreamingImageSequence {

internal class StreamingImageSequencePreview : IDisposable {

    public StreamingImageSequencePreview(StreamingImageSequencePlayableAsset playableAsset) {
        m_playableAsset = playableAsset;
    }

//----------------------------------------------------------------------------------------------------------------------
    public void Dispose() {
        if (m_disposed) return;

        m_disposed= true;
    }


//----------------------------------------------------------------------------------------------------------------------
    internal void Render(TimelineClip clip, double visibleLocalStartTime, double visibleLocalEndTime, Rect visibleRect) {

        IList<string> imagePaths = m_playableAsset.GetImagePaths();

        //Calculate the width if we are showing the whole clip
        //(visibleWidth / visibleDuration = fullWidth / fullDuration)
        double visibleDuration = visibleLocalEndTime - visibleLocalStartTime;
        double scaledClipDuration = clip.duration * clip.timeScale; 
        float fullWidth = Mathf.Ceil((float)(visibleRect.width * scaledClipDuration / visibleDuration));
        
        
        //Calculate rect for one image.
        float dimensionRatio = m_playableAsset.GetOrUpdateDimensionRatio();
        int widthPerPreviewImage = (int) (dimensionRatio * visibleRect.height);
        int heightPerPreviewImage = (int)visibleRect.height;

        //Set the number of preview images available for this clip, at least 1
        int numAllPreviewImages = Mathf.Max(Mathf.FloorToInt(fullWidth / widthPerPreviewImage),1);

        //Check the number of frames of this clip
        float fps = clip.parentTrack.timelineAsset.editorSettings.fps;
        int numFrames = Mathf.RoundToInt((float)(clip.duration * fps));
        numAllPreviewImages = Mathf.Min(numAllPreviewImages, numFrames);
        if (numAllPreviewImages <= 0)
            return;
        
        
        
        float xCounter = fullWidth / numAllPreviewImages;
        
        double localTimeCounter = scaledClipDuration / numAllPreviewImages;
        
        double localTime = clip.clipIn;
        int startFrame = Mathf.RoundToInt((float) clip.clipIn * fps);
        double clipInXOffset = ((startFrame / (float) clip.timeScale) * xCounter);

        //Find the place to draw the preview image[0], which might not be rendered. Consider clipIn too.
        float firstFrameXOffset = (float)(fullWidth * ((visibleLocalStartTime) / scaledClipDuration));              
        Rect drawRect = new Rect(visibleRect) {
            x      = (visibleRect.x - firstFrameXOffset) + (float) clipInXOffset, 
            y      = visibleRect.y,
            width  = widthPerPreviewImage,
            height = heightPerPreviewImage
        };
        // Debug.Log($"Full width: {fullWidth} numAllPreviewImages: {numAllPreviewImages}, " +
        //     $"StartFrame: {startFrame}, " +            
        //     $"drawRectX: {drawRect.x}, VisibleRectX: {visibleRect.x}, VisibleLocalStartTime: {visibleLocalStartTime}, "+
        //     $"firstFrameXOffset: {firstFrameXOffset}, clipInXOffset: {clipInXOffset}, " +
        //     $"FullWidth: {fullWidth}, widthPerPreviewImage: {widthPerPreviewImage}, DimensionRatio: {dimensionRatio}, "+ 
        //     $"xCounter: {xCounter}, ScaledClipDuration: {scaledClipDuration}, " +
        //     $"ClipStart: {clip.start}, ClipTimeScale: {clip.timeScale}, ClipIn: {clip.clipIn}");
        
        
        //Loop to render all preview Images, ignoring those outside the visible Rect
        float endVisibleRectX = visibleRect.x + visibleRect.width;
        float startVisibleRectX = visibleRect.x - widthPerPreviewImage; //for rendering preview images that are partly visible
        PreviewDrawInfo drawInfo = new PreviewDrawInfo() {
            DrawRect = new Rect() {
                y = visibleRect.y,
                width = widthPerPreviewImage,
                height = heightPerPreviewImage,
            }
        };
        for (int i = 0; i < numAllPreviewImages; ++i) {
            if (drawRect.x >= startVisibleRectX && drawRect.x <= endVisibleRectX) {
                
                drawInfo.DrawRect.x = drawRect.x;
                drawInfo.LocalTime = localTime;               
                
                DrawPreviewImage(drawInfo, clip);
                
            }
            //Check if x is inside the visible rect
            drawRect.x += xCounter;
            localTime  += localTimeCounter;
        }

    }

//----------------------------------------------------------------------------------------------------------------------    

    void DrawPreviewImage(PreviewDrawInfo drawInfo, TimelineClip clip) {
        int imageIndex = m_playableAsset.LocalTimeToImageIndex(clip, drawInfo.LocalTime);
        
        IList<string> imagePaths = m_playableAsset.GetImagePaths();

        //Load
        string fullPath = m_playableAsset.GetCompleteFilePath(imagePaths[imageIndex]);
        StreamingImageSequencePlugin.GetImageDataInto(fullPath, StreamingImageSequenceConstants.IMAGE_TYPE_PREVIEW
            ,Time.frameCount, out ImageData readResult);
        switch (readResult.ReadStatus) {
            case StreamingImageSequenceConstants.READ_STATUS_LOADING:
                break;
            case StreamingImageSequenceConstants.READ_STATUS_SUCCESS: {
                Texture2D tex = PreviewTextureFactory.GetOrCreate(fullPath, ref readResult);
                if (null != tex) {
                    Graphics.DrawTexture(drawInfo.DrawRect, tex);
                }
                break;
            }
            default: {
                PreviewImageLoadBGTask.Queue(fullPath, (int) drawInfo.DrawRect.width, (int) drawInfo.DrawRect.height, 
                    Time.frameCount);
                break;
            }

        }
        
    }

//----------------------------------------------------------------------------------------------------------------------
    private bool m_disposed;
    private readonly StreamingImageSequencePlayableAsset m_playableAsset = null;
    private const int MIN_PREVIEW_IMAGE_WIDTH = 60;
}

} //end namespace

