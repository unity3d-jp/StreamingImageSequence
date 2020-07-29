using System;
using System.Collections;
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
            //[TODO-sin: 2020-5-27] Copy images from prevFolder to newFolder
            m_asset.SetFolder(newFolder);
        }

        
        //[TODO-sin: 2020-5-27] Check the MD5 hash of the folder before overwriting
        if (GUILayout.Button("Update Render Cache")) {

            TimelineClip timelineClip = m_asset.GetTimelineClip();
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
                    "Please bind an appropriate RTCapturer component to the track.",
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
        
        TimelineClip timelineClip = m_asset.GetTimelineClip();
        m_nextDirectorTime = timelineClip.start;
        
        int  fileCounter = 0;
        int numFiles = (int) Math.Ceiling(timelineClip.duration / m_timePerFrame) + 1;
        int numDigits = MathUtility.GetNumDigits(numFiles);
        
        bool cancelled = false;
        while (m_nextDirectorTime <= timelineClip.end && !cancelled) {
            SetDirectorTime(m_director, m_nextDirectorTime);
            blitter.SetTexture(capturerTex);
            yield return null;            
            
            string fileName       = fileCounter.ToString($"D{numDigits}") + ".png";
            string outputFilePath = Path.Combine(outputFolder, fileName);
                        
            //[TODO-sin: 2020-5-27] Call StreamingImageSequencePlugin API to unload texture because it may be overwritten
           
            m_renderCapturer.CaptureToFile(outputFilePath);
            m_nextDirectorTime += m_timePerFrame;
            ++fileCounter;
        
            cancelled = EditorUtility.DisplayCancelableProgressBar(
                "StreamingImageSequence", "Caching render results", ((float)fileCounter / numFiles));            
        }
        
               
        //Cleanup
        EditorUtility.ClearProgressBar();
        m_renderCapturer.EndCapture();
        ObjectUtility.Destroy(progressGo);
        
        
        yield return null;

    }
 
//----------------------------------------------------------------------------------------------------------------------
    

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
            Rect folderRect = GUILayoutUtility.GetLastRect();
            

            if(GUILayout.Button("Select", GUILayout.Width(50f))) {
                string folderSelected = EditorUtility.OpenFolderPanel(dialogTitle, directoryOpenPath, "");
                if(!string.IsNullOrEmpty(folderSelected)) {
                    if (onValidFolderSelected != null) {
                        newDirPath = onValidFolderSelected (folderSelected);
                    } else {
                        newDirPath = folderSelected;
                    }
                }
            }
        }
        return newDirPath;
    }
    

//----------------------------------------------------------------------------------------------------------------------

    
    private RenderCachePlayableAsset m_asset = null;
    private PlayableDirector m_director = null;

    private BaseRenderCapturer m_renderCapturer = null;
    private double m_nextDirectorTime = 0;
    private double m_timePerFrame       = 0;

}

}