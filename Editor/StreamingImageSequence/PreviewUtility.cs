//#define DEBUG_PREVIEW_IMAGES         

using System;
using UnityEngine;


namespace UnityEditor.StreamingImageSequence {

internal static class PreviewUtility {

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

#if DEBUG_PREVIEW_IMAGES        
        string firstRectDebug = null;
#endif
        
        for (int i = 0; i < numAllPreviewImages; ++i) {

            //already exceeds the visible area
            if (drawRect.x > endVisibleRectX) {
                return;
            }
            
            if (drawRect.x >= startVisibleRectX) {
                
                drawInfo.DrawRect.x = drawRect.x;
                drawInfo.LocalTime = localTime;
                
#if DEBUG_PREVIEW_IMAGES        
                 if (null == firstRectDebug) {
                     firstRectDebug = ($"DrawRectX: {drawRect.x}, LocalTime: {localTime} ");
                 }
#endif
                                
                drawPreviewFunc(drawInfo);                
                
            }
            //Check if x is inside the visible rect
            drawRect.x += xCounter;
            localTime  += localTimeCounter;
        }

#if DEBUG_PREVIEW_IMAGES        
        Debug.Log($"Full width: {fullWidth} numAllPreviewImages: {numAllPreviewImages}, " +
            $"StartFrame: {startFrame}, " + $"drawRectX: {drawRect.x}, " +
            $"VisibleRect: {visibleRect}, " +
            $"VisibleLocalStartTime: {visibleLocalStartTime}, VisibleLocalEndTime: {visibleLocalEndTime}, "+
            $"firstFrameXOffset: {firstFrameXOffset}, clipInXOffset: {clipInXOffset}, " +
            $"FullWidth: {fullWidth}, widthPerPreviewImage: {widthPerPreviewImage}, DimensionRatio: {dimensionRatio}, "+ 
            $"xCounter: {xCounter}, ScaledClipDuration: {scaledClipDuration}, " +
            $"ClipTimeScale: {clipInfo.TimeScale}, ClipIn: {clipInfo.ClipIn}, {firstRectDebug}");
        
#endif
    }


}

} //end namespace

