using System.IO;
using UnityEngine;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence {

    [CustomEditor(typeof(StreamingImageSequencePlayableAsset))]
    public class StreamingImageSequencePlayableAssetInspector : Editor {

        void OnEnable() {
            m_asset = serializedObject.targetObject as StreamingImageSequencePlayableAsset;
        }
        
//---------------------------------------------------------------------------------------------------------------------

        public override void OnInspectorGUI() {

            DrawFolderField();
            base.OnInspectorGUI();
        }

//---------------------------------------------------------------------------------------------------------------------
        private void DrawFolderField() {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel("Folder");
            //GUI.skin.button.wordWrap = false;
            EditorGUILayout.SelectableLabel(m_asset.GetFolder(),
                EditorStyles.objectField, GUILayout.Height(EditorGUIUtility.singleLineHeight)
            );
            Rect folderRect = GUILayoutUtility.GetLastRect();
            EditorGUILayout.EndHorizontal();

            //Check drag and drop
            Event evt = Event.current;
            switch (evt.type) {
                case EventType.DragUpdated:
                case EventType.DragPerform:
                    if (!folderRect.Contains (evt.mousePosition))
                        return;
             
                    DragAndDrop.visualMode = DragAndDropVisualMode.Copy;
         
                    if (evt.type == EventType.DragPerform) {
                        DragAndDrop.AcceptDrag ();
             
                        if (DragAndDrop.paths.Length <= 0)
                            break;

                        //We only accept one dragged path here
                        string folder = DragAndDrop.paths[0];
                        FileAttributes attr = File.GetAttributes(folder);

                        if (!attr.HasFlag(FileAttributes.Directory)) {
                            folder = Path.GetDirectoryName(folder);
                        }
                        PictureFileImportWindow.Init(PictureFileImporterParam.Mode.StereamingAssets, folder);
                    }
                    break;
                default:
                    break;
            }
        }

//---------------------------------------------------------------------------------------------------------------------

        private StreamingImageSequencePlayableAsset m_asset;

    }
}