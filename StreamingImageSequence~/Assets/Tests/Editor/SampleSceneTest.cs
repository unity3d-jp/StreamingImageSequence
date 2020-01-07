using NUnit.Framework;
using UnityEditor;
using UnityEngine.StreamingImageSequence;


namespace Tests {
    public class SampleSceneTest {
        [Test]
        public void PlayableAssetValidityPass() {           
            string[] playableAssetPaths = {
                "Assets/AeConvert/Footage_A_StreamingImageSequence.playable",
                "Assets/AeConvert/Footage_B_StreamingImageSequence.playable",
            };

            foreach (string path in playableAssetPaths) {
                StreamingImageSequencePlayableAsset playableAsset = AssetDatabase.LoadAssetAtPath<StreamingImageSequencePlayableAsset>(path);
                Assert.IsNotNull(playableAsset);
                Assert.IsTrue(playableAsset.GetFolder().StartsWith("Assets/"));
            }
        }

    }
}
