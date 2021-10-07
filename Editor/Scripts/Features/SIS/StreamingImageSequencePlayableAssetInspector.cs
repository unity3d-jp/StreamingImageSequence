using Unity.FilmInternalUtilities;
using Unity.FilmInternalUtilities.Editor;
using UnityEditor.Timeline;
using UnityEditorInternal;
using UnityEngine;
using UnityEditor;
using UnityEngine.Timeline;

namespace Unity.StreamingImageSequence.Editor {

/// <summary>
/// The inspector of StreamingImageSequencePlayableAsset
/// </summary>
[CustomEditor(typeof(StreamingImageSequencePlayableAsset))]
internal class StreamingImageSequencePlayableAssetInspector : UnityEditor.Editor {


//----------------------------------------------------------------------------------------------------------------------
    void OnEnable() {
        m_isImageListDirty = true;
        m_asset = target as StreamingImageSequencePlayableAsset;

    }

    
//----------------------------------------------------------------------------------------------------------------------

    void OnDisable() {
        m_asset = null;
    }

//----------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// StreamingImageSequencePlayableAsset GUI Drawing
    /// </summary>
    public override void OnInspectorGUI() {
        if (null == m_asset)
            return;
        

        using (new EditorGUILayout.VerticalScope (GUI.skin.box))  {

            m_resolutionFoldout = EditorGUILayout.Foldout(m_resolutionFoldout, "Resolution");
            if (m_resolutionFoldout) {
                ImageDimensionInt res = m_asset.GetResolution();
                EditorGUILayout.LabelField("Width",  $"{res.Width } px");
                EditorGUILayout.LabelField("Height",  $"{res.Height } px");
            }
            GUILayout.Space(4f);
        }
        
        GUILayout.Space(4f);

        using (new EditorGUILayout.VerticalScope(GUI.skin.box))
        {
            GUILayout.Label("Folder", "BoldLabel");
            GUILayout.Space(4f);
            DrawFolderGUI();
        }
        GUILayout.Space(4f);

        using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
            int numImages = m_asset.GetNumImages();

            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Images: " + numImages, "BoldLabel");            
            if (GUILayout.Button("Reload", GUILayout.Width(50))) {
                m_asset.Reload();
            }
            EditorGUILayout.EndHorizontal();
            
            using (new EditorGUI.DisabledScope(0 == numImages)) {
                if (0 == numImages)
                    EditorGUILayout.IntField("FPS", 0);
                else {
                    TimelineClip clip = m_asset.GetBoundClipData()?.GetOwner();
                    //There is no assigned clip if the playableAsset is not loaded in TimelineWindow
                    if (null != clip) {                         
                                                
                        EditorGUIDrawerUtility.DrawUndoableGUI(clip.GetParentTrack(), "Change FPS", 
                            /*guiFunc=*/ ()=> {
                                float fps = SISPlayableAssetUtility.CalculateFPS(m_asset);
                                float val = EditorGUILayout.DelayedFloatField("FPS", fps); 
                                return Mathf.Max(0.1f, val);
                            }, 
                            /*updateFunc=*/ (float newFPS) => {
                                SISPlayableAssetEditorUtility.SetFPS(m_asset, newFPS);                                
                            }
                        );
                    }                    
                }
            }
            
            GUILayout.Space(4f);
            m_imageListFoldout = EditorGUILayout.Foldout(m_imageListFoldout, "Images");
            if (m_imageListFoldout) {
                DoImageGUI();
            }
        }

        if (null == TimelineEditor.selectedClip) 
            return;
        
        GUILayout.Space(15);
        //Frame markers
        if (TimelineEditor.selectedClip.asset == m_asset) {
            using (new EditorGUILayout.HorizontalScope()) {
                InspectorUtility.DrawFrameMarkersGUI(m_asset);
                if (GUILayout.Button("Reset", GUILayout.Width(50f))) {
                    m_asset.ResetPlayableFrames();
                }
            }
        }
        GUILayout.Space(15);
        
        using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
            EditorGUILayout.LabelField("Background Colors");
            ++EditorGUI.indentLevel;
            
            EditorGUIDrawerUtility.DrawUndoableGUI(m_asset, "Change BG Color", 
                /*guiFunc=*/ ()=> EditorGUILayout.ColorField("In Timeline Window", m_asset.GetTimelineBGColor()), 
                /*updateFunc=*/ (Color newColor) => {                               
                    m_asset.SetTimelineBGColor(newColor);                                
                }
            );
            
            --EditorGUI.indentLevel;
            GUILayout.Space(15);
        }

        
        if (GUILayout.Button("Reset Curve (Not Undoable)")) {
            //AnimationUtility.SetEditorCurve(), which is called below, doesn't seem to be undoable
            EditorCurveBinding curveBinding = StreamingImageSequencePlayableAsset.GetTimeCurveBinding();                 
            ExtendedClipEditorUtility.ResetClipDataCurve(m_asset, curveBinding);
        }
    }

//----------------------------------------------------------------------------------------------------------------------

    private void DrawFolderGUI() {
        string prevFolder = m_asset.GetFolder();
        string newLoadPath = EditorGUIDrawerUtility.DrawFolderSelectorGUI("Image Sequence", "Select Folder", 
            prevFolder,
            ReloadFolder            
        );
        newLoadPath = AssetUtility.NormalizeAssetPath(newLoadPath);
        if (string.IsNullOrEmpty(newLoadPath))
            return;
        
        if (newLoadPath != prevFolder) {
            Undo.RecordObject(m_asset, "Change Image Sequence Folder");            
            ImportImages(newLoadPath);
            GUIUtility.ExitGUI();
        }

    }

    private void ReloadFolder() {
        m_asset.Reload();
    }

//----------------------------------------------------------------------------------------------------------------------
    private void DoImageGUI()
    {
        if (m_isImageListDirty)
        {
            RefreshImageList();
            m_isImageListDirty = false;
        }
        
        m_imageList.DoLayoutList();
    }

//----------------------------------------------------------------------------------------------------------------------
    private void RefreshImageList()
    {
        m_imageList = new ReorderableList(m_asset.GetImageFileNamesNonGeneric(), typeof(WatchedFileInfo), true, false, false, false) {
            elementHeight = EditorGUIUtility.singleLineHeight + 8f,
            headerHeight = 3
        };
    }

//----------------------------------------------------------------------------------------------------------------------
    private void ImportImages(string path) {
        ImageSequenceImporter.ImportImages(path, m_asset);
        m_isImageListDirty = true;
    }

//----------------------------------------------------------------------------------------------------------------------
    private StreamingImageSequencePlayableAsset m_asset = null;

    private ReorderableList m_imageList;
    private bool m_isImageListDirty;

    
    private static bool m_resolutionFoldout = true;
    private static bool m_imageListFoldout;
}

} //end namespace