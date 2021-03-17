using System.Collections;
using NUnit.Framework;
using System.IO;
using Unity.FilmInternalUtilities;
using Unity.FilmInternalUtilities.Editor;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using Unity.StreamingImageSequence.Editor;
using UnityEditor;
using UnityEngine.TestTools;
using UnityEngine.Timeline;

namespace Unity.StreamingImageSequence.EditorTests {

internal class EditorUtilityTest {

    [UnityTest]
    public IEnumerator DeleteAssetsInsideDataPath() {
        
        string uniqueName = Path.GetFileName(FileUtil.GetUniqueTempPathInProject());
        Assert.IsFalse(string.IsNullOrEmpty(uniqueName));
        string destFolder = Path.Combine(Application.dataPath, uniqueName);

        yield return CopyAndDeleteSampleAsset(destFolder);

    }

//----------------------------------------------------------------------------------------------------------------------    
    
    [UnityTest]
    public IEnumerator DeleteAssetsOutsideDataPath() {

        string destFolder = FileUtil.GetUniqueTempPathInProject();
        yield return CopyAndDeleteSampleAsset(destFolder);

    }

//----------------------------------------------------------------------------------------------------------------------    
    
    [Test]
    public void GetMainViewGameSize() {

        ViewEditorUtility.GetMainGameViewSize();        

    }
//----------------------------------------------------------------------------------------------------------------------    
    private IEnumerator CopyAndDeleteSampleAsset(string destFolder) {
        
        //
        Assert.IsTrue(File.Exists(SRC_IMAGE_PATH));
        string uniqueName = Path.GetFileName(FileUtil.GetUniqueTempPathInProject());
        Assert.IsFalse(string.IsNullOrEmpty(uniqueName));


        Directory.CreateDirectory(destFolder);
        int numDuplicates = 10;
        int numDigits     = MathUtility.GetNumDigits(numDuplicates);
        for (int i = 0; i < numDuplicates; ++i) {
            string destFileName = i.ToString($"D{numDigits}") + ".png";
            string destPath     = Path.Combine(destFolder, destFileName);
            File.Copy(SRC_IMAGE_PATH, destPath);
            
            Assert.IsTrue(File.Exists(destPath));            
            
        }
        yield return null;
        AssetEditorUtility.DeleteAssetsOrFiles(destFolder, "*.png");
        
        yield return null;
        string[] files = Directory.GetFiles(destFolder);
        Assert.IsTrue(0 == files.Length);
        
        
        Directory.Delete(destFolder);

    }
    
//----------------------------------------------------------------------------------------------------------------------                

    internal static void ResizeSISTimelineClip(TimelineClip clip, double duration) {
            
#if UNITY_EDITOR            
        Undo.RegisterCompleteObjectUndo(clip.GetParentTrack(),"StreamingImageSequence: Set Duration");
#endif            
        clip.duration = duration;
            
        TimelineEditor.Refresh(RefreshReason.ContentsModified);
    }
    
//----------------------------------------------------------------------------------------------------------------------                
    internal static void UndoAndRefreshTimelineEditor() {
        Undo.PerformUndo(); 
        TimelineEditor.Refresh(RefreshReason.ContentsModified);
    }
        
//----------------------------------------------------------------------------------------------------------------------                
    internal static PlayableDirector NewSceneWithDirector() {
        EditorSceneManager.NewScene(NewSceneSetup.DefaultGameObjects);
        GameObject       directorGo = new GameObject("Director");
        PlayableDirector director   = directorGo.AddComponent<PlayableDirector>();
        return director;
    }
    

//----------------------------------------------------------------------------------------------------------------------                
    internal static TimelineClip CreateTestSISTimelineClip(PlayableDirector director) {
        string tempTimelineAssetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/TempSISTimelineForTestRunner.playable");

        //Create timeline asset
        TimelineAsset timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
        director.playableAsset = timelineAsset;
        AssetDatabase.CreateAsset(timelineAsset, tempTimelineAssetPath);
            
        //Create empty asset
        StreamingImageSequenceTrack sisTrack = timelineAsset.CreateTrack<StreamingImageSequenceTrack>(null, "SIS Track");
        TimelineClip clip = sisTrack.CreateDefaultClip();
        StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
        Assert.IsNotNull(sisAsset);

        clip.CreateCurves("Curves: " + clip.displayName);
        SISClipData sisData = new SISClipData(clip);
        sisAsset.BindClipData(sisData);           
        ExtendedClipEditorUtility.ResetClipDataCurve(sisAsset, StreamingImageSequencePlayableAsset.GetTimeCurveBinding());        

        //Select gameObject and open Timeline Window. This will trigger the TimelineWindow's update etc.
        EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
//            Selection.activeTransform = director.gameObject.transform;
//            TimelineEditor.selectedClip = sisAsset.GetBoundTimelineClip();
        Selection.activeObject = director;


        string fullPath = Path.GetFullPath(SRC_IMAGE_PATH);
        ImageSequenceImporter.ImportImages(fullPath, sisAsset,false);
            
            
        return clip;
    }

    internal static TimelineClip CreateTestRenderCacheTimelineClip(PlayableDirector director) {
        string tempTimelineAssetPath = AssetDatabase.GenerateUniqueAssetPath("Assets/TempRenderCacheTimelineForTestRunner.playable");

        //Create timeline asset
        TimelineAsset timelineAsset = ScriptableObject.CreateInstance<TimelineAsset>();
        director.playableAsset = timelineAsset;
        AssetDatabase.CreateAsset(timelineAsset, tempTimelineAssetPath);
            
        //Create empty asset
        RenderCacheTrack renderCacheTrack  = timelineAsset.CreateTrack<RenderCacheTrack>(null, "Footage");
        TimelineClip clip = renderCacheTrack.CreateDefaultClip();
        RenderCachePlayableAsset sisAsset = clip.asset as RenderCachePlayableAsset;
        Assert.IsNotNull(sisAsset);

        //Select gameObject and open Timeline Window. This will trigger the TimelineWindow's update etc.
        EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
        Selection.activeObject = director;

        return clip;
    }
        
//----------------------------------------------------------------------------------------------------------------------                
    internal static void DestroyTestTimelineAssets(TimelineClip clip) {
        TrackAsset    movieTrack    = clip.GetParentTrack();
        TimelineAsset timelineAsset = movieTrack.timelineAsset;
            
        string tempTimelineAssetPath = AssetDatabase.GetAssetPath(timelineAsset);
        Assert.False(string.IsNullOrEmpty(tempTimelineAssetPath));

        timelineAsset.DeleteTrack(movieTrack);
        ObjectUtility.Destroy(timelineAsset);
        AssetDatabase.DeleteAsset(tempTimelineAssetPath);
            
    }
    
//----------------------------------------------------------------------------------------------------------------------    

    const string SRC_IMAGE_PATH = "Packages/com.unity.streaming-image-sequence/Tests/Data/png/A_00000.png";

}

} //end namespace
