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
        bool prevUseFrame= m_assets[0].IsFrameUsed();
        bool useFrame = EditorGUILayout.Toggle("Use Frame", prevUseFrame);
        if (useFrame == prevUseFrame)
            return;

        //Set all selected objects
        foreach (FrameMarker m in m_assets) {
            SetMarkerValueByContext(m,useFrame);
        }
    }

//----------------------------------------------------------------------------------------------------------------------
    private static void SetMarkerValueByContext(FrameMarker frameMarker, bool value) {
        SISPlayableFrame    playableFrame       = frameMarker.GetOwner();
        TimelineClipSISData timelineClipSISData = playableFrame.GetOwner();
        PlayableFramePropertyID inspectedPropertyID = timelineClipSISData.GetInspectedProperty();
        switch (inspectedPropertyID) {
            case PlayableFramePropertyID.USED: {
                playableFrame.SetUsed(value);
                break;
            }
            case PlayableFramePropertyID.LOCKED: {
                playableFrame.SetLocked(value);
                break;
            }
            
        }
               
    }

    internal static void ToggleMarkerValueByContext(FrameMarker frameMarker) {
        SISPlayableFrame    playableFrame         = frameMarker.GetOwner();
        TimelineClipSISData timelineClipSISData   = playableFrame.GetOwner();
        PlayableFramePropertyID inspectedPropertyID = timelineClipSISData.GetInspectedProperty();
        switch (inspectedPropertyID) {
            case PlayableFramePropertyID.USED: {
                playableFrame.SetUsed(!playableFrame.IsUsed());
                break;
            }
            case PlayableFramePropertyID.LOCKED: {
                playableFrame.SetLocked(!playableFrame.IsLocked());
                break;
            }

        }
    }
//----------------------------------------------------------------------------------------------------------------------

    private FrameMarker[] m_assets = null;
}

} //end namespace

