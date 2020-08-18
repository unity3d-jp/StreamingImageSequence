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
            PlayableDirector director = EditorUtilityTest.NewSceneWithDirector();
            TimelineClip clip = EditorUtilityTest.CreateTestTimelineClip(director);
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
            

            EditorUtilityTest.DestroyTestTimelineAssets(clip);
            yield return null;
        }
        
        
//----------------------------------------------------------------------------------------------------------------------                
        [UnityTest]
        public IEnumerator ResizePlayableAsset() {
            PlayableDirector director = EditorUtilityTest.NewSceneWithDirector();
            TimelineClip                        clip     = EditorUtilityTest.CreateTestTimelineClip(director);
            StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
            Assert.IsNotNull(sisAsset);
            TimelineClipSISData timelineClipSISData = sisAsset.GetBoundTimelineClipSISData();
            yield return null;
            
            timelineClipSISData.ShowFrameMarkers(true); 
            Undo.IncrementCurrentGroup(); //the base of undo is here. FrameMarkerVisibility is still true after undo
            TimelineEditor.Refresh(RefreshReason.ContentsModified);
            yield return null;

            //Original length
            TrackAsset trackAsset = clip.parentTrack;
            Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());
            double origClipDuration = clip.duration;

            //Resize longer
            EditorUtilityTest.ResizeSISTimelineClip(clip, origClipDuration + 3.0f); yield return null;
            Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());

            //Undo
            EditorUtilityTest.UndoAndRefreshTimelineEditor(); yield return null;
            Assert.AreEqual(origClipDuration, clip.duration);
            Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());
            
            //Resize shorter
            EditorUtilityTest.ResizeSISTimelineClip(clip, Mathf.Max(0.1f, ( (float)(origClipDuration) - 3.0f))); yield return null;
            Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());
            
            //Undo
            EditorUtilityTest.UndoAndRefreshTimelineEditor(); yield return null;
            Assert.AreEqual(origClipDuration, clip.duration);
            Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());
            
            
            EditorUtilityTest.DestroyTestTimelineAssets(clip);
            yield return null;
        }
//----------------------------------------------------------------------------------------------------------------------                

        [UnityTest]
        public IEnumerator ReloadPlayableAsset() {
            PlayableDirector                    director = EditorUtilityTest.NewSceneWithDirector();
            TimelineClip                        clip     = EditorUtilityTest.CreateTestTimelineClip(director);
            StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
            Assert.IsNotNull(sisAsset);
            
            string folder = sisAsset.GetFolder();
            Assert.IsNotNull(folder);
            int numOriginalImages = sisAsset.GetImageFileNames().Count;
            Assert.Greater(numOriginalImages,0);

            
            List<string> testImages = StreamingImageSequencePlayableAsset.FindImages(folder);
            List<string> copiedImagePaths = new List<string>(testImages.Count);
            foreach (string imageFileName in testImages) {
                string src = Path.Combine(folder, imageFileName);
                string dest = Path.Combine(folder, "Copied_" + imageFileName);
                File.Copy(src,dest);
                copiedImagePaths.Add(dest);
            }

            yield return null;
            sisAsset.Reload();
            
            yield return null;
            Assert.AreEqual(numOriginalImages * 2 , sisAsset.GetImageFileNames().Count);

            //Cleanup
            foreach (string imagePath in copiedImagePaths) {
                File.Delete(imagePath);
            }
                       

            EditorUtilityTest.DestroyTestTimelineAssets(clip);
            yield return null;
            
        }
        
    }
}
