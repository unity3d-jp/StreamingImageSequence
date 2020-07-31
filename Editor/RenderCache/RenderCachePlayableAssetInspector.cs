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
        
        //[TODO-sin: 2020-5-27] Check the MD5 hash of the folder before overwriting
        if (GUILayout.Button("Update Render Cache")) {
            
            TimelineClip timelineClip = TimelineEditor.selectedClip;
            TrackAsset track = timelineClip.parentTrack;
            TimelineAsset timelineAsset = track.timelineAsset;
            m_director = FindDirectorInScene(timelineAsset);
            if (null == m_director) {
                EditorUtility.DisplayDialog("Streaming Image Sequence",
                    "PlayableAsset is not loaded in scene. Please load the correct scene before doing this operation.",
                    "Ok");
                return;
            }            
                        
            m_renderCapturer =  m_director.GetGenericBinding(track) as BaseRenderCapturer;
            if (null == m_renderCapturer) {
                EditorUtility.DisplayDialog("Streaming Image Sequence",
                    "Please bind an appropriate RenderCapturer component to the track.",
                    "Ok");
                return;                
            }

            bool canCapture = m_renderCapturer.BeginCapture();
            if (!canCapture) {
                EditorUtility.DisplayDialog("Streaming Image Sequence",
                    m_renderCapturer.GetLastErrorMessage(),
                    "Ok");
                return;                
                
            }
                       
            
            //Loop time             
            m_timePerFrame = 1.0f / timelineAsset.editorSettings.fps;
            EditorCoroutineUtility.StartCoroutine(UpdateRenderCacheCoroutine(), this);
                        
        }
        
        //[TODO-sin: 2020-7-29] Add a button to delete all images in the folder
        
        
        InspectorUtility.ShowFrameMarkersGUI(m_asset);

        

    }

//----------------------------------------------------------------------------------------------------------------------
    IEnumerator UpdateRenderCacheCoroutine() {
        Assert.IsNotNull(m_renderCapturer);
        Assert.IsNotNull(m_director);
        
        
        //Check output folder
        string outputFolder = m_asset.GetFolder();
        if (string.IsNullOrEmpty(outputFolder) || !Directory.Exists(outputFolder)) {
            outputFolder = FileUtil.GetUniqueTempPathInProject();
            Directory.CreateDirectory(outputFolder);
            m_asset.SetFolder(outputFolder);
        }

        Texture capturerTex = m_renderCapturer.GetInternalTexture();
               
        //Show progress in game view
        GameObject progressGo = new GameObject("Blitter");
        LegacyTextureBlitter blitter = progressGo.AddComponent<LegacyTextureBlitter>();
        blitter.SetTexture(capturerTex);
        blitter.SetCameraDepth(int.MaxValue);

        TimelineClipSISData timelineClipSISData = m_asset.GetBoundTimelineClipSISData();
        TimelineClip timelineClip = timelineClipSISData.GetOwner();
        m_nextDirectorTime = timelineClip.start;
        
        int  fileCounter = 0;
        int numFiles = (int) Math.Ceiling(timelineClip.duration / m_timePerFrame) + 1;
        int numDigits = MathUtility.GetNumDigits(numFiles);

        string prefix = $"{m_asset.name}_";
 
        //Store old files that has the same pattern
        string[] existingFiles = Directory.GetFiles (outputFolder, $"{prefix}*.png");
        HashSet<string> filesToDelete = new HashSet<string>(existingFiles);
        
        bool cancelled = false;
        while (m_nextDirectorTime <= timelineClip.end && !cancelled) {
            
            SISPlayableFrame playableFrame = timelineClipSISData.GetPlayableFrame(fileCounter);
            bool useFrame = (null!=playableFrame && (playableFrame.IsUsed()) || fileCounter == 0);
            
            if (useFrame) {
                SetDirectorTime(m_director, m_nextDirectorTime);
                yield return null;
            }

            blitter.SetTexture(capturerTex);
            yield return null;

            
            string fileName       = $"{prefix}{fileCounter.ToString($"D{numDigits}")}.png";
            string outputFilePath = Path.Combine(outputFolder, fileName);

            if (filesToDelete.Contains(outputFilePath)) {
                filesToDelete.Remove(outputFilePath);
            }

            //[TODO-sin: 2020-5-27] Call StreamingImageSequencePlugin API to unload texture because it may be overwritten           
            m_renderCapturer.CaptureToFile(outputFilePath);


            m_nextDirectorTime += m_timePerFrame;
            ++fileCounter;
        
            cancelled = EditorUtility.DisplayCancelableProgressBar(
                "StreamingImageSequence", "Caching render results", ((float)fileCounter / numFiles));            
        }
        
        //Delete old files
        foreach (string oldFile in filesToDelete) {
            File.Delete(oldFile);
        }
        
               
        //Cleanup
        EditorUtility.ClearProgressBar();
        m_renderCapturer.EndCapture();
        ObjectUtility.Destroy(progressGo);
        
        
        yield return null;

    }
 
    

//----------------------------------------------------------------------------------------------------------------------
    private static void SetDirectorTime(PlayableDirector director, double time) {
        director.time = time;
        TimelineEditor.Refresh(RefreshReason.ContentsModified); 
    }    

    
//----------------------------------------------------------------------------------------------------------------------

    PlayableDirector FindDirectorInScene(TimelineAsset timelineAsset) {
        PlayableDirector[] directors = UnityEngine.Object.FindObjectsOfType<PlayableDirector>();
        foreach (PlayableDirector director in directors) {
            if (timelineAsset == director.playableAsset) {
                return director;
            }            
        }

        return null;
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
    private PlayableDirector m_director = null;

    private BaseRenderCapturer m_renderCapturer = null;
    private double m_nextDirectorTime = 0;
    private double m_timePerFrame     = 0;

}

}