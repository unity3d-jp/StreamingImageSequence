using NUnit.Framework;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.TestTools;
using C = Unity.StreamingImageSequence.StreamingImageSequenceConstants;

namespace Unity.StreamingImageSequence.Tests {
    public class ImageLoadTest {


        [UnityTest]
        public IEnumerator QueueFullImageLoadTask() {
            EditorUpdateManager.ResetImageLoading();
            const string PKG_PATH = "Packages/com.unity.streaming-image-sequence/Tests/Data/png/A_00000.png";
            string fullPath = Path.GetFullPath(PKG_PATH);
            Assert.IsTrue(File.Exists(fullPath));

            const int IMAGE_TYPE = C.IMAGE_TYPE_FULL;
            AssertReadStatus(fullPath, IMAGE_TYPE,  C.READ_STATUS_UNAVAILABLE, "Texture is already available ?");

            ImageLoader.RequestLoadFullImage(fullPath);                                
            yield return new WaitForSeconds(LOAD_TIMEOUT);

            AssertReadStatus(fullPath, IMAGE_TYPE,  C.READ_STATUS_SUCCESS, "Loading texture is not successful.");
            
            
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
            const int IMAGE_TYPE = C.IMAGE_TYPE_PREVIEW;
            AssertReadStatus(fullPath, IMAGE_TYPE,  C.READ_STATUS_UNAVAILABLE, "Texture is already available ?");

            const int WIDTH = 256;
            const int HEIGHT= 128;
            ImageLoader.RequestLoadPreviewImage(fullPath, WIDTH, HEIGHT);
            yield return new WaitForSeconds(LOAD_TIMEOUT);

            ImageData r = AssertReadStatus(fullPath, IMAGE_TYPE,  C.READ_STATUS_SUCCESS, "Loading texture is not successful.");
            
            Assert.AreEqual(WIDTH, r.Width );
            Assert.AreEqual(HEIGHT, r.Height);

            ResetAndAssert(fullPath, IMAGE_TYPE);
        }

//----------------------------------------------------------------------------------------------------------------------
        [UnityTest]
        public IEnumerator LoadUnavailableImages() {
            
            EditorUpdateManager.ResetImageLoading();
            const string PKG_PATH = "ThisImageDoesNotExist.png";
            string       fullPath = Path.GetFullPath(PKG_PATH);
            Assert.IsFalse(File.Exists(fullPath));

            ImageLoader.RequestLoadFullImage(fullPath);                                
            yield return new WaitForSeconds(LOAD_TIMEOUT);
            ImageLoader.RequestLoadPreviewImage(fullPath, /*width= */256 , /* height= */ 128);
            yield return new WaitForSeconds(LOAD_TIMEOUT);

            AssertReadStatus(fullPath, C.IMAGE_TYPE_FULL,    C.READ_STATUS_FAIL, "Unavailable texture was loaded.");
            AssertReadStatus(fullPath, C.IMAGE_TYPE_PREVIEW, C.READ_STATUS_UNAVAILABLE, 
                "The preview was loaded even though the full version failed.");

            ResetAndAssert(fullPath, C.IMAGE_TYPE_PREVIEW);
            ResetAndAssert(fullPath, C.IMAGE_TYPE_FULL);
        }
        
//----------------------------------------------------------------------------------------------------------------------

        //Check that this image is not loaded yet, except for a particular texture type
        void AssertUnloaded(string fullPath, int exceptionTexType) {
            //Check that we are not affecting the other image types
            for (int texType = 0; texType < C.MAX_IMAGE_TYPES;++texType) {
                if (texType == exceptionTexType)
                    continue;
                ImageLoader.GetImageDataInto(fullPath, texType, out ImageData otherReadResult);
                Assert.AreEqual(C.READ_STATUS_UNAVAILABLE, otherReadResult.ReadStatus, 
                    "AssertUnloaded()"
                );
            }
        }

//----------------------------------------------------------------------------------------------------------------------

        void ResetAndAssert(string fullPath, int texType) {
            StreamingImageSequencePlugin.UnloadImageAndNotify(fullPath);
            ImageLoader.GetImageDataInto(fullPath, texType, out ImageData readResult);
            Assert.AreEqual(C.READ_STATUS_UNAVAILABLE, readResult.ReadStatus, "ResetAndAssert");
            Assert.AreEqual(0, StreamingImageSequencePlugin.GetUsedImagesMemory());
        }

        ImageData AssertReadStatus(string fullPath, int imageType, int requiredStatus, string assertMessage) {
            ImageLoader.GetImageDataInto(fullPath,imageType, out ImageData readResult );
            Assert.AreEqual(requiredStatus, readResult.ReadStatus, assertMessage);
            return readResult;

        }

//----------------------------------------------------------------------------------------------------------------------

        private const float LOAD_TIMEOUT = 3.0f; //in seconds
    }

} //end namespace
