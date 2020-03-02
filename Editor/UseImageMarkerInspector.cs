using UnityEditor;
using UnityObject = UnityEngine.Object;


namespace UnityEngine.StreamingImageSequence {

[CustomEditor(typeof(UseImageMarker), true)]
[CanEditMultipleObjects]
internal class UseImageMarkerInspector: Editor {

    void OnEnable() {
        int numTargets = targets.Length;
        m_assets = new UseImageMarker[numTargets];
        for (int i = 0; i < numTargets; i++) {
            m_assets[i] = targets[i] as UseImageMarker;
        }
    }

//----------------------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();
        bool prevUseImage= m_assets[0].IsImageUsed();
        bool useImage = EditorGUILayout.Toggle("Use Image", prevUseImage);
        if (useImage == prevUseImage)
            return;

        //Set all selected objects
        foreach (UseImageMarker m in m_assets) {
            m.SetImageUsed(useImage);
        }

    }

//----------------------------------------------------------------------------------------------------------------------

    private UseImageMarker[] m_assets = null;
}

} //end namespace

