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

            IList<string> imageFileNames = sisAsset.GetImageFileNames();
            Assert.IsNotNull(imageFileNames);
            Assert.IsTrue(imageFileNames.Count > 0);
            Assert.IsTrue(sisAsset.HasImages());
            
            

            //Test that there should be no active PlayableAsset at the time above what exists in the track.
            TimelineClip clip = sisAsset.GetBoundTimelineClip();
            director.time = clip.start + clip.duration + 1;
            yield return null;
            
            Assert.IsNull(track.GetActivePlayableAsset()); 
            

            DestroyTestTimelineAssets(clip);
            yield return null;
        }
        
//----------------------------------------------------------------------------------------------------------------------                
        [UnityTest]
        public IEnumerator ShowUseImageMarkers() {
            PlayableDirector director = NewSceneWithDirector();
            StreamingImageSequencePlayableAsset sisAsset = CreateTestTimelineAssets(director);
            yield return null;
            
            //Show
            TimelineClip clip = sisAsset.GetBoundTimelineClip();
            TimelineClipSISData timelineClipSISData = sisAsset.GetBoundTimelineClipSISData();
            
            TrackAsset trackAsset = clip.parentTrack;
            timelineClipSISData.SetUseImageMarkerVisibility(true);
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
            yield return null;
            
            Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());
            yield return null;


            //Undo showing UseImageMarkers
            UndoAndRefreshTimelineEditor(); yield return null;
            Assert.False(timelineClipSISData.GetUseImageMarkerVisibility());
            Assert.AreEqual(0, trackAsset.GetMarkerCount());
            
            
            DestroyTestTimelineAssets(clip);
            yield return null;
        }
        
//----------------------------------------------------------------------------------------------------------------------                
        [UnityTest]
        public IEnumerator ResizePlayableAsset() {
            PlayableDirector director = NewSceneWithDirector();
            StreamingImageSequencePlayableAsset sisAsset = CreateTestTimelineAssets(director);
            TimelineClipSISData timelineClipSISData = sisAsset.GetBoundTimelineClipSISData();
            yield return null;
            
            timelineClipSISData.SetUseImageMarkerVisibility(true); 
            Undo.IncrementCurrentGroup(); //the base of undo is here. UseImageMarkerVisibility is still true after undo
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
            yield return null;

            //Original length
            TimelineClip clip = sisAsset.GetBoundTimelineClip();
            TrackAsset trackAsset = clip.parentTrack;
            Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());
            double origClipDuration = clip.duration;

            //Resize longer
            ResizeSISPlayableAsset(sisAsset, origClipDuration + 3.0f); yield return null;
            Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());

            //Undo
            UndoAndRefreshTimelineEditor(); yield return null;
            Assert.AreEqual(origClipDuration, clip.duration);
            Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());
            
            //Resize shorter
            ResizeSISPlayableAsset(sisAsset, Mathf.Max(0.1f, ( (float)(origClipDuration) - 3.0f))); yield return null;
            Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());
            
            //Undo
            UndoAndRefreshTimelineEditor(); yield return null;
            Assert.AreEqual(origClipDuration, clip.duration);
            Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());
            
            
            DestroyTestTimelineAssets(clip);
            yield return null;
        }

