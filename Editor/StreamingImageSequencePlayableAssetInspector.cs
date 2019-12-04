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
             
                        foreach (string dragged_object in DragAndDrop.paths) {

                            Debug.Log(dragged_object);
                        }
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