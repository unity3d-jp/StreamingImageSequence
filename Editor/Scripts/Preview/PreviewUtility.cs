//#define DEBUG_PREVIEW_IMAGES         

using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Assertions;


namespace Unity.StreamingImageSequence.Editor {

internal static class PreviewUtility {

    internal static void EnumeratePreviewImages( ref PreviewClipInfo clipInfo, Action<PreviewDrawInfo> drawPreviewFunc) 
    {
        double visibleLocalStartTime = clipInfo.VisibleLocalStartTime;
        double visibleLocalEndTime   = clipInfo.VisibleLocalEndTime;
        Rect   visibleRect           = clipInfo.VisibleRect;

        //[Note-sin: 2020-12-16] Only support fixed height atm. Dynamic heights will make it more a lot more complex to:
        //- calculate the position/width/height of each image and if they should be shrunk in one dimension
        //- allocate memory for preview images
        const int FIXED_HEIGHT = 25;
        visibleRect.y      = (visibleRect.height-FIXED_HEIGHT) + 1;
        visibleRect.height = FIXED_HEIGHT;
        
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
            int numAllFrames = Mathf.RoundToInt((float)(clipInfo.Duration * clipInfo.FramePerSecond));
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

        //Shouldn't happen, but xCounter can be minus when moving the horizontal slider ?
        if (xCounter <= 0)
            return;
        
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
    
//----------------------------------------------------------------------------------------------------------------------
    
    internal static void DrawPreviewImage(ref PreviewDrawInfo drawInfo, string imagePath) {
#if UNITY_EDITOR_OSX
        //[TODO-sin: 2022-4-4] Disabling. There is a bug in Mac Silicon which causes crash when resizing images
        if (IsUsingOSX_Silicon())
            return;
#endif        
        if (!File.Exists(imagePath))
            return;

        Texture2D tex = null;
        if (imagePath.IsRegularAssetPath()) {
            tex = AssetDatabase.LoadAssetAtPath<Texture2D>(imagePath);
        } else {
            ImageLoader.GetImageDataInto(imagePath, StreamingImageSequenceConstants.IMAGE_TYPE_PREVIEW
                , out ImageData readResult);
            
            switch (readResult.ReadStatus) {
                case StreamingImageSequenceConstants.READ_STATUS_LOADING:
                    break;
                case StreamingImageSequenceConstants.READ_STATUS_SUCCESS: {
                    tex = PreviewTextureFactory.GetOrCreate(imagePath, ref readResult);                
                    break;
                }
                default: {
                    ImageLoader.RequestLoadPreviewImage(imagePath, (int) drawInfo.DrawRect.width, (int) drawInfo.DrawRect.height);                    
                    break;
                }

            }            
        }

        
        if (null == tex)
            return;

        if (PlayerSettings.colorSpace == ColorSpace.Linear) {
            Material mat = GetOrCreateLinearToGammaMaterial();
            Graphics.DrawTexture(drawInfo.DrawRect, tex, mat);
        } else {                    
            Graphics.DrawTexture(drawInfo.DrawRect, tex);
        }
        
    }

#if UNITY_EDITOR_OSX
    static bool IsUsingOSX_Silicon() {
#if UNITY_2021_2_OR_NEWER
        return SystemInfo.processorType.StartsWith("Apple M");
#else
        return false;
#endif
    }    
#endif
//----------------------------------------------------------------------------------------------------------------------
    static Material GetOrCreateLinearToGammaMaterial() {
        if (null != m_linearToGammaMaterial)
            return m_linearToGammaMaterial;
        
        Shader shader = AssetDatabase.LoadAssetAtPath<Shader>(StreamingImageSequenceConstants.LINEAR_TO_GAMMA_SHADER_PATH);
        m_linearToGammaMaterial = new Material(shader); 
        m_linearToGammaMaterial.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
        return m_linearToGammaMaterial;
    }

//----------------------------------------------------------------------------------------------------------------------
    
    private static Material m_linearToGammaMaterial = null;
}

} //end namespace

