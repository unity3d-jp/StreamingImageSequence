using System;
using UnityEditor.Timeline;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence {

/// <summary>
/// The inspector of StreamingImageSequencePlayableAsset
/// </summary>
[CustomEditor(typeof(StreamingImageSequencePlayableAsset))]
internal class StreamingImageSequencePlayableAssetInspector : Editor {

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
        
        EditorGUI.BeginChangeCheck();
        serializedObject.Update();
        Undo.RecordObject(m_asset, "StreamingImageSequencePlayableAssetInspector::OnInspectorGUI");

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
            DoFolderGUI();
        }
        GUILayout.Space(4f);

        using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
            int numImages = 0;
            if (m_asset.HasImages()) {
                numImages = m_asset.GetImageFileNames().Count;
            }
            GUILayout.Label("Images: " + numImages, "BoldLabel");
            GUILayout.Space(4f);
            m_imageListFoldout = EditorGUILayout.Foldout(m_imageListFoldout, "Images");
            if (m_imageListFoldout) {
                DoImageGUI();
            }
        }
        
        if (null!= TimelineEditor.selectedClip) {
            
            if (GUILayout.Button("Reset Curve (Not Undoable)")) {
                //AnimationClip.SetCurve() doesn't seem to be undoable
                StreamingImageSequencePlayableAsset.ResetTimelineClipCurve(TimelineEditor.selectedClip);
            }

            //Frame markers
            if (TimelineEditor.selectedClip.asset == m_asset) {
                InspectorUtility.ShowFrameMarkersGUI(m_asset);
            }
            
        }

        serializedObject.ApplyModifiedProperties();
        EditorGUI.EndChangeCheck();

    }

//----------------------------------------------------------------------------------------------------------------------

    private void DoFolderGUI() {
        string prevFolder = m_asset.GetFolder();
        string newLoadPath = InspectorUtility.ShowFolderSelectorGUI("Image Sequence", "Select Folder", 
            prevFolder,
            AssetEditorUtility.NormalizeAssetPath
        );        
        
        if (newLoadPath != prevFolder) {
            ImportImages(newLoadPath);
            GUIUtility.ExitGUI();
        }

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
        m_imageList = new ReorderableList(m_asset.GetImageFileNamesNonGeneric(), typeof(string), true, false, false, false) {
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