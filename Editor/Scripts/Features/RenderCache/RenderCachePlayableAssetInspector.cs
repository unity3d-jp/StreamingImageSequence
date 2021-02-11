using System;
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

        //Check if the asset is actually inspected
        if (null!=TimelineEditor.selectedClip && TimelineEditor.selectedClip.asset != m_asset) {
            return;
        }

        ValidateAssetFolder();

        string prevFolder = m_asset.GetFolder();
        
        string newFolder = EditorGUIDrawerUtility.DrawFolderSelectorGUI("Cache Output Folder", "Select Folder", 
            prevFolder,
            null,
            AssetUtility.NormalizeAssetPath
        );
        if (newFolder != prevFolder) {
            Undo.RecordObject(m_asset,"Change Output Folder");
            m_asset.SetFolder(AssetUtility.NormalizeAssetPath(newFolder));
            GUIUtility.ExitGUI();
        }
        
        TimelineClipSISData timelineClipSISData = m_asset.GetBoundTimelineClipSISData();
        if (null == timelineClipSISData)
            return;
                
        GUILayout.Space(15);
        
        //Capture Selected Frames
        using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
            DrawCaptureSelectedFramesGUI(TimelineEditor.selectedClip, timelineClipSISData);
            DrawLockFramesGUI(TimelineEditor.selectedClip, timelineClipSISData);
        }
        
        GUILayout.Space(15);
        using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
            EditorGUILayout.LabelField("Background Colors");
            ++EditorGUI.indentLevel;

            RenderCachePlayableAssetEditorConfig editorConfig = m_asset.GetEditorConfig();
            Color updateBGColor   = editorConfig.GetUpdateBGColor();
            Color timelineBgColor = m_asset.GetTimelineBGColor();
            
            EditorGUIDrawerUtility.DrawUndoableGUI(m_asset, "Change Update BG Color", updateBGColor,
                /*guiFunc=*/ (Color prevColor)=> {
                    return EditorGUILayout.ColorField("In Game Window (Update)", prevColor);
                }, 
                /*updateFunc=*/ (Color newColor) => { editorConfig.SetUpdateBGColor(newColor); }
            );
            
            EditorGUIDrawerUtility.DrawUndoableGUI(m_asset, "Change Timeline BG Color", timelineBgColor,
                /*guiFunc=*/ (Color prevColor)=> {
                    return EditorGUILayout.ColorField("In Timeline Window", prevColor);
                }, 
                /*updateFunc=*/ (Color newColor) => {  m_asset.SetTimelineBGColor(newColor); }
            );
            
            --EditorGUI.indentLevel;
            GUILayout.Space(5);
        }       
        GUILayout.Space(15);
        DrawUpdateRenderCacheGUI();

    }
    
//----------------------------------------------------------------------------------------------------------------------

    private void DrawUpdateRenderCacheGUI() {
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
                captureEndFrame = TimelineUtility.CalculateNumFrames(TimelineEditor.selectedClip);
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
            
            if (GUILayout.Button($"Update Render Cache ({updateRenderCacheShortcut})")) {            
                
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

        TrackAsset track = renderCachePlayableAsset.GetBoundTimelineClipSISData().GetOwner().GetParentTrack();        
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
                "Invalid output folder",
                "Ok");
            yield break;                                
        }

        //Check if we can capture
        bool canCapture = renderCapturer.CanCapture();
        if (!canCapture) {
            EditorUtility.DisplayDialog("Streaming Image Sequence",
                renderCapturer.GetLastErrorMessage(),
                "Ok");
            yield break;                                            
        }

        //begin capture
        IEnumerator beginCapture = renderCapturer.BeginCapture();
        while (beginCapture.MoveNext()) {
            yield return beginCapture.Current;
        }

        //Show progress in game view
        Texture capturerTex = renderCapturer.GetInternalTexture();
        RenderCachePlayableAssetEditorConfig editorConfig = renderCachePlayableAsset.GetEditorConfig();
        GameObject blitterGO  = CreateBlitter(capturerTex, editorConfig.GetUpdateBGColor()); 

        TimelineClip timelineClip = timelineClipSISData.GetOwner();
        double timePerFrame = 1.0f / track.timelineAsset.editorSettings.fps;
        
        //initial calculation of loop vars
        bool captureAllFrames = editorConfig.GetCaptureAllFrames();
        int  fileCounter      = 0;
        int  numFiles         = (int) Math.Ceiling(timelineClip.duration / timePerFrame) + 1;
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
        List<WatchedFileInfo> imageFiles = new List<WatchedFileInfo>(numFiles);
 
        //Store old files that has the same pattern
        string[] existingFiles = Directory.GetFiles (outputFolder, $"*.png");
        HashSet<string> filesToDelete = new HashSet<string>(existingFiles);
       
        bool cancelled = false;
        while (!cancelled) {            
            
            //Always recalculate from start to avoid floating point errors
            double directorTime = timelineClip.start + (fileCounter * timePerFrame);
            if (directorTime > timelineClip.end)
                break;

            if (!captureAllFrames && fileCounter > editorConfig.GetCaptureEndFrame())
                break;            
            
            string fileName       = $"{prefix}{fileCounter.ToString($"D{numDigits}")}.png";
            string outputFilePath = Path.Combine(outputFolder, fileName);

            SISPlayableFrame playableFrame = timelineClipSISData.GetPlayableFrame(fileCounter);                
            bool captureFrame = (!timelineClipSISData.AreFrameMarkersRequested() //if markers are not requested, capture
                || !File.Exists(outputFilePath) //if file doesn't exist, capture
                || (null!=playableFrame && playableFrame.IsUsed() && !playableFrame.IsLocked())
            );             
            
            if (filesToDelete.Contains(outputFilePath)) {
                filesToDelete.Remove(outputFilePath);
            }
            
           
            if (captureFrame) {
                SetDirectorTime(director, directorTime);
                
                //Need at least two frames in order to wait for the TimelineWindow to be updated ?
                yield return null;
                yield return null;
                yield return null;
                
                //Unload texture because it may be overwritten
                StreamingImageSequencePlugin.UnloadImageAndNotify(outputFilePath);
                renderCapturer.CaptureToFile(outputFilePath);
                
            } 
            Assert.IsTrue(File.Exists(outputFilePath));
            FileInfo fileInfo = new FileInfo(outputFilePath);
            
            imageFiles.Add(new WatchedFileInfo(fileName, fileInfo.Length));

            ++fileCounter;        
            cancelled = EditorUtility.DisplayCancelableProgressBar(
                "StreamingImageSequence", "Caching render results", ((float)(fileCounter - captureStartFrame) / numFiles));
        }

        if (!cancelled) {
            renderCachePlayableAsset.SetImageFiles(imageFiles);        

            //Delete old files
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
        
        //Notify
        FolderContentsChangedNotifier.GetInstance().Notify(outputFolder);        
               
        //Cleanup
        EditorUtility.ClearProgressBar();
        renderCapturer.EndCapture();
        ObjectUtility.Destroy(blitterGO);
        
        AssetDatabase.Refresh();
        
        yield return null;

    }

    
