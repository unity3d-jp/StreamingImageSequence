using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Unity.FilmInternalUtilities;
using Unity.StreamingImageSequence.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TestTools;
using UnityEngine.Timeline;
using Assert = NUnit.Framework.Assert;

namespace Unity.StreamingImageSequence.EditorTests {

internal class SISImageTest {

    [UnityTest]
    public IEnumerator DuplicateImage() {
        PlayableDirector                    director = EditorUtilityTest.NewSceneWithDirector();
        TimelineClip                        clip     = EditorUtilityTest.CreateTestSISTimelineClip(director);
        StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
        Assert.IsNotNull(sisAsset);

        TrackAsset track = clip.GetParentTrack();
        Assert.IsNotNull(track);
        
        //Create Image
        

        director.SetGenericBinding();
        
        clip.GetParentTrack().set
        
    
        //Cleanup
        EditorUtilityTest.DestroyTestTimelineAssets(clip);
        yield return null;
    }

//----------------------------------------------------------------------------------------------------------------------
    
    const float EPSILON = 0.001f;
    
    
}
} //end namespace
