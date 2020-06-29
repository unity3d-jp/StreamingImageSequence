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
            
            //Test the track immediately
            StreamingImageSequenceTrack track = sisAsset.GetBoundTimelineClip().parentTrack as StreamingImageSequenceTrack;
            Assert.IsNotNull(track);
            Assert.IsNotNull(track.GetActivePlayableAsset());
            
            yield return null;

            IList<string> imagePaths = sisAsset.GetImagePaths();
            Assert.IsNotNull(imagePaths);
            Assert.IsTrue(imagePaths.Count > 0);
            Assert.IsTrue(sisAsset.HasImages());
            
            

            //Test that there should be no active PlayableAsset at the time above what exists in the track.
            TimelineClip clip = sisAsset.GetBoundTimelineClip();
            director.time = clip.start + clip.duration + 1;
            yield return null;
            
            Assert.IsNull(track.GetActivePlayableAsset()); 
            

            DestroyTestTimelineAssets(sisAsset);
            yield return null;
        }
        
//----------------------------------------------------------------------------------------------------------------------                
        [UnityTest]
        public IEnumerator ShowUseImageMarkers() {
            PlayableDirector director = NewSceneWithDirector();
            StreamingImageSequencePlayableAsset sisAsset = CreateTestTimelineAssets(director);
            yield return null;
            
            //Resize
            TimelineClip clip = sisAsset.GetBoundTimelineClip();
            TrackAsset trackAsset = clip.parentTrack;
            sisAsset.SetUseImageMarkerVisibility(true);
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
            yield return null;
            
            Assert.AreEqual(sisAsset.CalculateIdealNumPlayableFrames(), trackAsset.GetMarkerCount());
            yield return null;


            //Undo showing UseImageMarkers
            UndoAndRefreshTimelineEditor(); yield return null;
            Assert.False(sisAsset.GetUseImageMarkerVisibility());
            Assert.AreEqual(0, trackAsset.GetMarkerCount());
            
            
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
            Undo.IncrementCurrentGroup(); //the base of undo is here. UseImageMarkerVisibility is still true after undo
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
            yield return null;

            //Original length
            TimelineClip clip = sisAsset.GetBoundTimelineClip();
            TrackAsset trackAsset = clip.parentTrack;
            Assert.AreEqual(sisAsset.CalculateIdealNumPlayableFrames(), trackAsset.GetMarkerCount());
            double origClipDuration = clip.duration;

            //Resize longer
            ResizeTimelineClip(clip, origClipDuration + 3.0f); yield return null;
            Assert.AreEqual(sisAsset.CalculateIdealNumPlayableFrames(), trackAsset.GetMarkerCount());

            //Undo
            UndoAndRefreshTimelineEditor(); yield return null;
            Assert.AreEqual(origClipDuration, clip.duration);
            Assert.AreEqual(sisAsset.CalculateIdealNumPlayableFrames(), trackAsset.GetMarkerCount());
            
            //Resize shorter
            ResizeTimelineClip(clip, Mathf.Max(0.1f, ( (float)(origClipDuration) - 3.0f))); yield return null;
            Assert.AreEqual(sisAsset.CalculateIdealNumPlayableFrames(), trackAsset.GetMarkerCount());
            
            //Undo
            UndoAndRefreshTimelineEditor(); yield return null;
            Assert.AreEqual(origClipDuration, clip.duration);
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
        private void UndoAndRefreshTimelineEditor() {
            Undo.PerformUndo(); 
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

            //Create timeline asset
            TimelineAsset timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
            director.playableAsset = timelineAsset;
            AssetDatabase.CreateAsset(timelineAsset, tempTimelineAssetPath);
            
            //Create empty asset
            StreamingImageSequenceTrack movieTrack = timelineAsset.CreateTrack<StreamingImageSequenceTrack>(null, "Footage");
            TimelineClip clip = movieTrack.CreateDefaultClip();
            StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
            Assert.IsNotNull(sisAsset);

            clip.CreateCurves("Curves: " + clip.displayName);
            sisAsset.BindTimelineClip(clip);
            sisAsset.ValidateAnimationCurve();

            //Select gameObject and open Timeline Window. This will trigger the TimelineWindow's update etc.
            EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
//            Selection.activeTransform = director.gameObject.transform;
//            TimelineEditor.selectedClip = sisAsset.GetBoundTimelineClip();
            Selection.activeObject = director;


            const string PKG_PATH = "Packages/com.unity.streaming-image-sequence/Tests/Data/png/A_00000.png";
            string fullPath = Path.GetFullPath(PKG_PATH);
            ImageSequenceImporter.ImportPictureFiles(ImageFileImporterParam.Mode.StreamingAssets, fullPath, sisAsset,false);
            
            
            return sisAsset;
        }

//----------------------------------------------------------------------------------------------------------------------        
        void DestroyTestTimelineAssets(StreamingImageSequencePlayableAsset sisAsset) {
            TrackAsset movieTrack = sisAsset.GetBoundTimelineClip().parentTrack;
            TimelineAsset timelineAsset = movieTrack.timelineAsset;
            
            string tempTimelineAssetPath = AssetDatabase.GetAssetPath(timelineAsset);
            Assert.False(string.IsNullOrEmpty(tempTimelineAssetPath));

            timelineAsset.DeleteTrack(movieTrack);
            ObjectUtility.Destroy(timelineAsset);
            AssetDatabase.DeleteAsset(tempTimelineAssetPath);
            
        }
    }
}
