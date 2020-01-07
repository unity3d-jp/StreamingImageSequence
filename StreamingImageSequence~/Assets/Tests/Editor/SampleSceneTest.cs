using NUnit.Framework;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.StreamingImageSequence;
using UnityEngine.Timeline;

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

//---------------------------------------------------------------------------------------------------------------------
        [Test]
        public void SampleSceneValidityPass() {           
            string sampleScenePath = "Assets/Scenes/defaultSample.unity";
            Assert.IsTrue(File.Exists(sampleScenePath));
            EditorSceneManager.OpenScene(sampleScenePath);

            PlayableDirector[] pds = Object.FindObjectsOfType<PlayableDirector>(); 
            foreach (PlayableDirector pd in pds) {
                PlayableAsset playableAsset = pd.playableAsset;

                if (!(playableAsset is TimelineAsset)) {
                    continue;
                }

                TimelineAsset timelineAsset = playableAsset as TimelineAsset;
                foreach (TrackAsset trackAsset in timelineAsset.GetOutputTracks()) {
                    //Make sure the Image is bound to the trackAsset
                    Assert.IsNotNull(pd.GetGenericBinding(trackAsset));
                }
            }
        }
    }
}
