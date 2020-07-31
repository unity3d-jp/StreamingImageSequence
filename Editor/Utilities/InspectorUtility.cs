using NUnit.Framework;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence {

internal static class InspectorUtility {    
    internal static void ShowFrameMarkersGUI(BaseTimelineClipSISDataPlayableAsset timelineClipSISDataPlayableAsset) {        

        //Image markers
        if (TimelineEditor.selectedClip.asset == timelineClipSISDataPlayableAsset) {
            TimelineClipSISData timelineClipSISData = timelineClipSISDataPlayableAsset.GetBoundTimelineClipSISData();
            Assert.IsNotNull(timelineClipSISData);
                    
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
            
    }
    
}

} //end namespace

