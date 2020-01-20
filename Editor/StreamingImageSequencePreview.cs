using System;
using System.Collections;
using System.Collections.Generic;

namespace UnityEngine.StreamingImageSequence {

internal class StreamingImageSequencePreview : IDisposable {

    public StreamingImageSequencePreview(StreamingImageSequencePlayableAsset playableAsset) {
        m_playableAsset = playableAsset;
        m_textures = new List<Texture2D>();
    }

//----------------------------------------------------------------------------------------------------------------------
    public void Dispose() {
        if (m_disposed) return;

        m_disposed= true;
        foreach (Texture2D tex in m_textures) {
            if (tex!= null) {
                if (Application.isPlaying)
                    UnityEngine.Object.Destroy(tex);
                else
                    UnityEngine.Object.DestroyImmediate(tex);
            }
        }
        m_textures.Clear();
    }

//----------------------------------------------------------------------------------------------------------------------
    public void Render(Rect rect) {

        IList<string> imagePaths = m_playableAsset.GetImagePaths();

        //TODO-sin: 2020-1-17 Set the size of textures according to the used size in the Timeline instead of full size
        ImageDimensionInt resolution = m_playableAsset.GetResolution();

        //Calculate rect for one image.
        float dimensionRatio = m_playableAsset.GetDimensionRatio();
        int widthPerPreviewImage = (int) (dimensionRatio * rect.height);
        int heightPerPreviewImage = (int)rect.height;

        //Prepare the textures
        int numPreviewImages = Mathf.FloorToInt(rect.width / widthPerPreviewImage);

        //[TODO-sin: 2020-1-17] The middle of the preview should show the middle frame, not the left
        int imageIndexCounter = imagePaths.Count / numPreviewImages;
        int imageIndex = 0;
        Rect drawRect = new Rect(rect) {
            width = widthPerPreviewImage,
            height = heightPerPreviewImage
        };

        for (int i = 0; i < numPreviewImages; ++i) {

            //[TODO-sin: 2020-1-17] imageIndex should be based on time, instead of index directly

            //Load
            string fullPath = m_playableAsset.GetCompleteFilePath(imagePaths[imageIndex]);
            StreamingImageSequencePlugin.GetNativTextureInfo(fullPath, out ReadResult readResult);

            //[TODO-sin: 2020-1-17] Queue a job if the read status is not success yet
            if (readResult.ReadStatus == StreamingImageSequenceConstants.READ_RESULT_SUCCESS) {

                //[TODO-sin: 2020-1-17] Store Texture and filename in the list and compare it.
                if (m_textures.Count <= i || null==m_textures[i]) {

                    Texture2D curTex = new Texture2D(readResult.Width, readResult.Height, 
                        StreamingImageSequenceConstants.TEXTURE_FORMAT, false, false
                    );

                    curTex.LoadRawTextureData(readResult.Buffer, readResult.Width * readResult.Height * 4);
                    curTex.filterMode = FilterMode.Bilinear;
                    curTex.Apply();

                    if (m_textures.Count <= i) {
                        m_textures.Add(curTex);
                    } else {
                        m_textures[i] = curTex;
                    }
                }

                Graphics.DrawTexture(drawRect, m_textures[i]);
            }
            drawRect.x += widthPerPreviewImage;
            imageIndex += imageIndexCounter;
        }

    }

//----------------------------------------------------------------------------------------------------------------------
    private bool m_disposed;
    List<Texture2D> m_textures;
    private readonly StreamingImageSequencePlayableAsset m_playableAsset = null;
    private const int MIN_PREVIEW_IMAGE_WIDTH = 60;
}

} //end namespace

