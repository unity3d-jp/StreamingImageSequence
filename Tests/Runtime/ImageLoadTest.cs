using NUnit.Framework;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.StreamingImageSequence;
using UnityEngine.TestTools;

namespace UnityEditor.StreamingImageSequence.Tests {
    public class ImageLoadTest {


        [UnityTest]
        public IEnumerator QueueFullImageLoadTask() {
            string fullPath = "Packages/com.unity.streaming-image-sequence/Tests/Data/png/A_00000.png";
            Assert.IsTrue(File.Exists(fullPath));

            const int TEX_TYPE = StreamingImageSequenceConstants.TEXTURE_TYPE_FULL;

            StreamingImageSequencePlugin.GetNativeTextureInfo(fullPath, out ReadResult readResult, TEX_TYPE);
            Assert.AreEqual(readResult.ReadStatus, StreamingImageSequenceConstants.READ_RESULT_NONE);

            ImageLoadBGTask.Queue(fullPath);
            yield return new WaitForSeconds(0.1f);

            StreamingImageSequencePlugin.GetNativeTextureInfo(fullPath, out readResult, TEX_TYPE);
            Assert.AreEqual(readResult.ReadStatus, StreamingImageSequenceConstants.READ_RESULT_SUCCESS);

            AssertUnloaded(fullPath, TEX_TYPE);
            ResetAndAssert(fullPath, TEX_TYPE);
        }

//----------------------------------------------------------------------------------------------------------------------
        [UnityTest]
        public IEnumerator QueuePreviewImageLoadTask() {
            string fullPath = "Packages/com.unity.streaming-image-sequence/Tests/Data/png/A_00000.png";
            Assert.IsTrue(File.Exists(fullPath));

            const int TEX_TYPE = StreamingImageSequenceConstants.TEXTURE_TYPE_PREVIEW;

            StreamingImageSequencePlugin.GetNativeTextureInfo(fullPath, out ReadResult readResult, TEX_TYPE);
            Assert.AreEqual(readResult.ReadStatus, StreamingImageSequenceConstants.READ_RESULT_NONE);

            const int WIDTH = 256;
            const int HEIGHT= 128;
            PreviewImageLoadBGTask.Queue(fullPath, WIDTH, HEIGHT);
            yield return new WaitForSeconds(0.1f);

            StreamingImageSequencePlugin.GetNativeTextureInfo(fullPath, out readResult, TEX_TYPE);
            Assert.AreEqual(readResult.ReadStatus, StreamingImageSequenceConstants.READ_RESULT_SUCCESS);
            Assert.AreEqual(readResult.Width, WIDTH);
            Assert.AreEqual(readResult.Height, HEIGHT);

            AssertUnloaded(fullPath, TEX_TYPE);
            ResetAndAssert(fullPath, TEX_TYPE);
        }

//----------------------------------------------------------------------------------------------------------------------

        //Check that this image is not loaded yet, except for a particular texture type
        void AssertUnloaded(string fullPath, int exceptionTexType) {
            //Check that we are not affecting the other image types
            for (int texType = 0; texType < StreamingImageSequenceConstants.MAX_TEXTURE_TYPES;++texType) {
                if (texType == exceptionTexType)
                    continue;
                StreamingImageSequencePlugin.GetNativeTextureInfo(fullPath, out ReadResult otherReadResult, texType);
                Assert.AreEqual(otherReadResult.ReadStatus, StreamingImageSequenceConstants.READ_RESULT_NONE);
            }
        }

//----------------------------------------------------------------------------------------------------------------------

        void ResetAndAssert(string fullPath, int texType) {
            StreamingImageSequencePlugin.ResetNativeTexture(fullPath);
            StreamingImageSequencePlugin.GetNativeTextureInfo(fullPath, out ReadResult readResult, texType);
            Assert.AreEqual(readResult.ReadStatus, StreamingImageSequenceConstants.READ_RESULT_NONE);
        }

    }

} //end namespace