//----------------------------------------------------------------------------------------------------------------------

    private static GameObject CreateBlitter(Texture texToBlit, Color bgColor) {
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

        //Setup blitMaterial
        Shader blitShader = AssetDatabase.LoadAssetAtPath<Shader>(SISEditorConstants.TRANSPARENT_BG_COLOR_SHADER_PATH);            
        Material blitMaterial = new Material(blitShader);
        blitMaterial.hideFlags = HideFlags.DontSaveInBuild | HideFlags.DontSaveInEditor;
        blitMaterial.SetColor(m_bgColorProperty, bgColor);
        blitter.SetBlitMaterial(blitMaterial);
        
        return blitterGO;
    } 
    
    
//----------------------------------------------------------------------------------------------------------------------


    private void DrawCaptureSelectedFramesGUI(TimelineClip timelineClip, TimelineClipSISData timelineClipSISData) {
        TrackAsset   track              = timelineClip.GetParentTrack();
        
        GUILayout.BeginHorizontal();
        bool markerVisibility = InspectorUtility.DrawFrameMarkersGUI(m_asset);
        
        GUILayout.FlexibleSpace();
        EditorGUI.BeginDisabledGroup(!markerVisibility);        
        if (GUILayout.Button("Capture All", GUILayout.Width(80))) {
            Undo.RegisterCompleteObjectUndo(track, "Capturing all frames");
            timelineClipSISData.SetAllPlayableFramesProperty(PlayableFramePropertyID.USED, true);
        }
        if (GUILayout.Button("Reset", GUILayout.Width(50))) {
            Undo.RegisterCompleteObjectUndo(track, "Capturing no frames");
            timelineClipSISData.SetAllPlayableFramesProperty(PlayableFramePropertyID.USED, false);            
        }
        EditorGUI.EndDisabledGroup();
        GUILayout.EndHorizontal();
        
    }

//----------------------------------------------------------------------------------------------------------------------


    private void DrawLockFramesGUI(TimelineClip timelineClip, TimelineClipSISData timelineClipSISData) {
        TrackAsset track = timelineClip.GetParentTrack();
        
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
            if (GUILayout.Button("Lock All", GUILayout.Width(80))) {
                Undo.RegisterCompleteObjectUndo(track, "Locking all frames");
                timelineClipSISData.SetAllPlayableFramesProperty(PlayableFramePropertyID.LOCKED, true);
            }
            if (GUILayout.Button("Reset", GUILayout.Width(50))) {
                Undo.RegisterCompleteObjectUndo(track, "Locking no frames");
                timelineClipSISData.SetAllPlayableFramesProperty(PlayableFramePropertyID.LOCKED, false);
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
        string folder = PathUtility.GenerateUniqueFolder(baseFolder); 
        m_asset.SetFolder(AssetUtility.NormalizeAssetPath(folder).Replace('\\','/'));
        Repaint();        
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
        TimelineEditor.Refresh(RefreshReason.SceneNeedsUpdate); 
    }        

//----------------------------------------------------------------------------------------------------------------------

    
    private                 RenderCachePlayableAsset m_asset                      = null;
    private static          bool                     m_lockMode                   = false;
    private static          TimelineClipSISData      m_inspectedSISDataForLocking = null;
    private static readonly int                      m_bgColorProperty            = Shader.PropertyToID("_BGColor");
}

}