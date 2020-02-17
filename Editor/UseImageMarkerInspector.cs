﻿using UnityEditor;
using UnityObject = UnityEngine.Object;


namespace UnityEngine.StreamingImageSequence {

[CustomEditor(typeof(UseImageMarker), true)]
[CanEditMultipleObjects]
public class UseImageMarkerInspector: Editor {

    void OnEnable() {
        m_asset = target as UseImageMarker;
    }

//----------------------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();
        m_asset.SetImageUsed(EditorGUILayout.Toggle("Show Image", m_asset.IsImageUsed()));
    }

//----------------------------------------------------------------------------------------------------------------------

    UseImageMarker m_asset = null;

}

} //end namespace

