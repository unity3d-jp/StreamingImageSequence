using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.StreamingImageSequence;
using UnityEngine.Timeline;

namespace UnityEditor.StreamingImageSequence {

internal class StreamingImageSequencePreview {

    public StreamingImageSequencePreview(StreamingImageSequencePlayableAsset playableAsset) {
        m_playableAsset = playableAsset;
    }

//----------------------------------------------------------------------------------------------------------------------
    internal void SetVisibleLocalTime(double startTime, double endTime) {
        m_visibleLocalStartTime = startTime;
        m_visibleLocalEndTime   = endTime;
    }

//----------------------------------------------------------------------------------------------------------------------
    internal void Render(Rect visibleRect) {

        TimelineClip clip = m_playableAsset.GetTimelineClip();
        IList<string> imagePaths = m_playableAsset.GetImagePaths();

        //Calculate the width if we are showing the whole clip
        //(visibleWidth / visibleDuration = fullWidth / fullDuration)
        double visibleDuration = m_visibleLocalEndTime - m_visibleLocalStartTime;
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
        
        //Find the place to draw the preview image[0], which might not be rendered
        float xOffset = (float)(fullWidth * (m_visibleLocalStartTime / scaledClipDuration));
        Rect drawRect = new Rect(visibleRect) {
            x = visibleRect.x - xOffset,
            y = visibleRect.y,
            width = widthPerPreviewImage,
            height = heightPerPreviewImage
        };
        
        
        float xCounter = fullWidth / numAllPreviewImages;
        //Debug.Log($"Full width: {fullWidth} numAllPreviewImages: {numAllPreviewImages}, xCounter: {xCounter}");
        
        double localTimeCounter = scaledClipDuration / numAllPreviewImages;
        //Each preview should show the image used in the time in the middle of the span, instead of the left start point
        double localTime = (localTimeCounter * 0.5f);
        
        
        //Loop to render all preview Images, ignoring those outside the visible Rect
        float endVisibleRectX = visibleRect.x + visibleRect.width;
        float startVisibleRectX = visibleRect.x - widthPerPreviewImage; //for rendering preview images that are partly visible
        for (int i = 0; i < numAllPreviewImages; ++i) {
            if (drawRect.x >= startVisibleRectX && drawRect.x <= endVisibleRectX) {

                int imageIndex = m_playableAsset.LocalTimeToImageIndex(localTime);

                //Load
                string fullPath = m_playableAsset.GetCompleteFilePath(imagePaths[imageIndex]);
                StreamingImageSequencePlugin.GetNativeTextureInfo(fullPath, out ReadResult readResult, 
                    StreamingImageSequenceConstants.TEXTURE_TYPE_PREVIEW);
                switch (readResult.ReadStatus) {
                    case StreamingImageSequenceConstants.READ_RESULT_NONE: {
                        PreviewImageLoadBGTask.Queue(fullPath, widthPerPreviewImage, heightPerPreviewImage);
                        break;
                    }
                    case StreamingImageSequenceConstants.READ_RESULT_SUCCESS: {
                        Texture2D tex = PreviewTextureFactory.GetOrCreate(fullPath, ref readResult);
                        if (null != tex) {
                            Graphics.DrawTexture(drawRect, tex);
                        }
                        break;
                    }
                    default: {
                        break;
                    }

                }
                
            }
            //Check if x is inside the visible rect
            drawRect.x += xCounter;
            localTime  += localTimeCounter;
        }

    }

//----------------------------------------------------------------------------------------------------------------------
    private readonly StreamingImageSequencePlayableAsset m_playableAsset = null;
    private const int MIN_PREVIEW_IMAGE_WIDTH = 60;
    private double m_visibleLocalStartTime;
    private double m_visibleLocalEndTime;
}

} //end namespace

