﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using NUnit.Framework;
using Unity.FilmInternalUtilities;
using Unity.FilmInternalUtilities.Editor;
using Unity.EditorCoroutines.Editor;
using UnityEditor.ShortcutManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Playables;
using UnityEditor;
using UnityEngine.Timeline;

namespace Unity.StreamingImageSequence.Editor {

/// <summary>
/// The inspector of RenderCachePlayableAsset
/// </summary>
[CustomEditor(typeof(RenderCachePlayableAsset))]
internal class RenderCachePlayableAssetInspector : UnityEditor.Editor {

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

            if (m_inspectedClipDataForLocking != marker.GetOwner().GetOwner()) {
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

        //Check if the asset is actually inspected
        TimelineClip selectedClip = TimelineEditor.selectedClip;
        if (null == selectedClip)
            return;
        
        if (selectedClip.asset != m_asset) {
            return;
        }

        ValidateAssetFolder();

        string prevFolder = m_asset.GetFolder();
        
        string newFolder = EditorGUIDrawerUtility.DrawFolderSelectorGUI("Cache Output Folder", "Select Folder", 
            prevFolder, null
        );

        newFolder = AssetEditorUtility.NormalizePath(newFolder);

        if (newFolder != prevFolder) {
            Undo.RecordObject(m_asset,"Change Output Folder");
            m_asset.SetFolder(AssetEditorUtility.NormalizePath(newFolder));
            GUIUtility.ExitGUI();
        }
        
        //Output Format
        EditorGUIDrawerUtility.DrawUndoableGUI(m_asset, "RenderCache Output Format", 
            /*guiFunc=*/ ()=> (RenderCacheOutputFormat) EditorGUILayout.EnumPopup("Output Format:", m_asset.GetOutputFormat()), 
            /*updateFunc=*/ (RenderCacheOutputFormat newOutputFormat) => { m_asset.SetOutputFormat(newOutputFormat); }
        );
                
        RenderCacheClipData clipData = m_asset.GetBoundClipData();
        if (null == clipData)
            return;
                
        GUILayout.Space(15);
        
        //Capture Selected Frames
        using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
            DrawCaptureSelectedFramesGUI(selectedClip, clipData);
            DrawLockFramesGUI(selectedClip, clipData);
        }
        
        GUILayout.Space(15);
        using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
            EditorGUILayout.LabelField("Background Colors");
            ++EditorGUI.indentLevel;

            RenderCachePlayableAssetEditorConfig editorConfig = m_asset.GetEditorConfig();
            
            EditorGUIDrawerUtility.DrawUndoableGUI(m_asset, "Change Update BG Color", 
                /*guiFunc=*/ ()=> EditorGUILayout.ColorField("In Game Window (Update)", editorConfig.GetUpdateBGColor()), 
                /*updateFunc=*/ (Color newColor) => { editorConfig.SetUpdateBGColor(newColor); }
            );
            
            EditorGUIDrawerUtility.DrawUndoableGUI(m_asset, "Change Timeline BG Color", 
                /*guiFunc=*/ ()=> EditorGUILayout.ColorField("In Timeline Window", m_asset.GetTimelineBGColor()), 
                /*updateFunc=*/ (Color newColor) => {  m_asset.SetTimelineBGColor(newColor); }
            );
            
            --EditorGUI.indentLevel;
            GUILayout.Space(5);
        }       
        GUILayout.Space(15);
        DrawUpdateRenderCacheGUI(selectedClip);

    }
    
//----------------------------------------------------------------------------------------------------------------------

    private void DrawUpdateRenderCacheGUI(TimelineClip clip) {
        RenderCacheClipData clipData = m_asset.GetBoundClipData();
        Assert.IsNotNull(clipData);
        if (clipData.GetOwner().GetParentTrack().IsNullRef())
            return;
        
        ShortcutBinding updateRenderCacheShortcut 
            = ShortcutManager.instance.GetShortcutBinding(SISEditorConstants.SHORTCUT_UPDATE_RENDER_CACHE);            

        using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
            RenderCachePlayableAssetEditorConfig editorConfig = m_asset.GetEditorConfig();
            
            EditorGUI.BeginChangeCheck();
            
            bool captureAllFrames = EditorGUILayout.Toggle("Capture All Frames", editorConfig.GetCaptureAllFrames());
            EditorGUI.BeginDisabledGroup(captureAllFrames);
            ++EditorGUI.indentLevel;

            int captureStartFrame = Math.Max(0,editorConfig.GetCaptureStartFrame());
            int captureEndFrame   = editorConfig.GetCaptureEndFrame();
            if (captureEndFrame < 0) {
                captureEndFrame = TimelineUtility.CalculateNumFrames(clip);
            }

            captureStartFrame = EditorGUILayout.IntField("From", captureStartFrame);
            captureEndFrame   = EditorGUILayout.IntField("To", captureEndFrame);

            --EditorGUI.indentLevel;                        
            EditorGUI.EndDisabledGroup();
            
            if (EditorGUI.EndChangeCheck()) {
                Undo.RecordObject(m_asset,"Change Frames to Capture");
                editorConfig.SetCaptureAllFrames(captureAllFrames);
                editorConfig.SetCaptureStartFrame(captureStartFrame);
                editorConfig.SetCaptureEndFrame(captureEndFrame);
                
            }
            
            
            GUILayout.Space(10);
            
            PlayableDirector director = TimelineEditor.inspectedDirector;
            if (null == director) 
                return;
            if (GUILayout.Button($"Update Render Cache ({updateRenderCacheShortcut})")) {            
                EditorCoroutineUtility.StartCoroutine(UpdateRenderCacheCoroutine(director, m_asset), this);                
            }
        }
        
    }
    
