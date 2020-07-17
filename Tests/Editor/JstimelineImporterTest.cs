using NUnit.Framework;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.StreamingImageSequence;
using UnityEngine.Timeline;

namespace UnityEditor.StreamingImageSequence.Tests {
    public class JstimelineImporterTest {
    [Test]
    public void Import() {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);

        string fullPath = "Packages/com.unity.streaming-image-sequence/Tests/Data/AeConvert.jstimeline";
        Assert.IsTrue(File.Exists(fullPath));

        string destFolder = "TestRunner";
        JstimelineImporter.ImportTimeline(fullPath, destFolder);

        //Check if the generated director is valid
        PlayableDirector[] directors = Object.FindObjectsOfType<PlayableDirector>();
        Assert.AreEqual(1, directors.Length);

        PlayableDirector pd = directors[0];
        TimelineAsset timelineAsset = pd.playableAsset as TimelineAsset;
        Assert.IsNotNull(timelineAsset);

        Assert.AreEqual(timelineAsset.outputTrackCount,1);
        foreach (TrackAsset trackAsset in timelineAsset.GetOutputTracks()) {
            StreamingImageSequenceTrack imageSequenceTrack = trackAsset as StreamingImageSequenceTrack;
            Assert.IsNotNull(imageSequenceTrack);

            foreach (TimelineClip clip in imageSequenceTrack.GetClips()) {
                Assert.IsNotNull(clip.asset);
                StreamingImageSequencePlayableAsset playableAsset = clip.asset as StreamingImageSequencePlayableAsset;
                Assert.IsNotNull(playableAsset);
                
                Assert.AreEqual(10, playableAsset.GetImageFileNames().Count);
            }

            //Make sure a StreamingImageSequenceRenderer is bound to the trackAsset
            StreamingImageSequenceRenderer r = pd.GetGenericBinding(trackAsset) as StreamingImageSequenceRenderer;
            Assert.IsNotNull(r);
        }
        
        //Delete created assets
        string destAssetsFolder = "Assets/" + destFolder;
        string[] createdAssets = AssetDatabase.FindAssets("", new[] { destAssetsFolder});
        Assert.Greater(createdAssets.Length,0);
        foreach (string guid in createdAssets) {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            AssetDatabase.DeleteAsset(assetPath);
        }        
        Directory.Delete(destAssetsFolder);
        

    }

}

} //end namespace
