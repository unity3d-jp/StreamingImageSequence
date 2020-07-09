using System;
using UnityEditor.Timeline;
using UnityEditorInternal;
using UnityEngine;
using UnityEngine.StreamingImageSequence;
using UnityEngine.Timeline;

namespace UnityEditor.StreamingImageSequence {

    /// <summary>
    /// The inspector of StreamingImageSequencePlayableAsset
    /// </summary>
    [CustomEditor(typeof(StreamingImageSequencePlayableAsset))]
    public class StreamingImageSequencePlayableAssetInspector : Editor {

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
                EditorGUILayout.LabelField("Version",  $"{m_asset.GetVersion() }", "BoldLabel");

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
                    numImages = m_asset.GetImagePaths().Count;
                }
                GUILayout.Label("Images: " + numImages, "BoldLabel");
                GUILayout.Space(4f);
                m_imageListFoldout = EditorGUILayout.Foldout(m_imageListFoldout, "Images");
                if (m_imageListFoldout) {
                    DoImageGUI();
                }
            }
            
            if (null!= TimelineEditor.selectedClip) {
                
                if (GUILayout.Button("Reset Curve")) {
                    //[TODO-sin:2020-1-29] Support undo for this
                    ResetTimelineCurve(TimelineEditor.selectedClip);                
                }

                //[TODO-sin: 2020-6-29] Fix this so that the playable frames are stored inside the timelineClip
                if (TimelineEditor.selectedClip == m_asset.GetBoundTimelineClip()) {
                    GUILayout.Space(15);
                    m_asset.SetUseImageMarkerVisibility(GUILayout.Toggle(m_asset.GetUseImageMarkerVisibility(), "Show UseImageMarkers"));
                    if (GUILayout.Button("Reset UseImageMarkers")) {
                        //[TODO-sin:2020-6-29] Support undo for this
                        m_asset.ResetPlayableFrames();
                    }
                }
                
            }

            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();

        }

//----------------------------------------------------------------------------------------------------------------------
        private static void ResetTimelineCurve(TimelineClip clip) {
            AnimationCurve animationCurve = new AnimationCurve();
            StreamingImageSequencePlayableAsset.ValidateAnimationCurve(ref animationCurve, (float) clip.duration);
            StreamingImageSequencePlayableAsset.RefreshTimelineClipCurve(clip, animationCurve);
            clip.clipIn    = 0;
            clip.timeScale = 1.0;
        }

//----------------------------------------------------------------------------------------------------------------------

        private void DoFolderGUI() {
            string prevFolder = m_asset.GetFolder();
            string newLoadPath = DrawFolderSelector ("Image Sequence", "Select Folder", 
                prevFolder,
                prevFolder,
                AssetEditorUtility.NormalizeAssetPath
            );

            if (newLoadPath != prevFolder) {
                ImportImages(newLoadPath);
                GUIUtility.ExitGUI();
            }

            GUILayout.Space(10f);

            using (new EditorGUILayout.HorizontalScope()) {
                UnityEditor.DefaultAsset timelineDefaultAsset = m_asset.GetTimelineDefaultAsset();
                using(new EditorGUI.DisabledScope(timelineDefaultAsset == null)) 
                {
                    GUILayout.FlexibleSpace();
                    if(GUILayout.Button("Highlight in Project Window", GUILayout.Width(180f))) {
                        EditorGUIUtility.PingObject(timelineDefaultAsset);
                    }
                }
            }
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
                
                //Check drag and drop
                Event evt = Event.current;
                switch (evt.type) {
                    case EventType.DragUpdated:
                    case EventType.DragPerform:
                        if (!folderRect.Contains (evt.mousePosition))
                            return fieldValue;
             
                        DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
                        if (evt.type == EventType.DragPerform) {
                            DragAndDrop.AcceptDrag ();
            
                            if (DragAndDrop.paths.Length <= 0)
                                break;
                            ImportImages(DragAndDrop.paths[0]);
                        }
                        break;
                    default:
                        break;
                }

                if(GUILayout.Button("Select", GUILayout.Width(50f))) {
                    string folderSelected = EditorUtility.OpenFolderPanel(dialogTitle, directoryOpenPath, "");
                    if(!string.IsNullOrEmpty(folderSelected)) {
                        if (onValidFolderSelected != null) {
                            newDirPath = onValidFolderSelected (folderSelected);
                        } else {
                            newDirPath = folderSelected;
                        }
                    } else {
                        GUIUtility.ExitGUI();
                    }
                }
            }
            return newDirPath;
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
            m_imageList = new ReorderableList(m_asset.GetImagePathsNonGeneric(), typeof(string), true, false, false, false) {
                elementHeight = EditorGUIUtility.singleLineHeight + 8f,
                headerHeight = 3
            };
        }

//----------------------------------------------------------------------------------------------------------------------
        private void ImportImages(string path) {
            ImageSequenceImporter.ImportPictureFiles(ImageFileImporterParam.Mode.StreamingAssets, path, m_asset);
            m_isImageListDirty = true;
        }

//----------------------------------------------------------------------------------------------------------------------
        private StreamingImageSequencePlayableAsset m_asset = null;

        private ReorderableList m_imageList;
        private bool m_isImageListDirty;
        private bool m_imageListFoldout;
        private bool m_resolutionFoldout = true;

    }
}