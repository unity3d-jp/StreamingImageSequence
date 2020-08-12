using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using Unity.EditorCoroutines.Editor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.StreamingImageSequence;
using UnityEngine.Timeline;

namespace UnityEditor.StreamingImageSequence {

/// <summary>
/// The inspector of RenderCachePlayableAsset
/// </summary>
[CustomEditor(typeof(RenderCachePlayableAsset))]
internal class RenderCachePlayableAssetInspector : Editor {

//----------------------------------------------------------------------------------------------------------------------
    void OnEnable() {
        m_asset = target as RenderCachePlayableAsset;
    }

    
//----------------------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI() {
        
        //View resolution
        Vector2 res = ViewEditorUtility.GetMainGameViewSize();
        EditorGUILayout.LabelField("Resolution (Modify GameView size to change)");
        ++EditorGUI.indentLevel;
        EditorGUILayout.LabelField("Width", res.x.ToString(CultureInfo.InvariantCulture));
        EditorGUILayout.LabelField("Height", res.y.ToString(CultureInfo.InvariantCulture));        
        --EditorGUI.indentLevel;
        EditorGUILayout.Space(15f);

        string prevFolder = m_asset.GetFolder();
        string newFolder = DrawFolderSelector ("Cache Output Folder", "Select Folder", 
            prevFolder,
            prevFolder,
            AssetEditorUtility.NormalizeAssetPath
        );

        if (newFolder != prevFolder) {
            m_asset.SetFolder(newFolder);
            GUIUtility.ExitGUI();
        }


        if (TimelineEditor.selectedClip.asset != m_asset) {
            return;
        }
        
        TimelineClipSISData timelineClipSISData = m_asset.GetBoundTimelineClipSISData();
        if (null == timelineClipSISData)
            return;
                
        GUILayout.Space(15);
        bool prevMarkerVisibility = timelineClipSISData.AreFrameMarkersVisible();
        TimelineClip timelineClip = TimelineEditor.selectedClip;
        TrackAsset   track        = timelineClip.parentTrack;

        
        //Capture Selected Frames
        GUILayout.BeginHorizontal();
        bool markerVisibility = EditorGUILayout.Toggle("Capture Selected Frames", prevMarkerVisibility);
        if (markerVisibility != prevMarkerVisibility) {
            timelineClipSISData.ShowFrameMarkers(markerVisibility);
        }
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("All", GUILayout.Width(40))) {
            Undo.RegisterCompleteObjectUndo(track, "RenderCachePlayableAsset: Capturing all frames");
            timelineClipSISData.SetAllPlayableFrames(true);
        }
        if (GUILayout.Button("None", GUILayout.Width(40))) {
            Undo.RegisterCompleteObjectUndo(track, "RenderCachePlayableAsset: Capturing no frames");
            timelineClipSISData.SetAllPlayableFrames(false);
            
        }
        GUILayout.EndHorizontal();
       
        //[TODO-sin: 2020-5-27] Check the MD5 hash of the folder before overwriting
        if (GUILayout.Button("Update Render Cache")) {
            
            PlayableDirector director = TimelineEditor.inspectedDirector;
            if (null == director) {
                EditorUtility.DisplayDialog("Streaming Image Sequence",
                    "PlayableAsset is not loaded in scene. Please load the correct scene before doing this operation.",
                    "Ok");
                return;
            }            
            
            //Loop time             
            EditorCoroutineUtility.StartCoroutine(UpdateRenderCacheCoroutine(director, m_asset), this);
                        
        }
    }

    
    
