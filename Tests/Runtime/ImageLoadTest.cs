using NUnit.Framework;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.StreamingImageSequence;
using UnityEngine.TestTools;

namespace UnityEditor.StreamingImageSequence.Tests {
    public class ImageLoadTest {
        [UnityTest]
        public IEnumerator QueueImageLoadTask() {
            string fullPath = "Packages/com.unity.streaming-image-sequence/Tests/Data/png/A_00000.png";
            Assert.IsTrue(File.Exists(fullPath));

            StreamingImageSequencePlugin.GetNativeTextureInfo(fullPath, out ReadResult readResult, 
                StreamingImageSequenceConstants.TEXTURE_TYPE_FULL);
            Assert.AreEqual(readResult.ReadStatus, 0);

            ImageLoadBGTask.Queue(fullPath, StreamingImageSequenceConstants.TEXTURE_TYPE_FULL);
            yield return new WaitForSeconds(1.0f);

            StreamingImageSequencePlugin.ResetNativeTexture(fullPath);
            Assert.AreEqual(readResult.ReadStatus, 0);
        }


    }

} //end namespace
