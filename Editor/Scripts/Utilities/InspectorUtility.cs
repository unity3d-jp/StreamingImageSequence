using System;
using UnityEngine;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence {

internal static class InspectorUtility {    
    internal static void ShowFrameMarkersGUI(BaseTimelineClipSISDataPlayableAsset timelineClipSISDataPlayableAsset) {        

        TimelineClipSISData timelineClipSISData = timelineClipSISDataPlayableAsset.GetBoundTimelineClipSISData();
        if (null == timelineClipSISData)
            return;
                
        GUILayout.Space(15);
        bool prevMarkerVisibility = timelineClipSISData.AreFrameMarkersVisible();
        bool markerVisibility     = GUILayout.Toggle(prevMarkerVisibility, "Show FrameMarkers");
        if (markerVisibility != prevMarkerVisibility) {
            timelineClipSISData.ShowFrameMarkers(markerVisibility);
        }
            
            
        if (GUILayout.Button("Reset FrameMarkers")) {
            timelineClipSISDataPlayableAsset.ResetPlayableFrames();
        }            
    }

//----------------------------------------------------------------------------------------------------------------------
    
    internal static string ShowSelectFolderButton(string title, string folderPath, Func<string, string> onValidFolderSelected) {
        if(GUILayout.Button("Select", GUILayout.Width(50f))) {
            string folderSelected = EditorUtility.OpenFolderPanel(title, folderPath, "");
            if(!string.IsNullOrEmpty(folderSelected)) {
                string newDirPath = null;                    
                if (onValidFolderSelected != null) {
                    newDirPath = onValidFolderSelected (folderSelected);
                } else {
                    newDirPath = folderSelected;
                }

                return newDirPath;
            } else {
                GUIUtility.ExitGUI(); //prevent error when cancel is pressed                
            }
        }

        return folderPath;
    }
    
}

} //end namespace

