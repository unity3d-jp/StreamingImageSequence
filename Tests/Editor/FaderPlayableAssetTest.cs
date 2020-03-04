using NUnit.Framework;
using System.Collections;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.StreamingImageSequence;
using UnityEngine.TestTools;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace UnityEditor.StreamingImageSequence.Tests {

internal class FaderPlayableAssetTest {

    [UnityTest]
    public IEnumerator CreatePlayableAsset() {
        Color col = Color.green;
        GameObject directorGo = new GameObject("Director");
        PlayableDirector director = directorGo.AddComponent<PlayableDirector>();

        //Create timeline asset
        TimelineAsset asset = ScriptableObject.CreateInstance<TimelineAsset>();
        director.playableAsset = asset;

        //Create empty asset
        FaderPlayableAsset faderAsset = ScriptableObject.CreateInstance<FaderPlayableAsset>();
        FaderTrack faderTrack = asset.CreateTrack<FaderTrack>(null, "Footage");
        TimelineClip clip = faderTrack.CreateDefaultClip();
        clip.asset = faderAsset;
        faderAsset.SetFadeType(FadeType.FADE_IN);
        faderAsset.SetColor(col);

        //Create new Image 
        GameObject canvasObj = new GameObject("Canvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<UnityEngine.UI.CanvasScaler>();
        canvasObj.AddComponent<GraphicRaycaster>();

        GameObject imageObj = new GameObject("Image"); 
        imageObj.transform.SetParent(canvas.transform);
        Image image = imageObj.AddComponent<Image>();             
        director.SetGenericBinding(faderTrack, image);

        //Select gameObject and open Timeline Window. This will trigger the TimelineWindow's update etc.
        EditorApplication.ExecuteMenuItem("Window/Sequencing/Timeline");
        Selection.activeTransform = directorGo.transform;
        yield return null;

        TimelineEditor.selectedClip = clip;
        yield return null;

        //FadeIn
        Color zeroAlphaCol = col;
        zeroAlphaCol.a = 0;           
        Assert.AreEqual(zeroAlphaCol, image.color);

        //FadeOut
        faderAsset.SetFadeType(FadeType.FADE_OUT);           
        TimelineEditor.Refresh(RefreshReason.ContentsModified);
        yield return null; //Give time for the Timeline Window to update.
        Assert.AreEqual(col, image.color);
    }
}

} //end namespace