//----------------------------------------------------------------------------------------------------------------------
    internal static IEnumerator UpdateRenderCacheCoroutine(PlayableDirector director, RenderCachePlayableAsset renderCachePlayableAsset) {
        Assert.IsNotNull(director);
        Assert.IsNotNull(renderCachePlayableAsset);
        
        PlayableFrameClipData clipData = renderCachePlayableAsset.GetBoundClipData();
        if (null == clipData) {
            EditorUtility.DisplayDialog("Streaming Image Sequence",
                "RenderCachePlayableAsset is not ready",
                "Ok");
            yield break;
            
        }           

        TimelineClip       timelineClip   = clipData.GetOwner();
        TrackAsset         track          = timelineClip.GetParentTrack();
        BaseRenderCapturer renderCapturer = director.GetGenericBinding(track) as BaseRenderCapturer;
        if (null == renderCapturer) {
            EditorUtility.DisplayDialog("Streaming Image Sequence",
                "Please bind an appropriate RenderCapturer component to the track.",
                "Ok");
            yield break;                
        }

        //Check output folder
        string outputFolder = renderCachePlayableAsset.GetFolder();
        if (string.IsNullOrEmpty(outputFolder) || !Directory.Exists(outputFolder)) {
            EditorUtility.DisplayDialog("Streaming Image Sequence",
                $"Invalid output folder: {outputFolder}",
                "Ok");
            yield break;                                
        }

        //Check if we can capture
        bool canCapture = renderCapturer.CanCaptureV();
        if (!canCapture) {
            EditorUtility.DisplayDialog("Streaming Image Sequence",
                renderCapturer.GetLastErrorMessage(),
                "Ok");
            yield break;                                            
        }

        //begin capture
        IEnumerator beginCapture = renderCapturer.BeginCaptureV();
        while (beginCapture.MoveNext()) {
            yield return beginCapture.Current;
        }

        //Show progress in game view
        Texture capturerTex = renderCapturer.GetInternalTexture();
        RenderCachePlayableAssetEditorConfig editorConfig = renderCachePlayableAsset.GetEditorConfig();        
        BaseTextureBlitter blitter = CreateBlitter(capturerTex);
        Material blitToScreenMat = renderCapturer.GetOrCreateBlitToScreenEditorMaterialV();
        if (!blitToScreenMat.IsNullRef()) {
            blitToScreenMat.SetColor(m_bgColorProperty, editorConfig.GetUpdateBGColor());
            blitter.SetBlitMaterial(blitToScreenMat);            
        }
        
        GameObject blitterGO = blitter.gameObject;

        double timePerFrame = 1.0f / track.timelineAsset.editorSettings.GetFPS();
        
        //initial calculation of loop vars
        bool captureAllFrames = editorConfig.GetCaptureAllFrames();
        int  fileCounter      = 0;
        int  numFiles         = TimelineUtility.CalculateNumFrames(timelineClip);
        int  maxFrame         = numFiles - 1;
        int  numDigits        = MathUtility.GetNumDigits(numFiles);
        if (!captureAllFrames) {
            fileCounter = editorConfig.GetCaptureStartFrame();
            numFiles    = (editorConfig.GetCaptureEndFrame() - fileCounter) + 1;
            if (numFiles <= 0) {
                EditorUtility.DisplayDialog("Streaming Image Sequence", "Invalid Start/End Frame Settings", "Ok");
                yield break;                                            
            }
        }
        int captureStartFrame = fileCounter;
        
        string prefix = $"{timelineClip.displayName}_";
 

        RenderCacheOutputFormat outputFormat = renderCachePlayableAsset.GetOutputFormat();
        string                  outputExt    = null;
        switch (outputFormat) {
            case RenderCacheOutputFormat.EXR: outputExt = "exr"; break;
            default:                          outputExt = "png"; break;;
        }
        
        bool cancelled      = false;
        bool captureSuccessful = true;
        while (!cancelled && captureSuccessful) {
            
            //Always recalculate from start to avoid floating point errors
            double directorTime = timelineClip.start + (fileCounter * timePerFrame);
            if (directorTime > timelineClip.end)
                break;

            if (!captureAllFrames && fileCounter > editorConfig.GetCaptureEndFrame())
                break;            
            
            string outputFilePath = GenerateImageSequencePath(outputFolder, prefix, fileCounter, numDigits, outputExt); 

            SISPlayableFrame playableFrame = clipData.GetPlayableFrame(fileCounter);                
            bool captureFrame = (!clipData.AreFrameMarkersRequested() //if markers are not requested, capture
                || !File.Exists(outputFilePath) //if file doesn't exist, capture
                || (null!=playableFrame && playableFrame.IsUsed() && !playableFrame.IsLocked())
            );             
            
           
            if (captureFrame) {
                SetDirectorTime(director, directorTime);
                
                //Need at least two frames in order to wait for the TimelineWindow to be updated ?
                yield return null;
                yield return null;
                yield return null;
                
                //Unload texture because it may be overwritten
                StreamingImageSequencePlugin.UnloadImageAndNotify(outputFilePath);
                captureSuccessful = renderCapturer.TryCaptureToFile(outputFilePath, outputFormat);
            } 

            ++fileCounter;        
            cancelled = EditorUtility.DisplayCancelableProgressBar(
                "StreamingImageSequence", "Caching render results", ((float)(fileCounter - captureStartFrame) / numFiles));
        }
        
        AnalyticsSender.SendEventInEditor(
            new RenderCacheClipUpdateEvent(timelineClip.duration, clipData.AreFrameMarkersRequested(),
                fileCounter - captureStartFrame + 1 , (int) outputFormat
            )
        );
        
        if (!cancelled) {

            //Delete old files that has the same extension, and has fileCounter that is more than max 
            string[]        existingFiles = Directory.GetFiles (outputFolder, $"*.{outputExt}");
            HashSet<string> filesToDelete = new HashSet<string>(existingFiles);
            for (int i = 0; i <= maxFrame; ++i) {
                string outputFilePath = GenerateImageSequencePath(outputFolder, prefix, i, numDigits, outputExt); 
                filesToDelete.Remove(outputFilePath);                           
            }

            if (AssetDatabase.IsValidFolder(outputFolder)) {
                foreach (string oldFile in filesToDelete) {                
                    AssetDatabase.DeleteAsset(oldFile);
                }                
            } else {
                foreach (string oldFile in filesToDelete) {                
                    File.Delete(oldFile);
                }
                
            }            
        }

        if (!captureSuccessful) {
            EditorUtility.DisplayDialog("Streaming Image Sequence", "Capturing failed", "Ok");
        }
        
        //Notify
        FolderContentsChangedNotifier.GetInstance().Notify(outputFolder);        
               
        //Cleanup
        EditorUtility.ClearProgressBar();
        renderCapturer.EndCaptureV();
        ObjectUtility.Destroy(blitterGO);        
        AssetDatabase.Refresh();
        renderCachePlayableAsset.Reload();;
        
        
        
        yield return null;

    }

