using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using Unity.FilmInternalUtilities;
using Unity.StreamingImageSequence.Editor;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.TestTools;
using UnityEngine.Timeline;
using Assert = NUnit.Framework.Assert;
using UIImage = UnityEngine.UI.Image;

namespace Unity.StreamingImageSequence.EditorTests {

internal class SISImageTest {

    [UnityTest]
    public IEnumerator CheckAutomaticSpriteCreation() {
        PlayableDirector                    director = EditorUtilityTest.NewSceneWithDirector();
        TimelineClip                        clip     = EditorUtilityTest.CreateTestSISTimelineClip(director);
        StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
        Assert.IsNotNull(sisAsset);

        TrackAsset track = clip.GetParentTrack();
        director.time = clip.start;
        Assert.IsNotNull(track);
        
        //Create Image        
        UIImage image0 = CreateImageWithSISRenderer(out StreamingImageSequenceRenderer sisRenderer);            
        director.SetGenericBinding(track, sisRenderer);
        TimelineEditor.Refresh(RefreshReason.ContentsModified);
        yield return null;
        
        //Test that the sprite is automatically created
        Assert.IsNotNull(image0.sprite, "Sprite is null");        
        yield return null;
    
        //Cleanup
        EditorUtilityTest.DestroyTestTimelineAssets(clip);
        yield return null;
    }

//----------------------------------------------------------------------------------------------------------------------


    UIImage CreateImageWithSISRenderer(out StreamingImageSequenceRenderer sisRenderer) {
        UIImage image = FilmInternalUtilities.ObjectUtility.CreateGameObjectWithComponent<UIImage>("");
        Assert.IsNotNull(image);
        
        sisRenderer = image.gameObject.AddComponent<StreamingImageSequenceRenderer>();       
        Assert.IsNotNull(sisRenderer);
        return image;        
    }
    
    
    
}
} //end namespace
