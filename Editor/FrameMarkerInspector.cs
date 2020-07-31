using UnityEngine.StreamingImageSequence;
using UnityObject = UnityEngine.Object;


namespace UnityEditor.StreamingImageSequence {

[CustomEditor(typeof(FrameMarker), true)]
[CanEditMultipleObjects]
internal class FrameMarkerInspector: Editor {

    void OnEnable() {
        int numTargets = targets.Length;
        m_assets = new FrameMarker[numTargets];
        for (int i = 0; i < numTargets; i++) {
            m_assets[i] = targets[i] as FrameMarker;
        }
    }

//----------------------------------------------------------------------------------------------------------------------
    public override void OnInspectorGUI() {
        //base.OnInspectorGUI();
        bool prevUseImage= m_assets[0].IsFrameUsed();
        bool useImage = EditorGUILayout.Toggle("Use Image", prevUseImage);
        if (useImage == prevUseImage)
            return;

        //Set all selected objects
        foreach (FrameMarker m in m_assets) {
            m.SetFrameUsed(useImage);
        }

    }

//----------------------------------------------------------------------------------------------------------------------

    private FrameMarker[] m_assets = null;
}

} //end namespace

