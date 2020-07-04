//#define DEBUG_PREVIEW_IMAGES         

using System;
using UnityEngine;


namespace UnityEditor.StreamingImageSequence {

internal static class PreviewUtility {

    internal static void EnumeratePreviewImages( ref PreviewClipInfo clipInfo, Action<PreviewDrawInfo> drawPreviewFunc) 
    {

        double visibleLocalStartTime = clipInfo.VisibleLocalStartTime;
        double visibleLocalEndTime   = clipInfo.VisibleLocalEndTime;
        Rect   visibleRect           = clipInfo.VisibleRect;
        float  visibleRectEnd        = visibleRect.x + visibleRect.width;       
        double visibleDuration       = visibleLocalEndTime - visibleLocalStartTime;
        double scaledFramePerSecond  = clipInfo.FramePerSecond / clipInfo.TimeScale; 
        
        //Calculate rect for one image.
        float dimensionRatio        = clipInfo.ImageDimensionRatio;
        int   widthPerPreviewImage  = (int) (dimensionRatio * visibleRect.height);
        int   heightPerPreviewImage = (int)visibleRect.height;
        
        //Calculate the time and pos of the first frame to be drawn        
        int    firstFrame = (int )Math.Floor( (float) (visibleLocalStartTime * scaledFramePerSecond));
        double firstFrameTime  = firstFrame / scaledFramePerSecond;        
        double firstFrameRectX = FindFrameXPos(firstFrameTime, visibleLocalStartTime, visibleDuration, visibleRect.x, visibleRect.width);

        //Set the number of preview images based on visibleRect, at least 1
        int numPreviewImagesToDraw = Mathf.Max(Mathf.FloorToInt((visibleRectEnd - (float)firstFrameRectX) / widthPerPreviewImage),1);

        
        int lastFrame = (int )Math.Ceiling( (float) (visibleLocalEndTime * scaledFramePerSecond));
        double lastFrameTime  = lastFrame / scaledFramePerSecond;        

        //Check the number of preview images based on the number of actual frames in Timeline 
        int numFrames = lastFrame - firstFrame;
        numPreviewImagesToDraw = Mathf.Min(numPreviewImagesToDraw, numFrames);
        if (numPreviewImagesToDraw <= 0)
            return;
        
        
        double localTimeCounter = ((lastFrameTime  - firstFrameTime) / numPreviewImagesToDraw);
                
        //Loop to render all preview Images, ignoring those outside the visible Rect
        float endVisibleRectX = visibleRect.x + visibleRect.width;
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

        
        for (int i = 0; i < numPreviewImagesToDraw; ++i) {
                 
            drawInfo.DrawRect.x = (float) FindFrameXPos(drawInfo.LocalTime, visibleLocalStartTime, visibleDuration, visibleRect.x, visibleRect.width);
            
            //already exceeds the visible area            
            if (drawInfo.DrawRect.x >= (endVisibleRectX)) {
                break;
            }
            
            if (drawInfo.DrawRect.x >= startVisibleRectX) {
                drawPreviewFunc(drawInfo);                
                
            }
            drawInfo.LocalTime += localTimeCounter;
        }

#if DEBUG_PREVIEW_IMAGES        
        Debug.Log($"Width: {visibleRect.width} numAllPreviewImages: {numPreviewImagesToDraw}, " +
            $"firstFrameRectX: {firstFrameRectX}, firstFrameTime: {firstFrameTime}, " +
            $"VisibleRect: {visibleRect}, " +
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

