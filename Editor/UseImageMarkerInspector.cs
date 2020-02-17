using UnityEditor;
using UnityObject = UnityEngine.Object;


namespace UnityEngine.StreamingImageSequence {

[CustomEditor(typeof(UseImageMarker), true)]
[CanEditMultipleObjects]
public class UseImageMarkerInspector: Editor {

    void OnEnable() {
        m_asset = target as UseImageMarker;
        m_useImageProperty = serializedObject.FindProperty("m_info.m_useImage");
        m_useImageContent = new GUIContent("Use Image");
    }

//----------------------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();
        serializedObject.Update ();

        EditorGUILayout.PropertyField(m_useImageProperty, m_useImageContent);
        serializedObject.ApplyModifiedProperties ();
    }

//----------------------------------------------------------------------------------------------------------------------

    private UseImageMarker m_asset = null;
    private GUIContent m_useImageContent = null;
    private SerializedProperty m_useImageProperty; //For multiple object editing
}

} //end namespace

