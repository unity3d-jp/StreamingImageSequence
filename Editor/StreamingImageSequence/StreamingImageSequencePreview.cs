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
    internal void SetVisibleLocalTime(double startTime, double endTime) {
        m_visibleLocalStartTime = startTime;
        m_visibleLocalEndTime   = endTime;
    }

//----------------------------------------------------------------------------------------------------------------------
    internal void Render(TimelineClip clip, Rect visibleRect) {

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
        
        
        Rect drawRect = new Rect(visibleRect) {
            x = visibleRect.x,
            y = visibleRect.y,
            width = widthPerPreviewImage,
            height = heightPerPreviewImage
        };
        
        
        float xCounter = fullWidth / numAllPreviewImages;
        //Debug.Log($"Full width: {fullWidth} numAllPreviewImages: {numAllPreviewImages}, xCounter: {xCounter}");
        
        double localTimeCounter = scaledClipDuration / numAllPreviewImages;
        
        double localTime = clip.clipIn;
        
        //Loop to render all preview Images
        for (int i = 0; i < numAllPreviewImages; ++i) {

            int imageIndex = m_playableAsset.LocalTimeToImageIndex(clip, localTime);

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
                        Graphics.DrawTexture(drawRect, tex);
                    }
                    break;
                }
                default: {
                    PreviewImageLoadBGTask.Queue(fullPath, widthPerPreviewImage, heightPerPreviewImage, 
                        Time.frameCount);
                    break;
                }

                
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
    private double m_visibleLocalStartTime;
    private double m_visibleLocalEndTime;
}

} //end namespace