//----------------------------------------------------------------------------------------------------------------------

    private static BaseTextureBlitter CreateBlitter(Texture texToBlit) {
        GameObject           blitterGO = new GameObject("Blitter");

#if AT_USE_HDRP        
        HDRPTextureEndFrameBlitter blitter = blitterGO.AddComponent<HDRPTextureEndFrameBlitter>();
        blitter.SetTargetCameraType(CameraType.Game);
#elif AT_USE_URP        
        URPTextureBlitter blitter = blitterGO.AddComponent<URPTextureBlitter>();        
#else        
        LegacyTextureBlitter blitter = blitterGO.AddComponent<LegacyTextureBlitter>();
#endif        
        blitter.SetSrcTexture(texToBlit);
        blitter.SetCameraDepth(int.MaxValue);
        
        return blitter;
    } 
    
    
//----------------------------------------------------------------------------------------------------------------------


    private void DrawCaptureSelectedFramesGUI(TimelineClip timelineClip, PlayableFrameClipData clipData) {
        TrackAsset   track              = timelineClip.GetParentTrack();
        
        GUILayout.BeginHorizontal();
        bool markerVisibility = InspectorUtility.DrawFrameMarkersGUI(m_asset);
        
        GUILayout.FlexibleSpace();
        EditorGUI.BeginDisabledGroup(!markerVisibility);        
        if (GUILayout.Button("Capture All", GUILayout.Width(80))) {
            Undo.RegisterCompleteObjectUndo(track, "Capturing all frames");
            clipData.SetAllPlayableFramesProperty(PlayableFramePropertyID.USED, true);
        }
        if (GUILayout.Button("Reset", GUILayout.Width(50))) {
            Undo.RegisterCompleteObjectUndo(track, "Capturing no frames");
            clipData.SetAllPlayableFramesProperty(PlayableFramePropertyID.USED, false);            
        }
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
        
    }

