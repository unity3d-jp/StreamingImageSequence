using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.FilmInternalUtilities;
using Unity.FilmInternalUtilities.Editor;
using UnityEngine.Playables;
using UnityEditor;
using UnityEngine.TestTools;
using UnityEngine.Timeline;
using UnityEditor.Timeline;

namespace Unity.StreamingImageSequence.EditorTests {

internal class FrameMarkerTest {
    
    [UnityTest]
    public IEnumerator ShowFrameMarkers() {
        PlayableDirector director = EditorUtilityTest.NewSceneWithDirector();
        TimelineClip                        clip     = EditorUtilityTest.CreateTestSISTimelineClip(director);
        StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
        Assert.IsNotNull(sisAsset);
        yield return null;
        
        //Show
        SISClipData clipData = sisAsset.GetBoundClipData();
        
        TrackAsset trackAsset = clip.GetParentTrack();
        clipData.RequestFrameMarkers(true, true);
        TimelineEditor.Refresh(RefreshReason.ContentsModified);
        yield return null;
        
        Assert.AreEqual(TimelineUtility.CalculateNumFrames(clip), trackAsset.GetMarkerCount());
        yield return null;


        //Undo showing FrameMarkers
        EditorUtilityTest.UndoAndRefreshTimelineEditor(); yield return null;
        Assert.False(clipData.AreFrameMarkersRequested());
        Assert.AreEqual(0, trackAsset.GetMarkerCount());
        
        
        EditorUtilityTest.DestroyTestTimelineAssets(clip);
        yield return null;
    }
    
    
//----------------------------------------------------------------------------------------------------------------------                
    [UnityTest]
    public IEnumerator UncheckFrameMarkers() {
        PlayableDirector director = EditorUtilityTest.NewSceneWithDirector();
        TimelineClip                        clip     = EditorUtilityTest.CreateTestSISTimelineClip(director);
        StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
        Assert.IsNotNull(sisAsset);
        SISClipData clipData = sisAsset.GetBoundClipData();
        Assert.IsNotNull(clipData);
        clipData.RequestFrameMarkers(true, true);
        yield return null;

        double timePerFrame = TimelineUtility.CalculateTimePerFrame(clip);
        int numImages = sisAsset.GetNumImages();
        clip.timeScale = 3.75f; //use scaling
        EditorUtilityTest.ResizeSISTimelineClip(clip, (timePerFrame * numImages));
        yield return null;
        
        int numFrames = TimelineUtility.CalculateNumFrames(clip);
        Assert.AreEqual(numImages, numFrames);
        
        //Reset: make sure that the curve is a simple straight line from 0 to 1        
        EditorCurveBinding curveBinding = StreamingImageSequencePlayableAsset.GetTimeCurveBinding();                 
        ExtendedClipEditorUtility.ResetClipDataCurve(sisAsset, curveBinding);
        yield return null;
        
        sisAsset.ResetPlayableFrames();            
        yield return null;
        
        StreamingImageSequenceTrack track = clip.GetParentTrack() as StreamingImageSequenceTrack;
        Assert.IsNotNull(track);
        List<FrameMarker> frameMarkers = new List<FrameMarker>();

        int i = 0;
        foreach (var m in track.GetMarkers()) {
            FrameMarker marker = m as FrameMarker;
            Assert.IsNotNull(marker);
            frameMarkers.Add(marker);
            int imageIndex = sisAsset.GlobalTimeToImageIndex(clip, marker.time);
            Assert.AreEqual(i, imageIndex);
            ++i;
        }
        Assert.AreEqual(numImages,i);
        
        //Uncheck and see if the unchecked images became ignored
        frameMarkers[4].SetFrameUsed(false);
        frameMarkers[5].SetFrameUsed(false);
        Assert.AreEqual(3, sisAsset.GlobalTimeToImageIndex(clip, frameMarkers[4].time));
        Assert.AreEqual(3, sisAsset.GlobalTimeToImageIndex(clip, frameMarkers[5].time));
        

        frameMarkers[7].SetFrameUsed(false);
        frameMarkers[8].SetFrameUsed(false);
        Assert.AreEqual(6, sisAsset.GlobalTimeToImageIndex(clip, frameMarkers[7].time));
        Assert.AreEqual(6, sisAsset.GlobalTimeToImageIndex(clip, frameMarkers[8].time));
                   
        EditorUtilityTest.DestroyTestTimelineAssets(clip);
        yield return null;
    }

//----------------------------------------------------------------------------------------------------------------------                
    [UnityTest]
    public IEnumerator ResetFrameMarkers() {
        PlayableDirector director = EditorUtilityTest.NewSceneWithDirector();
        TimelineClip                        clip     = EditorUtilityTest.CreateTestSISTimelineClip(director);
        StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
        Assert.IsNotNull(sisAsset);
        SISClipData clipData = sisAsset.GetBoundClipData();
        Assert.IsNotNull(clipData);
        clipData.RequestFrameMarkers(true);
        yield return null;
        
        //Change image to false
        StreamingImageSequenceTrack track = clip.GetParentTrack() as StreamingImageSequenceTrack;
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

                   
        EditorUtilityTest.DestroyTestTimelineAssets(clip);
        yield return null;
    }
    
    
}

} //end namespace
