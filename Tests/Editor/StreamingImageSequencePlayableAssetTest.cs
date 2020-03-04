using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.StreamingImageSequence;
using UnityEngine.TestTools;
using UnityEngine.Timeline;
using UnityEditor.Timeline;

namespace UnityEditor.StreamingImageSequence.Tests {

    internal class StreamingImageSequencePlayableAssetTest {

        [UnityTest]
        public IEnumerator CreatePlayableAsset() {
            GameObject directorGo = new GameObject("Director");
            PlayableDirector director = directorGo.AddComponent<PlayableDirector>();

            //Create timeline asset
            TimelineAsset asset = ScriptableObject.CreateInstance<TimelineAsset>();
            director.playableAsset = asset;

            //Create empty asset
            StreamingImageSequencePlayableAsset sisAsset = ScriptableObject.CreateInstance<StreamingImageSequencePlayableAsset>();

            StreamingImageSequenceTrack movieTrack = asset.CreateTrack<StreamingImageSequenceTrack>(null, "Footage");
            TimelineClip clip = movieTrack.CreateDefaultClip();
            clip.asset = sisAsset;
            clip.CreateCurves("Curves: " + clip.displayName);
            sisAsset.SetTimelineClip(clip);
            sisAsset.ValidateAnimationCurve();

            //Select gameObject and open Timeline Window. This will trigger the TimelineWindow's update etc.
            EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
            Selection.activeGameObject=directorGo;
            yield return null;

            TimelineEditor.selectedClip = clip;
            yield return null;

            const string PKG_PATH = "Packages/com.unity.streaming-image-sequence/Tests/Data/png/A_00000.png";
            string fullPath = Path.GetFullPath(PKG_PATH);
            ImageSequenceImporter.ImportPictureFiles(ImageFileImporterParam.Mode.StreamingAssets, fullPath, sisAsset,false);

            IList<string> imagePaths = sisAsset.GetImagePaths();
            Assert.IsNotNull(imagePaths);
            Assert.IsTrue(imagePaths.Count > 0);
            Assert.IsTrue(sisAsset.HasImages());

        }
    }
}
