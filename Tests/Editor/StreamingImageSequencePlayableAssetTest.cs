using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEditor.SceneManagement;
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
            PlayableDirector director = NewSceneWithDirector();
            StreamingImageSequencePlayableAsset sisAsset = CreateTestTimelineAssets(director);
            yield return null;

            IList<string> imagePaths = sisAsset.GetImagePaths();
            Assert.IsNotNull(imagePaths);
            Assert.IsTrue(imagePaths.Count > 0);
            Assert.IsTrue(sisAsset.HasImages());

            DestroyTestTimelineAssets(sisAsset);
            yield return null;
        }
        
//----------------------------------------------------------------------------------------------------------------------                
        [UnityTest]
        public IEnumerator ShowUseImageMarkers() {
            PlayableDirector director = NewSceneWithDirector();
            StreamingImageSequencePlayableAsset sisAsset = CreateTestTimelineAssets(director);
            yield return null;
            
            sisAsset.SetUseImageMarkerVisibility(true);
            yield return null;
            
            TimelineClip clip = sisAsset.GetTimelineClip();
            TrackAsset trackAsset = clip.parentTrack;
            Assert.AreEqual(sisAsset.CalculateIdealNumPlayableFrames(), trackAsset.GetMarkerCount());
            
            DestroyTestTimelineAssets(sisAsset);
            yield return null;
        }
        
//----------------------------------------------------------------------------------------------------------------------                
        [UnityTest]
        public IEnumerator ResizePlayableAsset() {
            PlayableDirector director = NewSceneWithDirector();
            StreamingImageSequencePlayableAsset sisAsset = CreateTestTimelineAssets(director);
            yield return null;
            
            sisAsset.SetUseImageMarkerVisibility(true);
            yield return null;
            
            TimelineClip clip = sisAsset.GetTimelineClip();
            TrackAsset trackAsset = clip.parentTrack;
            Assert.AreEqual(sisAsset.CalculateIdealNumPlayableFrames(), trackAsset.GetMarkerCount());

            //Resize
            ResizeTimelineClip(clip, 7.0f); yield return null;
            Assert.AreEqual(sisAsset.CalculateIdealNumPlayableFrames(), trackAsset.GetMarkerCount());
            ResizeTimelineClip(clip, 4.0f); yield return null;
            Assert.AreEqual(sisAsset.CalculateIdealNumPlayableFrames(), trackAsset.GetMarkerCount());
            ResizeTimelineClip(clip, 6.0f); yield return null;
            Assert.AreEqual(sisAsset.CalculateIdealNumPlayableFrames(), trackAsset.GetMarkerCount());
            
            DestroyTestTimelineAssets(sisAsset);
            yield return null;
        }

//----------------------------------------------------------------------------------------------------------------------                

        private void ResizeTimelineClip(TimelineClip clip, double duration) {
            clip.duration = duration;
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
        }

//----------------------------------------------------------------------------------------------------------------------                
        private PlayableDirector NewSceneWithDirector() {
            EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
            GameObject directorGo = new GameObject("Director");
            PlayableDirector director = directorGo.AddComponent<PlayableDirector>();
            return director;
        }
        
//----------------------------------------------------------------------------------------------------------------------                

        StreamingImageSequencePlayableAsset CreateTestTimelineAssets(PlayableDirector director) {
            string tempTimelineAssetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/TempTimelineForTestRunner.playable");
            string tempSisAssetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/TempSisForTestRunner.playable");


            //Create timeline asset
            TimelineAsset timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
            director.playableAsset = timelineAsset;
            AssetDatabase.CreateAsset(timelineAsset, tempTimelineAssetPath);
            
            //Create empty asset
            StreamingImageSequencePlayableAsset sisAsset = ScriptableObject.CreateInstance<StreamingImageSequencePlayableAsset>();
            AssetDatabase.CreateAsset(sisAsset, tempSisAssetPath);
            
            StreamingImageSequenceTrack movieTrack = timelineAsset.CreateTrack<StreamingImageSequenceTrack>(null, "Footage");
            TimelineClip clip = movieTrack.CreateDefaultClip();
            clip.asset = sisAsset;
            clip.CreateCurves("Curves: " + clip.displayName);
            sisAsset.SetTimelineClip(clip);
            sisAsset.ValidateAnimationCurve();

            //Select gameObject and open Timeline Window. This will trigger the TimelineWindow's update etc.
            EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
//            Selection.activeTransform = director.gameObject.transform;
//            TimelineEditor.selectedClip = sisAsset.GetTimelineClip();
            Selection.activeObject = director;


            const string PKG_PATH = "Packages/com.unity.streaming-image-sequence/Tests/Data/png/A_00000.png";
            string fullPath = Path.GetFullPath(PKG_PATH);
            ImageSequenceImporter.ImportPictureFiles(ImageFileImporterParam.Mode.StreamingAssets, fullPath, sisAsset,false);
            
            
            return sisAsset;
        }

//----------------------------------------------------------------------------------------------------------------------        
        void DestroyTestTimelineAssets(StreamingImageSequencePlayableAsset sisAsset) {
            TrackAsset movieTrack = sisAsset.GetTimelineClip().parentTrack;
            TimelineAsset timelineAsset = movieTrack.timelineAsset;
            
            string tempSisAssetPath = AssetDatabase.GetAssetPath(sisAsset);
            string tempTimelineAssetPath = AssetDatabase.GetAssetPath(timelineAsset);
            Assert.False(string.IsNullOrEmpty(tempSisAssetPath));
            Assert.False(string.IsNullOrEmpty(tempTimelineAssetPath));
            
            ObjectUtility.Destroy(movieTrack);
            ObjectUtility.Destroy(sisAsset);
            ObjectUtility.Destroy(timelineAsset);
            AssetDatabase.DeleteAsset(tempTimelineAssetPath);
            AssetDatabase.DeleteAsset(tempSisAssetPath);
            
        }
    }
}
