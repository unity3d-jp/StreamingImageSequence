using UnityEngine;
using UnityEditor;

namespace Unity.StreamingImageSequence.Editor {

internal static class InspectorUtility {    
    internal static void ShowFrameMarkersGUI(BaseTimelineClipSISDataPlayableAsset timelineClipSISDataPlayableAsset) {        

        TimelineClipSISData timelineClipSISData = timelineClipSISDataPlayableAsset.GetBoundTimelineClipSISData();
        if (null == timelineClipSISData)
            return;

        using (new EditorGUILayout.VerticalScope(GUI.skin.box)) {
            bool prevMarkerVisibility = timelineClipSISData.AreFrameMarkersRequested();

            EditorGUILayout.BeginHorizontal();
            bool markerVisibility = EditorGUILayout.Toggle("Show Frame Markers", prevMarkerVisibility);
            if (markerVisibility != prevMarkerVisibility) {
                timelineClipSISData.RequestFrameMarkers(markerVisibility);
            }
            if (GUILayout.Button("Reset", GUILayout.Width(50f))) {
                timelineClipSISDataPlayableAsset.ResetPlayableFrames();
            }                    
            EditorGUILayout.EndHorizontal();        
        }
            
    }

}

} //end namespace

