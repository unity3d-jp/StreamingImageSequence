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
            TimelineClip clip = CreateTestTimelineClip(director);
            StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
            Assert.IsNotNull(sisAsset);
            
            //Test the track immediately
            StreamingImageSequenceTrack track = clip.parentTrack as StreamingImageSequenceTrack;
            Assert.IsNotNull(track);
            Assert.IsNotNull(track.GetActivePlayableAsset());
            
            yield return null;

            IList<string> imageFileNames = sisAsset.GetImageFileNames();
            Assert.IsNotNull(imageFileNames);
            Assert.IsTrue(imageFileNames.Count > 0);
            Assert.IsTrue(sisAsset.HasImages());
            
            

            //Test that there should be no active PlayableAsset at the time above what exists in the track.
            director.time = clip.start + clip.duration + 1;
            yield return null;
            
            Assert.IsNull(track.GetActivePlayableAsset()); 
            

            DestroyTestTimelineAssets(clip);
            yield return null;
        }
        
//----------------------------------------------------------------------------------------------------------------------                
        [UnityTest]
        public IEnumerator ShowFrameMarkers() {
            PlayableDirector director = NewSceneWithDirector();
            TimelineClip                        clip     = CreateTestTimelineClip(director);
            StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
            Assert.IsNotNull(sisAsset);
            yield return null;
            
            //Show
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
            TimelineClip                        clip     = CreateTestTimelineClip(director);
            StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
            Assert.IsNotNull(sisAsset);
            TimelineClipSISData timelineClipSISData = sisAsset.GetBoundTimelineClipSISData();
            yield return null;
            
            timelineClipSISData.SetUseImageMarkerVisibility(true); 
            Undo.IncrementCurrentGroup(); //the base of undo is here. UseImageMarkerVisibility is still true after undo
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
            yield return null;

            //Original length
            TrackAsset trackAsset = clip.parentTrack;
            Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());
            double origClipDuration = clip.duration;

            //Resize longer
            ResizeSISTimelineClip(clip, origClipDuration + 3.0f); yield return null;
            Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());

            //Undo
            UndoAndRefreshTimelineEditor(); yield return null;
            Assert.AreEqual(origClipDuration, clip.duration);
            Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());
            
            //Resize shorter
            ResizeSISTimelineClip(clip, Mathf.Max(0.1f, ( (float)(origClipDuration) - 3.0f))); yield return null;
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
            TimelineClip                        clip     = CreateTestTimelineClip(director);
            StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
            Assert.IsNotNull(sisAsset);
            TimelineClipSISData timelineClipSISData = sisAsset.GetBoundTimelineClipSISData();
            timelineClipSISData.SetUseImageMarkerVisibility(true);
            yield return null;

            double timePerFrame = TimelineUtility.CalculateTimePerFrame(clip);
            int numImages = sisAsset.GetImageFileNames().Count;
            clip.timeScale = 3.75f; //use scaling
            ResizeSISTimelineClip(clip, (timePerFrame * numImages));
            yield return null;
            
            int numFrames = TimelineUtility.CalculateNumFrames(clip);
            Assert.AreEqual(numImages, numFrames);
            
            //Reset: make sure that the curve is a simple straight line from 0 to 1
            StreamingImageSequencePlayableAsset.ResetTimelineClipCurve(clip);
            yield return null;
            
            sisAsset.ResetPlayableFrames();            
            yield return null;
            
            StreamingImageSequenceTrack track = clip.parentTrack as StreamingImageSequenceTrack;
            Assert.IsNotNull(track);
            List<FrameMarker> useImageMarkers = new List<FrameMarker>();

            int i = 0;
            foreach (var m in track.GetMarkers()) {
                FrameMarker marker = m as FrameMarker;
                Assert.IsNotNull(marker);
                useImageMarkers.Add(marker);
                int imageIndex = sisAsset.GlobalTimeToImageIndex(clip, marker.time);
                Assert.AreEqual(i, imageIndex);
                ++i;
            }
            
            //Uncheck and see if the unchecked images became ignored
            useImageMarkers[4].SetFrameUsed(false);
            useImageMarkers[5].SetFrameUsed(false);
            Assert.AreEqual(3, sisAsset.GlobalTimeToImageIndex(clip, useImageMarkers[4].time));
            Assert.AreEqual(3, sisAsset.GlobalTimeToImageIndex(clip, useImageMarkers[5].time));
            

            useImageMarkers[7].SetFrameUsed(false);
            useImageMarkers[8].SetFrameUsed(false);
            Assert.AreEqual(6, sisAsset.GlobalTimeToImageIndex(clip, useImageMarkers[7].time));
            Assert.AreEqual(6, sisAsset.GlobalTimeToImageIndex(clip, useImageMarkers[8].time));
                       
            DestroyTestTimelineAssets(clip);
            yield return null;
        }