//----------------------------------------------------------------------------------------------------------------------
    internal static IEnumerator UpdateRenderCacheCoroutine(PlayableDirector director, RenderCachePlayableAsset renderCachePlayableAsset) {
        Assert.IsNotNull(director);
        Assert.IsNotNull(renderCachePlayableAsset);
        
        TimelineClipSISData timelineClipSISData = renderCachePlayableAsset.GetBoundTimelineClipSISData();
        if (null == timelineClipSISData) {
            EditorUtility.DisplayDialog("Streaming Image Sequence",
                "RenderCachePlayableAsset is not ready",
                "Ok");
            yield break;
            
        }
            

        TrackAsset track = renderCachePlayableAsset.GetBoundTimelineClipSISData().GetOwner().parentTrack;        
        BaseRenderCapturer renderCapturer = director.GetGenericBinding(track) as BaseRenderCapturer;
        if (null == renderCapturer) {
            EditorUtility.DisplayDialog("Streaming Image Sequence",
                "Please bind an appropriate RenderCapturer component to the track.",
                "Ok");
            yield break;                
        }


        //begin capture
        bool canCapture = renderCapturer.BeginCapture();
        if (!canCapture) {
            EditorUtility.DisplayDialog("Streaming Image Sequence",
                renderCapturer.GetLastErrorMessage(),
                "Ok");
            yield break;                                
        }
        
        
        //Check output folder
        string outputFolder = renderCachePlayableAsset.GetFolder();
        if (string.IsNullOrEmpty(outputFolder) || !Directory.Exists(outputFolder)) {
            outputFolder = FileUtil.GetUniqueTempPathInProject();
            Directory.CreateDirectory(outputFolder);
            renderCachePlayableAsset.SetFolder(outputFolder);
        }

        Texture capturerTex = renderCapturer.GetInternalTexture();
               
        //Show progress in game view
        GameObject progressGo = new GameObject("Blitter");
        LegacyTextureBlitter blitter = progressGo.AddComponent<LegacyTextureBlitter>();
        blitter.SetTexture(capturerTex);
        blitter.SetCameraDepth(int.MaxValue);

        TimelineClip timelineClip = timelineClipSISData.GetOwner();
        double nextDirectorTime = timelineClip.start;
        double timePerFrame = 1.0f / track.timelineAsset.editorSettings.fps;
        
        int  fileCounter = 0;
        int numFiles = (int) Math.Ceiling(timelineClip.duration / timePerFrame) + 1;
        int numDigits = MathUtility.GetNumDigits(numFiles);

        string prefix = $"{renderCachePlayableAsset.name}_";
 
        //Store old files that has the same pattern
        string[] existingFiles = Directory.GetFiles (outputFolder, $"{prefix}*.png");
        HashSet<string> filesToDelete = new HashSet<string>(existingFiles);
        
        bool cancelled = false;
        string prevOutputFilePath = "";
        while (nextDirectorTime <= timelineClip.end && !cancelled) {
            
            SISPlayableFrame playableFrame = timelineClipSISData.GetPlayableFrame(fileCounter);                
            bool useFrame = (null!=playableFrame && (playableFrame.IsUsed()));             
            
            string fileName       = $"{prefix}{fileCounter.ToString($"D{numDigits}")}.png";
            string outputFilePath = Path.Combine(outputFolder, fileName);
            if (filesToDelete.Contains(outputFilePath)) {
                filesToDelete.Remove(outputFilePath);
            }
            
            if (useFrame) {
                SetDirectorTime(director, nextDirectorTime);
                yield return null;
                
                //[TODO-sin: 2020-5-27] Call StreamingImageSequencePlugin API to unload texture because it may be overwritten           
                renderCapturer.CaptureToFile(outputFilePath);
                
            } 


            nextDirectorTime += timePerFrame;
            ++fileCounter;
        
            cancelled = EditorUtility.DisplayCancelableProgressBar(
                "StreamingImageSequence", "Caching render results", ((float)fileCounter / numFiles));
            prevOutputFilePath = outputFilePath;
        }
        
        //Delete old files
        foreach (string oldFile in filesToDelete) {
            File.Delete(oldFile);
        }
        
               
        //Cleanup
        EditorUtility.ClearProgressBar();
        renderCapturer.EndCapture();
        ObjectUtility.Destroy(progressGo);
        
        
        yield return null;

    }
 
    

//----------------------------------------------------------------------------------------------------------------------
    private static void SetDirectorTime(PlayableDirector director, double time) {
        director.time = time;
        TimelineEditor.Refresh(RefreshReason.ContentsModified); 
    }    

    

//----------------------------------------------------------------------------------------------------------------------
    
    private string DrawFolderSelector(string label, 
        string dialogTitle, 
        string fieldValue, 
        string directoryOpenPath, 
        Func<string, string> onValidFolderSelected = null) 
    {

        string newDirPath = fieldValue;
        using(new EditorGUILayout.HorizontalScope()) {
            if (!string.IsNullOrEmpty (label)) {
                EditorGUILayout.PrefixLabel(label);
            } 

            EditorGUILayout.SelectableLabel(fieldValue,
                EditorStyles.textField, GUILayout.Height(EditorGUIUtility.singleLineHeight)
            );

            newDirPath = InspectorUtility.ShowSelectFolderButton(dialogTitle, directoryOpenPath, onValidFolderSelected);

            if (GUILayout.Button("Show", GUILayout.Width(50f))) {
                EditorUtility.RevealInFinder(newDirPath);
            }

        }
        return newDirPath;
    }
    

//----------------------------------------------------------------------------------------------------------------------

    
    private RenderCachePlayableAsset m_asset = null;
 
}

}