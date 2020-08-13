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
            SetMarkerValueByContext(m,useImage);
        }
    }

//----------------------------------------------------------------------------------------------------------------------
    private static void SetMarkerValueByContext(FrameMarker frameMarker, bool value) {
        SISPlayableFrame    playableFrame       = frameMarker.GetOwner();
        TimelineClipSISData timelineClipSISData = playableFrame.GetOwner();
        string inspectedPropertyName = timelineClipSISData.GetInspectedPropertyName();
        switch (inspectedPropertyName) {
            case PlayableFramePropertyName.USED: {
                playableFrame.SetUsed(value);
                break;
            }
            case PlayableFramePropertyName.LOCKED: {
                playableFrame.SetLocked(value);
                break;
            }
            
        }
               
    }

    internal static void ToggleMarkerValueByContext(FrameMarker frameMarker) {
        SISPlayableFrame    playableFrame         = frameMarker.GetOwner();
        TimelineClipSISData timelineClipSISData   = playableFrame.GetOwner();
        string              inspectedPropertyName = timelineClipSISData.GetInspectedPropertyName();
        switch (inspectedPropertyName) {
            case PlayableFramePropertyName.USED: {
                playableFrame.SetUsed(!playableFrame.IsUsed());
                break;
            }
            case PlayableFramePropertyName.LOCKED: {
                playableFrame.SetLocked(!playableFrame.IsLocked());
                break;
            }

        }
    }
//----------------------------------------------------------------------------------------------------------------------

    private FrameMarker[] m_assets = null;
}

} //end namespace