//----------------------------------------------------------------------------------------------------------------------                
        [UnityTest]
        public IEnumerator UncheckUseImageMarkers() {
            PlayableDirector director = NewSceneWithDirector();
            StreamingImageSequencePlayableAsset sisAsset = CreateTestTimelineAssets(director);
            TimelineClipSISData timelineClipSISData = sisAsset.GetBoundTimelineClipSISData();
            timelineClipSISData.SetUseImageMarkerVisibility(true);
            yield return null;

            TimelineClip clip = sisAsset.GetBoundTimelineClip();
            double timePerFrame = TimelineUtility.CalculateTimePerFrame(clip);
            int numImages = sisAsset.GetImageFileNames().Count;
            clip.timeScale = 3.75f; //use scaling
            ResizeSISPlayableAsset(sisAsset, (timePerFrame * numImages));
            yield return null;
            
            int numFrames = TimelineUtility.CalculateNumFrames(clip);
            Assert.AreEqual(numImages, numFrames);
            
            //Reset: make sure that the curve is a simple straight line from 0 to 1
            TimelineUtility.ResetTimelineClipCurve(clip);
            yield return null;
            
            sisAsset.ResetPlayableFrames();            
            yield return null;
            
            StreamingImageSequenceTrack track = sisAsset.GetBoundTimelineClip().parentTrack as StreamingImageSequenceTrack;
            Assert.IsNotNull(track);
            List<UseImageMarker> useImageMarkers = new List<UseImageMarker>();

            int i = 0;
            foreach (var m in track.GetMarkers()) {
                UseImageMarker marker = m as UseImageMarker;
                Assert.IsNotNull(marker);
                useImageMarkers.Add(marker);
                int imageIndex = sisAsset.GlobalTimeToImageIndex(clip, marker.time);
                Assert.AreEqual(i, imageIndex);
                ++i;
            }
            
            //Uncheck and see if the unchecked images became ignored
            useImageMarkers[4].SetImageUsed(false);
            useImageMarkers[5].SetImageUsed(false);
            Assert.AreEqual(3, sisAsset.GlobalTimeToImageIndex(clip, useImageMarkers[4].time));
            Assert.AreEqual(3, sisAsset.GlobalTimeToImageIndex(clip, useImageMarkers[5].time));
            

            useImageMarkers[7].SetImageUsed(false);
            useImageMarkers[8].SetImageUsed(false);
            Assert.AreEqual(6, sisAsset.GlobalTimeToImageIndex(clip, useImageMarkers[7].time));
            Assert.AreEqual(6, sisAsset.GlobalTimeToImageIndex(clip, useImageMarkers[8].time));
                       
            DestroyTestTimelineAssets(clip);
            yield return null;
        }

//----------------------------------------------------------------------------------------------------------------------                
        [UnityTest]
        public IEnumerator ResetUseImageMarkers() {
            PlayableDirector director = NewSceneWithDirector();
            StreamingImageSequencePlayableAsset sisAsset = CreateTestTimelineAssets(director);
            TimelineClipSISData timelineClipSISData = sisAsset.GetBoundTimelineClipSISData();
            timelineClipSISData.SetUseImageMarkerVisibility(true);
            yield return null;
            
            //Change image to false
            StreamingImageSequenceTrack track = sisAsset.GetBoundTimelineClip().parentTrack as StreamingImageSequenceTrack;
            Assert.IsNotNull(track);           
            foreach (var m in track.GetMarkers()) {
                UseImageMarker marker = m as UseImageMarker;
                Assert.IsNotNull(marker);
                marker.SetImageUsed(false);
                
                UnityEngine.Assertions.Assert.IsFalse(marker.IsImageUsed());
            }            
            yield return null;
            
            sisAsset.ResetPlayableFrames();            
            yield return null;
            
            //Check if all markers have been reset to used
            foreach (var m in track.GetMarkers()) {
                UseImageMarker marker = m as UseImageMarker;
                Assert.IsNotNull(marker);                
                UnityEngine.Assertions.Assert.IsTrue(marker.IsImageUsed());
            }
            yield return null;

                       
            TimelineClip clip = sisAsset.GetBoundTimelineClip();
            DestroyTestTimelineAssets(clip);
            yield return null;
        }
        
//----------------------------------------------------------------------------------------------------------------------                

        private void ResizeSISPlayableAsset(StreamingImageSequencePlayableAsset sisAsset, double duration) {            
            sisAsset.SetDuration(duration);
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
            TimelineClipSISData sisData = new TimelineClipSISData(clip);
            sisAsset.BindTimelineClip(clip, sisData);           

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
        private static void DestroyTestTimelineAssets(TimelineClip clip) {
            TrackAsset movieTrack = clip.parentTrack;
            TimelineAsset timelineAsset = movieTrack.timelineAsset;
            
            string tempTimelineAssetPath = AssetDatabase.GetAssetPath(timelineAsset);
            Assert.False(string.IsNullOrEmpty(tempTimelineAssetPath));

            timelineAsset.DeleteTrack(movieTrack);
            ObjectUtility.Destroy(timelineAsset);
            AssetDatabase.DeleteAsset(tempTimelineAssetPath);
            
        }
    }
}
