using NUnit.Framework;
using System.Collections;
using System.IO;
using UnityEngine.TestTools;

namespace UnityEngine.StreamingImageSequence.Tests {
    public class ImageLoadTest {


        [UnityTest]
        public IEnumerator QueueFullImageLoadTask() {
            EditorUpdateManager.ResetImageLoading();
            const string PKG_PATH = "Packages/com.unity.streaming-image-sequence/Tests/Data/png/A_00000.png";
            string fullPath = Path.GetFullPath(PKG_PATH);
            Assert.IsTrue(File.Exists(fullPath));

            const int IMAGE_TYPE = StreamingImageSequenceConstants.IMAGE_TYPE_FULL;

            ImageLoader.GetImageDataInto(fullPath, IMAGE_TYPE, out ImageData readResult );
            Assert.AreEqual(StreamingImageSequenceConstants.READ_STATUS_UNAVAILABLE, readResult.ReadStatus, 
                "Texture is already or currently being loaded"
            );

            ImageLoader.RequestLoadFullImage(fullPath);                                
            yield return new WaitForSeconds(LOAD_TIMEOUT);
            
            ImageLoader.GetImageDataInto(fullPath, IMAGE_TYPE, out readResult );
            Assert.AreEqual(StreamingImageSequenceConstants.READ_STATUS_SUCCESS, readResult.ReadStatus,
                "Loading texture is not successful."
            );
            
            AssertUnloaded(fullPath, IMAGE_TYPE);
            ResetAndAssert(fullPath, IMAGE_TYPE);
        }

//----------------------------------------------------------------------------------------------------------------------
        [UnityTest]
        public IEnumerator QueuePreviewImageLoadTask() {
            
            EditorUpdateManager.ResetImageLoading();
            const string PKG_PATH = "Packages/com.unity.streaming-image-sequence/Tests/Data/png/A_00000.png";
            string fullPath = Path.GetFullPath(PKG_PATH);
            Assert.IsTrue(File.Exists(fullPath));

            //Loading preview type would also load the full type
            const int IMAGE_TYPE = StreamingImageSequenceConstants.IMAGE_TYPE_PREVIEW;

            ImageLoader.GetImageDataInto(fullPath, IMAGE_TYPE, out ImageData readResult );
            Assert.AreEqual(StreamingImageSequenceConstants.READ_STATUS_UNAVAILABLE, readResult.ReadStatus, 
                "Texture is already or currently being loaded"
            );

            const int WIDTH = 256;
            const int HEIGHT= 128;
            ImageLoader.RequestLoadPreviewImage(fullPath, WIDTH, HEIGHT);
            yield return new WaitForSeconds(LOAD_TIMEOUT);

            ImageLoader.GetImageDataInto(fullPath,IMAGE_TYPE, out readResult );
            Assert.AreEqual(StreamingImageSequenceConstants.READ_STATUS_SUCCESS, readResult.ReadStatus, 
                "Loading texture is not successful."
            );
            Assert.AreEqual(WIDTH, readResult.Width );
            Assert.AreEqual(HEIGHT, readResult.Height);

            ResetAndAssert(fullPath, IMAGE_TYPE);
        }

//----------------------------------------------------------------------------------------------------------------------

        //Check that this image is not loaded yet, except for a particular texture type
        void AssertUnloaded(string fullPath, int exceptionTexType) {
            //Check that we are not affecting the other image types
            for (int texType = 0; texType < StreamingImageSequenceConstants.MAX_IMAGE_TYPES;++texType) {
                if (texType == exceptionTexType)
                    continue;
                ImageLoader.GetImageDataInto(fullPath, texType, out ImageData otherReadResult);
                Assert.AreEqual(StreamingImageSequenceConstants.READ_STATUS_UNAVAILABLE, otherReadResult.ReadStatus, 
                    "AssertUnloaded()"
                );
            }
        }

//----------------------------------------------------------------------------------------------------------------------

        void ResetAndAssert(string fullPath, int texType) {
            StreamingImageSequencePlugin.UnloadImageAndNotify(fullPath);
            ImageLoader.GetImageDataInto(fullPath, texType, out ImageData readResult);
            Assert.AreEqual(StreamingImageSequenceConstants.READ_STATUS_UNAVAILABLE, readResult.ReadStatus, "ResetAndAssert");
            Assert.AreEqual(0, StreamingImageSequencePlugin.GetUsedImagesMemory());
        }

//----------------------------------------------------------------------------------------------------------------------

        private const float LOAD_TIMEOUT = 3.0f; //in seconds
    }

} //end namespace
