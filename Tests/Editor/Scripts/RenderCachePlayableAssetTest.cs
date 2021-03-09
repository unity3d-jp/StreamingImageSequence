using System.Collections;
using System.IO;
using NUnit.Framework;
using Unity.EditorCoroutines.Editor;
using Unity.FilmInternalUtilities;
using UnityEngine;
using UnityEngine.Playables;
using Unity.StreamingImageSequence.Editor;
using UnityEngine.TestTools;
using UnityEngine.Timeline;

namespace Unity.StreamingImageSequence.EditorTests {

internal class RenderCachePlayableAssetTest {

    [UnityTest]
    public IEnumerator CreatePlayableAsset() {
        PlayableDirector director = EditorUtilityTest.NewSceneWithDirector();
        TimelineClip clip = EditorUtilityTest.CreateTestRenderCacheTimelineClip(director);
        RenderCachePlayableAsset renderCachePlayableAsset = clip.asset as RenderCachePlayableAsset;
        Assert.IsNotNull(renderCachePlayableAsset);
        
        //Test the track immediately
        RenderCacheTrack track = clip.GetParentTrack() as RenderCacheTrack;
        Assert.IsNotNull(track);        
        yield return null;

        EditorUtilityTest.DestroyTestTimelineAssets(clip);
        yield return null;
    }
    
    
//----------------------------------------------------------------------------------------------------------------------                
    [UnityTest]
    public IEnumerator UpdateRenderCachePNGInStreamingAssets() {
        PlayableDirector director   = EditorUtilityTest.NewSceneWithDirector();
        TimelineClip     clip       = EditorUtilityTest.CreateTestRenderCacheTimelineClip(director);
        TimelineAsset timelineAsset = director.playableAsset as TimelineAsset;        
        RenderCachePlayableAsset renderCachePlayableAsset = clip.asset as RenderCachePlayableAsset;
        RenderCacheTrack         track = clip.GetParentTrack() as RenderCacheTrack;
        Assert.IsNotNull(timelineAsset);
        Assert.IsNotNull(renderCachePlayableAsset);
        Assert.IsNotNull(track);        
        Assert.IsNotNull(Camera.main);        
        yield return null;

        clip.duration = (1.0f / timelineAsset.editorSettings.fps);
        const string OUTPUT_FOLDER = "Asset/StreamingAssets/RenderCachePNGForTestRunner";
        Directory.CreateDirectory(OUTPUT_FOLDER);
        renderCachePlayableAsset.SetFolder(OUTPUT_FOLDER);        
        
        GameObject cameraRenderCapturerGO = new GameObject();
        CameraRenderCapturer cameraRenderCapturer = cameraRenderCapturerGO.AddComponent<CameraRenderCapturer>();
        cameraRenderCapturer.SetCamera(Camera.main);
        director.SetGenericBinding(track, cameraRenderCapturer);       
        yield return null;
        
        //Update RenderCache              
        EditorCoroutineUtility.StartCoroutineOwnerless(
            RenderCachePlayableAssetInspector.UpdateRenderCacheCoroutine(director, renderCachePlayableAsset)
        );        
        
        //A hack to wait until the coroutine is finished
        const float TIMEOUT_SEC = 3.0f;
        Assert.IsTrue(Directory.Exists(OUTPUT_FOLDER));
        float  prevTime      = Time.realtimeSinceStartup;        
        while (Time.realtimeSinceStartup - prevTime < TIMEOUT_SEC) {
             yield return null;
        }       
        
        string imageFilePath = renderCachePlayableAsset.GetImageFilePath(0);
        Assert.IsTrue(File.Exists(imageFilePath));
        ImageLoader.RequestLoadFullImage(imageFilePath);

        //Another hack to wait until the load is finished
        prevTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - prevTime < TIMEOUT_SEC) {
            yield return null;
        }       
        
        ImageLoader.GetImageDataInto(imageFilePath,StreamingImageSequenceConstants.IMAGE_TYPE_FULL,out ImageData imageData);
        Assert.AreEqual(StreamingImageSequenceConstants.READ_STATUS_SUCCESS, imageData.ReadStatus);        
        yield return null;
        
        //cleanup
        StreamingImageSequencePlugin.UnloadAllImages();
        bool folderDeleted = FileUtility.DeleteFilesAndFolders(OUTPUT_FOLDER);
        Assert.IsTrue(folderDeleted);
        EditorUtilityTest.DestroyTestTimelineAssets(clip);
        yield return null;
        
    }

}

} //end namespace
