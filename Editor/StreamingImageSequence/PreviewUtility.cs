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
        
        double visibleDuration    = visibleLocalEndTime - visibleLocalStartTime;
        double scaledClipDuration = clipInfo.Duration * clipInfo.TimeScale;
        
        //Calculate the time and pos of the first frame to be drawn
        int    firstFrame = (int )Math.Floor( (float) (visibleLocalStartTime * clipInfo.FramePerSecond));
        //firstFrame = Mathf.Max(firstFrame - 1, 0); //always start from one frame less
        double firstFrameTime  = firstFrame / clipInfo.FramePerSecond;        
        double firstFrameRectX = FindFrameXPos(firstFrameTime, visibleLocalStartTime, visibleDuration, visibleRect.x, visibleRect.width);
        
        
        //Calculate rect for one image.
        float dimensionRatio = clipInfo.ImageDimensionRatio;
        int widthPerPreviewImage = (int) (dimensionRatio * visibleRect.height);
        int heightPerPreviewImage = (int)visibleRect.height;

        //Set the number of preview images available for this clip, at least 1
        int numPreviewImagesToDraw = Mathf.Max(Mathf.FloorToInt((visibleRectEnd - (float)firstFrameRectX) / widthPerPreviewImage),1);

        

        //Calculate xCounter to increment the rectX when drawing the next frame
        double secondFrameTime  = (firstFrame + 1) / clipInfo.FramePerSecond;
        double secondFrameRectX = FindFrameXPos(secondFrameTime, visibleLocalStartTime, visibleDuration, visibleRect.x, visibleRect.width);              
        float  xCounter = (float)(secondFrameRectX - firstFrameRectX);

        
        int lastFrame = (int )Math.Ceiling( (float) ((visibleLocalEndTime) * clipInfo.FramePerSecond));
        //firstFrame = Mathf.Max(firstFrame - 1, 0); //always start from one frame less
        double lastFrameTime  = lastFrame / clipInfo.FramePerSecond;        
        double lastFrameRectX = FindFrameXPos(firstFrameTime, visibleLocalStartTime, visibleDuration, visibleRect.x, visibleRect.width);

        //Check the number of frames of this clip
        int numFrames = lastFrame - firstFrame;
        numPreviewImagesToDraw = Mathf.Min(numPreviewImagesToDraw, numFrames);
        if (numPreviewImagesToDraw <= 0)
            return;
        
        
//        double localTimeCounter = (secondFrameTime  - firstFrameTime);
        double localTimeCounter = (lastFrameTime  - firstFrameTime) / numPreviewImagesToDraw;
        
        double localTime = firstFrameTime;
        int startFrame = Mathf.RoundToInt((float) clipInfo.ClipIn * clipInfo.FramePerSecond);
        double clipInXOffset = ((startFrame / (float) clipInfo.TimeScale) * xCounter);

        //Find the place to draw the preview image[0], which might not be rendered. Consider clipIn too.
        //float firstFrameXOffset = (float)(fullWidth * ((visibleLocalStartTime-clipInfo.ClipIn) / scaledClipDuration));              
      
        
        //Loop to render all preview Images, ignoring those outside the visible Rect
        float endVisibleRectX = visibleRect.x + visibleRect.width;
        float startVisibleRectX = visibleRect.x - widthPerPreviewImage; //for rendering preview images that are partly visible
        PreviewDrawInfo drawInfo = new PreviewDrawInfo() {
            DrawRect = new Rect() {
                x = (float) firstFrameRectX,
                y = visibleRect.y,
                width = widthPerPreviewImage,
                height = heightPerPreviewImage,
            }
        };

#if DEBUG_PREVIEW_IMAGES        
        string firstRectDebug = null;
#endif
        
        for (int i = 0; i < numPreviewImagesToDraw; ++i) {

                 
            drawInfo.DrawRect.x = (float) FindFrameXPos(localTime, visibleLocalStartTime, visibleDuration, visibleRect.x, visibleRect.width);
            
            //already exceeds the visible area            
            if (drawInfo.DrawRect.x >= (endVisibleRectX)) {
                break;
            }
            
            if (drawInfo.DrawRect.x >= startVisibleRectX) {

                drawInfo.LocalTime = localTime;
                
#if DEBUG_PREVIEW_IMAGES        
                 if (null == firstRectDebug) {
                     firstRectDebug = ($"DrawRectX: {drawInfo.DrawRect.x}, LocalTime: {localTime} ");
                 }
#endif
                                
                drawPreviewFunc(drawInfo);                
                
            }
            //Check if x is inside the visible rect
            //drawInfo.DrawRect.x += xCounter;
            localTime  += localTimeCounter;
        }

#if DEBUG_PREVIEW_IMAGES        
        Debug.Log($"Width: {visibleRect.width} numAllPreviewImages: {numPreviewImagesToDraw}, " +
            $"StartFrame: {startFrame}, " + $"firstFrameRectX: {firstFrameRectX}, " +
            $"VisibleRect: {visibleRect}, " +
            $"VisibleLocalStartTime: {visibleLocalStartTime}, VisibleLocalEndTime: {visibleLocalEndTime}, "+
            $"clipInXOffset: {clipInXOffset}, " +
            $"widthPerPreviewImage: {widthPerPreviewImage}, DimensionRatio: {dimensionRatio}, "+ 
            $"xCounter: {xCounter}, ScaledClipDuration: {scaledClipDuration}, " +
            $"ClipTimeScale: {clipInfo.TimeScale}, ClipIn: {clipInfo.ClipIn}, {firstRectDebug}");
        
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

