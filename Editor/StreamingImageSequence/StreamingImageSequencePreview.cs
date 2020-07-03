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
    internal static void EnumeratePreviewImages( ref PreviewClipInfo clipInfo, Action<PreviewDrawInfo> drawPreviewFunc) 
    {

        double visibleLocalStartTime = clipInfo.VisibleLocalStartTime;
        double visibleLocalEndTime = clipInfo.VisibleLocalEndTime;
        Rect   visibleRect = clipInfo.VisibleRect;
        
        
        //Calculate the width if we are showing the whole clip
        //(visibleWidth / visibleDuration = fullWidth / fullDuration)
        double visibleDuration = visibleLocalEndTime - visibleLocalStartTime;
        double scaledClipDuration = clipInfo.Duration * clipInfo.TimeScale; 
        float fullWidth = Mathf.Ceil((float)(visibleRect.width * scaledClipDuration / visibleDuration));
        
        
        //Calculate rect for one image.
        float dimensionRatio = clipInfo.ImageDimensionRatio;
        int widthPerPreviewImage = (int) (dimensionRatio * visibleRect.height);
        int heightPerPreviewImage = (int)visibleRect.height;

        //Set the number of preview images available for this clip, at least 1
        int numAllPreviewImages = Mathf.Max(Mathf.FloorToInt(fullWidth / widthPerPreviewImage),1);

        //Check the number of frames of this clip
        // float fps = clip.parentTrack.timelineAsset.editorSettings.fps;
        int numFrames = Mathf.RoundToInt((float)(clipInfo.Duration * clipInfo.FramePerSecond));
        numAllPreviewImages = Mathf.Min(numAllPreviewImages, numFrames);
        if (numAllPreviewImages <= 0)
            return;
        
        
        
        float xCounter = fullWidth / numAllPreviewImages;
        
        double localTimeCounter = scaledClipDuration / numAllPreviewImages;
        
        double localTime = clipInfo.ClipIn;
        int startFrame = Mathf.RoundToInt((float) clipInfo.ClipIn * clipInfo.FramePerSecond);
        double clipInXOffset = ((startFrame / (float) clipInfo.TimeScale) * xCounter);

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
                
                drawPreviewFunc(drawInfo);                
                
            }
            //Check if x is inside the visible rect
            drawRect.x += xCounter;
            localTime  += localTimeCounter;
        }

    }


//----------------------------------------------------------------------------------------------------------------------
    private bool m_disposed;
    private readonly StreamingImageSequencePlayableAsset m_playableAsset = null;
    private const int MIN_PREVIEW_IMAGE_WIDTH = 60;
}

} //end namespace

