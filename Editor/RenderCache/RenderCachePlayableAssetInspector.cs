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
using Object = UnityEngine.Object;

namespace UnityEditor.StreamingImageSequence {

/// <summary>
/// The inspector of RenderCachePlayableAsset
/// </summary>
[CustomEditor(typeof(RenderCachePlayableAsset))]
internal class RenderCachePlayableAssetInspector : Editor {

    [InitializeOnLoadMethod]
    static void RenderCachePlayableAssetInspector_OnEditorLoad() {
        Selection.selectionChanged += RenderCachePlayableAssetInspector_OnSelectionChanged;
    }

    static void RenderCachePlayableAssetInspector_OnSelectionChanged() {
        if (!m_lockMode)
            return;
        
        //Abort lock mode if we are not selecting marker
        foreach (var selectedObj in Selection.objects) {
            FrameMarker marker = selectedObj as FrameMarker;
            if (null == marker) {
                UnlockSISData();
                return;                
            }

            if (m_inspectedSISDataForLocking != marker.GetOwner().GetOwner()) {
                UnlockSISData();
                return;
            }

        }         
    }

    
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

        ValidateAssetFolder();
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
        
        //Capture Selected Frames
        ShowCaptureSelectedFramesGUI(TimelineEditor.selectedClip, timelineClipSISData);
        ShowLockFramesGUI(TimelineEditor.selectedClip, timelineClipSISData);
       
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
        while (nextDirectorTime <= timelineClip.end && !cancelled) {
            
            SISPlayableFrame playableFrame = timelineClipSISData.GetPlayableFrame(fileCounter);                
            bool captureFrame = (!timelineClipSISData.AreFrameMarkersVisible() //if not visible, use it
                || (null!=playableFrame && playableFrame.IsUsed() && !playableFrame.IsLocked())
            );             
            
            string fileName       = $"{prefix}{fileCounter.ToString($"D{numDigits}")}.png";
            string outputFilePath = Path.Combine(outputFolder, fileName);
            if (filesToDelete.Contains(outputFilePath)) {
                filesToDelete.Remove(outputFilePath);
            }
            
            if (captureFrame) {
                SetDirectorTime(director, nextDirectorTime);
                yield return null;
                
                //[TODO-sin: 2020-5-27] Call StreamingImageSequencePlugin API to unload texture because it may be overwritten           
                renderCapturer.CaptureToFile(outputFilePath);
                
            } 


            nextDirectorTime += timePerFrame;
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
        renderCapturer.EndCapture();
        ObjectUtility.Destroy(progressGo);
        
        
        yield return null;

    }
    
    
//----------------------------------------------------------------------------------------------------------------------


    private void ShowCaptureSelectedFramesGUI(TimelineClip timelineClip, TimelineClipSISData timelineClipSISData) {
        bool         prevMarkerVisibility = timelineClipSISData.AreFrameMarkersVisible();
        TrackAsset   track                = timelineClip.parentTrack;
        
        GUILayout.BeginHorizontal();
        bool markerVisibility = EditorGUILayout.Toggle("Capture Selected Frames", prevMarkerVisibility);
        if (markerVisibility != prevMarkerVisibility) {
            timelineClipSISData.ShowFrameMarkers(markerVisibility);
        }
        GUILayout.FlexibleSpace();
        EditorGUI.BeginDisabledGroup(!markerVisibility);        
        if (GUILayout.Button("All", GUILayout.Width(40))) {
            Undo.RegisterCompleteObjectUndo(track, "RenderCachePlayableAsset: Capturing all frames");
            timelineClipSISData.SetAllPlayableFramesProperty(PlayableFramePropertyID.USED, true);
        }
        if (GUILayout.Button("None", GUILayout.Width(40))) {
            Undo.RegisterCompleteObjectUndo(track, "RenderCachePlayableAsset: Capturing no frame");
            timelineClipSISData.SetAllPlayableFramesProperty(PlayableFramePropertyID.USED, false);            
        }
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
        
    }

//----------------------------------------------------------------------------------------------------------------------


    private void ShowLockFramesGUI(TimelineClip timelineClip, TimelineClipSISData timelineClipSISData) {
        TrackAsset track = timelineClip.parentTrack;
        
        using(new EditorGUILayout.HorizontalScope()) {
            EditorGUILayout.PrefixLabel("Lock Frames");
            
            bool lockMode = GUILayout.Toggle(m_lockMode, EditorTextures.GetLockTexture(), "Button", 
                GUILayout.Height(20f), GUILayout.Width(30f));            
            if (lockMode != m_lockMode) { //lock state changed
                if (lockMode) {
                    LockSISData(timelineClipSISData);
                } else {
                    UnlockSISData();
                }
            }
            
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(!m_lockMode);        
            if (GUILayout.Button("All", GUILayout.Width(40))) {
                Undo.RegisterCompleteObjectUndo(track, "RenderCachePlayableAsset: Locking all frames");
                timelineClipSISData.SetAllPlayableFramesProperty(PlayableFramePropertyID.LOCKED, true);
            }
            if (GUILayout.Button("None", GUILayout.Width(40))) {
                Undo.RegisterCompleteObjectUndo(track, "RenderCachePlayableAsset: Locking no frame");
                timelineClipSISData.SetAllPlayableFramesProperty(PlayableFramePropertyID.LOCKED, false);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
    
//----------------------------------------------------------------------------------------------------------------------
    private void ValidateAssetFolder() {

        string folder = m_asset.GetFolder();
        if (!string.IsNullOrEmpty(folder) && Directory.Exists(folder))
            return;

        //Generate unique folder
        string baseFolder = Path.Combine(Application.streamingAssetsPath, m_asset.name);
        folder = baseFolder;
        int index = 1;
        while (Directory.Exists(folder)) {
            folder = baseFolder + index.ToString();
        }
                
        Directory.CreateDirectory(folder);
        m_asset.SetFolder(folder);
    }
    
//----------------------------------------------------------------------------------------------------------------------

    static void LockSISData(TimelineClipSISData timelineClipSISData) {
        m_inspectedSISDataForLocking = timelineClipSISData;
        m_inspectedSISDataForLocking.SetInspectedProperty(PlayableFramePropertyID.LOCKED);
        m_lockMode = true;
    }
    
    static void UnlockSISData() {
        Assert.IsNotNull(m_inspectedSISDataForLocking);
        m_inspectedSISDataForLocking.SetInspectedProperty(PlayableFramePropertyID.USED);
        m_inspectedSISDataForLocking = null;
        m_lockMode = false;
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
    private static bool m_lockMode = false;
    private static TimelineClipSISData m_inspectedSISDataForLocking = null;

}

}