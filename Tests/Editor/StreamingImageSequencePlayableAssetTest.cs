using System.Collections;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.StreamingImageSequence;
using UnityEngine.TestTools;
using UnityEngine.Timeline;

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
            StreamingImageSequencePlayableAssetParam param = new StreamingImageSequencePlayableAssetParam();
            StreamingImageSequencePlayableAsset sisAsset = ScriptableObject.CreateInstance<StreamingImageSequencePlayableAsset>();
            sisAsset.SetParam(param);

            StreamingImageSequenceTrack movieTrack = asset.CreateTrack<StreamingImageSequenceTrack>(null, "Footage");
            TimelineClip clip = movieTrack.CreateDefaultClip();
            clip.asset = sisAsset;
            clip.CreateCurves("Curves: " + clip.displayName);
            sisAsset.SetTimelineClip(clip);
            sisAsset.ValidateAnimationCurve();

            //Select gameObject and open Timeline Window. This will trigger the TimelineWindow's update etc.
            Selection.activeGameObject=directorGo;
            EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
            yield return null;

        }
    }
}