//----------------------------------------------------------------------------------------------------------------------


    private void DrawLockFramesGUI(TimelineClip timelineClip, PlayableFrameClipData clipData) {
        TrackAsset track = timelineClip.GetParentTrack();
        
        using(new EditorGUILayout.HorizontalScope()) {
            EditorGUILayout.PrefixLabel("Lock Frames");
            
            bool lockMode = GUILayout.Toggle(m_lockMode, EditorTextures.GetLockTexture(), "Button", 
                GUILayout.Height(20f), GUILayout.Width(30f));            
            if (lockMode != m_lockMode) { //lock state changed
                if (lockMode) {
                    LockSISData(clipData);
                } else {
                    UnlockSISData();
                }
            }
            
            GUILayout.FlexibleSpace();
            EditorGUI.BeginDisabledGroup(!m_lockMode);        
            if (GUILayout.Button("Lock All", GUILayout.Width(80))) {
                Undo.RegisterCompleteObjectUndo(track, "Locking all frames");
                clipData.SetAllPlayableFramesProperty(PlayableFramePropertyID.LOCKED, true);
            }
            if (GUILayout.Button("Reset", GUILayout.Width(50))) {
                Undo.RegisterCompleteObjectUndo(track, "Locking no frames");
                clipData.SetAllPlayableFramesProperty(PlayableFramePropertyID.LOCKED, false);
            }
            EditorGUI.EndDisabledGroup();
        }
    }
    
//----------------------------------------------------------------------------------------------------------------------
    private void ValidateAssetFolder() {

        string folder = m_asset.GetFolder();
        if (!string.IsNullOrEmpty(folder)) {
            
            if (Directory.Exists(folder))           
                return;
            
            try {
                Directory.CreateDirectory(folder);
                return;
            } catch {
                //Fallback to the below code
            }
        }

        AssignDefaultAssetFolder();
                
    }

//----------------------------------------------------------------------------------------------------------------------

    private void AssignDefaultAssetFolder() {
        string assetName = string.IsNullOrEmpty(m_asset.name) ? "RenderCachePlayableAsset" : m_asset.name;               
        //Generate unique folder
        string baseFolder = Path.Combine(Application.streamingAssetsPath, assetName);
        string folder = Unity.FilmInternalUtilities.PathUtility.GenerateUniqueFolder(baseFolder); 
        m_asset.SetFolder(AssetEditorUtility.NormalizePath(folder).Replace('\\','/'));
        Repaint();        
    }
    
//----------------------------------------------------------------------------------------------------------------------

    static void LockSISData(PlayableFrameClipData clipData) {
        m_inspectedClipDataForLocking = clipData;
        m_inspectedClipDataForLocking.SetInspectedProperty(PlayableFramePropertyID.LOCKED);
        m_lockMode = true;
    }
    
    static void UnlockSISData() {
        Assert.IsNotNull(m_inspectedClipDataForLocking);
        m_inspectedClipDataForLocking.SetInspectedProperty(PlayableFramePropertyID.USED);
        m_inspectedClipDataForLocking = null;
        m_lockMode = false;
    }
    
//----------------------------------------------------------------------------------------------------------------------
    private static void SetDirectorTime(PlayableDirector director, double time) {
        director.time = time;
        TimelineEditor.Refresh(RefreshReason.SceneNeedsUpdate); 
    }        

//----------------------------------------------------------------------------------------------------------------------
    private static string GenerateImageSequencePath(string folder, string prefix, int fileCounter, int numDigits, string ext) {
        string fileName = $"{prefix}{fileCounter.ToString($"D{numDigits}")}.{ext}";
        return Path.Combine(folder, fileName);
        
    }
    
//----------------------------------------------------------------------------------------------------------------------
 
    
    private                 RenderCachePlayableAsset m_asset                       = null;
    private static          bool                     m_lockMode                    = false;
    private static          PlayableFrameClipData    m_inspectedClipDataForLocking = null;
    private static readonly int                      m_bgColorProperty             = Shader.PropertyToID("_BGColor");
    
}

}