//----------------------------------------------------------------------------------------------------------------------                
        [UnityTest]
        public IEnumerator ResetUseImageMarkers() {
            PlayableDirector director = NewSceneWithDirector();
            TimelineClip                        clip     = CreateTestTimelineClip(director);
            StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
            Assert.IsNotNull(sisAsset);
            TimelineClipSISData timelineClipSISData = sisAsset.GetBoundTimelineClipSISData();
            timelineClipSISData.SetUseImageMarkerVisibility(true);
            yield return null;
            
            //Change image to false
            StreamingImageSequenceTrack track = clip.parentTrack as StreamingImageSequenceTrack;
            Assert.IsNotNull(track);           
            foreach (var m in track.GetMarkers()) {
                FrameMarker marker = m as FrameMarker;
                Assert.IsNotNull(marker);
                marker.SetFrameUsed(false);
                
                UnityEngine.Assertions.Assert.IsFalse(marker.IsFrameUsed());
            }            
            yield return null;
            
            sisAsset.ResetPlayableFrames();            
            yield return null;
            
            //Check if all markers have been reset to used
            foreach (var m in track.GetMarkers()) {
                FrameMarker marker = m as FrameMarker;
                Assert.IsNotNull(marker);                
                UnityEngine.Assertions.Assert.IsTrue(marker.IsFrameUsed());
            }
            yield return null;

                       
            DestroyTestTimelineAssets(clip);
            yield return null;
        }
        
//----------------------------------------------------------------------------------------------------------------------                

        private void ResizeSISTimelineClip(TimelineClip clip, double duration) {
            
#if UNITY_EDITOR            
            Undo.RegisterFullObjectHierarchyUndo(clip.parentTrack,"StreamingImageSequence: Set Duration");
#endif            
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

        TimelineClip CreateTestTimelineClip(PlayableDirector director) {
            string tempTimelineAssetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/TempTimelineForTestRunner.playable");

            //Create timeline asset
            TimelineAsset timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
            director.playableAsset = timelineAsset;
            AssetDatabase.CreateAsset(timelineAsset, tempTimelineAssetPath);
            
            //Create empty asset
            StreamingImageSequenceTrack sisTrack = timelineAsset.CreateTrack<StreamingImageSequenceTrack>(null, "Footage");
            TimelineClip clip = sisTrack.CreateDefaultClip();
            StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
            Assert.IsNotNull(sisAsset);

            clip.CreateCurves("Curves: " + clip.displayName);
            TimelineClipSISData sisData = new TimelineClipSISData(clip);
            sisAsset.InitTimelineClipCurve(clip);
            sisAsset.BindTimelineClipSISData(sisData);           

            //Select gameObject and open Timeline Window. This will trigger the TimelineWindow's update etc.
            EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
//            Selection.activeTransform = director.gameObject.transform;
//            TimelineEditor.selectedClip = sisAsset.GetBoundTimelineClip();
            Selection.activeObject = director;


            const string PKG_PATH = "Packages/com.unity.streaming-image-sequence/Tests/Data/png/A_00000.png";
            string fullPath = Path.GetFullPath(PKG_PATH);
            ImageSequenceImporter.ImportPictureFiles(ImageFileImporterParam.Mode.StreamingAssets, fullPath, sisAsset,false);
            
            
            return clip;
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
