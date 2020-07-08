//#define DEBUG_PREVIEW_IMAGES         

using System;
using NUnit.Framework;
using UnityEngine;


namespace UnityEditor.StreamingImageSequence {

internal static class PreviewUtility {

    internal static void EnumeratePreviewImages( ref PreviewClipInfo clipInfo, Action<PreviewDrawInfo> drawPreviewFunc) 
    {
        double visibleLocalStartTime = clipInfo.VisibleLocalStartTime;
        double visibleLocalEndTime   = clipInfo.VisibleLocalEndTime;
        Rect   visibleRect           = clipInfo.VisibleRect;
        double visibleDuration       = visibleLocalEndTime - visibleLocalStartTime;
        double scaledFramePerSecond  = clipInfo.FramePerSecond / clipInfo.TimeScale; 
        double scaledClipDuration    = clipInfo.Duration * clipInfo.TimeScale; 
        
        //Calculate rect for one image.
        float dimensionRatio        = clipInfo.ImageDimensionRatio;
        int   widthPerPreviewImage  = (int) (dimensionRatio * visibleRect.height);
        int   heightPerPreviewImage = (int)visibleRect.height;
        
        //Calculate the time first visible frame         
        int    firstFrame = (int )Math.Floor( (float) (visibleLocalStartTime * scaledFramePerSecond));
        double firstFrameTime  = firstFrame / scaledFramePerSecond;        
       
        int numAllPreviewImages = 0;
        {            
            //Calculate the width if we are showing the whole clip
            //Eq: (visibleWidth / visibleDuration = fullClipWidth / fullDuration)
            float fullClipWidth = Mathf.Ceil((float)(visibleRect.width * scaledClipDuration / visibleDuration));

            //Calculate the number of preview images available for this clip, at least 1 (incl. the invisible ones)
            numAllPreviewImages = Mathf.Max(Mathf.FloorToInt(fullClipWidth / widthPerPreviewImage),1);
        
            //All frames for the clip (including the invisible ones)
            int numAllFrames = Mathf.RoundToInt((float)(clipInfo.Duration * scaledFramePerSecond));
            numAllPreviewImages = Mathf.Min(numAllPreviewImages, numAllFrames);
        }        
        
        if (numAllPreviewImages <= 0)
            return;
        
        double localTimeCounter = scaledClipDuration / numAllPreviewImages;        

        //Base the firstFrameTime on localTimeCounter, which was calculated using full clip length and fullWidth,
        //so that they transition smoothly when we slide the slider in Timeline window
        firstFrameTime = Mathf.Floor((float)firstFrameTime / (float )localTimeCounter) * localTimeCounter;        
        double firstFrameRectX = FindFrameXPos(firstFrameTime, visibleLocalStartTime, visibleDuration, visibleRect.x, visibleRect.width);
        

        //Loop to render all preview Images, ignoring those outside the visible Rect
        float startVisibleRectX = visibleRect.x - widthPerPreviewImage; //for rendering preview images that are partly visible
        PreviewDrawInfo drawInfo = new PreviewDrawInfo() {
            DrawRect = new Rect() {
                x = (float) firstFrameRectX,
                y = visibleRect.y,
                width = widthPerPreviewImage,
                height = heightPerPreviewImage,
            },
            LocalTime = firstFrameTime,
        };

        //minor optimization by executing FindFrameXPos() less
        double secondFrameRectX = (float) FindFrameXPos(drawInfo.LocalTime + localTimeCounter, visibleLocalStartTime, visibleDuration, visibleRect.x, visibleRect.width);
        float xCounter = (float)(secondFrameRectX - firstFrameRectX);

        Assert.Greater(xCounter, 0);
        
        float endVisibleRectX = (visibleRect.x + visibleRect.width) - (xCounter * 0.5f);
        while (drawInfo.DrawRect.x < (endVisibleRectX)) {
                 
            //drawInfo.DrawRect.x = (float) FindFrameXPos(drawInfo.LocalTime, visibleLocalStartTime, visibleDuration, visibleRect.x, visibleRect.width);
            
            if (drawInfo.DrawRect.x >= startVisibleRectX) {
                drawPreviewFunc(drawInfo);                                
            }

            drawInfo.DrawRect.x += xCounter;
            drawInfo.LocalTime += localTimeCounter;
        }

#if DEBUG_PREVIEW_IMAGES        
        Debug.Log($"Width: {visibleRect.width} numAllPreviewImages: {numPreviewImagesToDraw}, " +
            $"firstFrameRectX: {firstFrameRectX}, firstFrameTime: {firstFrameTime}, " +
            $"VisibleRect: {visibleRect}, xCounter: {xCounter}, " +
            $"VisibleLocalStartTime: {visibleLocalStartTime}, VisibleLocalEndTime: {visibleLocalEndTime}, "+
            $"widthPerPreviewImage: {widthPerPreviewImage}, DimensionRatio: {dimensionRatio}, "+ 
            $"ClipTimeScale: {clipInfo.TimeScale}, ClipIn: {clipInfo.ClipIn}");
        
#endif
    }


//----------------------------------------------------------------------------------------------------------------------    
    static double FindFrameXPos(double frameTime, double visibleLocalStartTime, double visibleDuration, 
        double visibleRectX, double visibleRectWidth) 
    {
        //Use triangle proportion
        double ret = (frameTime - visibleLocalStartTime) / visibleDuration * visibleRectWidth + visibleRectX;
        return ret;
    }
}

} //end namespace

