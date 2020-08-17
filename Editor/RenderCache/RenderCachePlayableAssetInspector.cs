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

        //Check if the asset is actually inspected
        if (TimelineEditor.selectedClip.asset != m_asset) {
            return;
        }

        ValidateAssetFolder();

        string prevFolder = m_asset.GetFolder();
        
        string newFolder = InspectorUtility.ShowFolderSelectorGUI("Cache Output Folder", "Select Folder", 
            prevFolder,
            AssetEditorUtility.NormalizeAssetPath
        );
        if (newFolder != prevFolder) {
            m_asset.SetFolder(newFolder);
            GUIUtility.ExitGUI();
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
            EditorUtility.DisplayDialog("Streaming Image Sequence",
                "Invalid output folder",
                "Ok");
            yield break;                                
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
        List<string> imageFileNames = new List<string>(numFiles);
 
        //Store old files that has the same pattern
        string[] existingFiles = Directory.GetFiles (outputFolder, $"{prefix}*.png");
        HashSet<string> filesToDelete = new HashSet<string>(existingFiles);
        
        bool cancelled = false;
        while (nextDirectorTime <= timelineClip.end && !cancelled) {
            
            string fileName       = $"{prefix}{fileCounter.ToString($"D{numDigits}")}.png";
            string outputFilePath = Path.Combine(outputFolder, fileName);

            SISPlayableFrame playableFrame = timelineClipSISData.GetPlayableFrame(fileCounter);                
            bool captureFrame = (!timelineClipSISData.AreFrameMarkersVisible() //if markers are not visible, capture
                || !File.Exists(outputFilePath) //if file doesn't exist, capture
                || (null!=playableFrame && playableFrame.IsUsed() && !playableFrame.IsLocked())
            );             
            
            if (filesToDelete.Contains(outputFilePath)) {
                filesToDelete.Remove(outputFilePath);
            }
            imageFileNames.Add(fileName);
            
            if (captureFrame) {
                SetDirectorTime(director, nextDirectorTime);
                
                //Need at least two frames in order to wait for the TimelineWindow to be updated ?
                yield return null;
                yield return null;
                yield return null;
                
                //Unload texture because it may be overwritten
                StreamingImageSequencePlugin.UnloadImageAndNotify(outputFilePath);
                renderCapturer.CaptureToFile(outputFilePath);
                
            } 


            nextDirectorTime += timePerFrame;
            ++fileCounter;
        
            cancelled = EditorUtility.DisplayCancelableProgressBar(
                "StreamingImageSequence", "Caching render results", ((float)fileCounter / numFiles));
        }

        if (!cancelled) {
            renderCachePlayableAsset.SetImageFileNames(imageFileNames);        

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
        bool markerVisibility = EditorGUILayout.Toggle("Show Frame Markers", prevMarkerVisibility);
        if (markerVisibility != prevMarkerVisibility) {
            timelineClipSISData.ShowFrameMarkers(markerVisibility);
        }
        GUILayout.FlexibleSpace();
        EditorGUI.BeginDisabledGroup(!markerVisibility);        
        if (GUILayout.Button("Capture All", GUILayout.Width(80))) {
            Undo.RegisterCompleteObjectUndo(track, "RenderCachePlayableAsset: Capturing all frames");
            timelineClipSISData.SetAllPlayableFramesProperty(PlayableFramePropertyID.USED, true);
        }
        if (GUILayout.Button("Reset", GUILayout.Width(50))) {
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
            if (GUILayout.Button("Lock All", GUILayout.Width(80))) {
                Undo.RegisterCompleteObjectUndo(track, "RenderCachePlayableAsset: Locking all frames");
                timelineClipSISData.SetAllPlayableFramesProperty(PlayableFramePropertyID.LOCKED, true);
            }
            if (GUILayout.Button("Reset", GUILayout.Width(50))) {
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

        string assetName = string.IsNullOrEmpty(m_asset.name) ? "RenderCachePlayableAsset" : m_asset.name;
        
        //Generate unique folder
        string baseFolder = Path.Combine(Application.streamingAssetsPath, assetName);
        folder = baseFolder;
        int index = 1;
        while (Directory.Exists(folder)) {
            folder = baseFolder + index.ToString();
            ++index;
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

    
    private RenderCachePlayableAsset m_asset = null;
    private static bool m_lockMode = false;
    private static TimelineClipSISData m_inspectedSISDataForLocking = null;

}

}