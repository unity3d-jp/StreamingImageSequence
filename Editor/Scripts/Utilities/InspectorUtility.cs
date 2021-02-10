using System;
using Unity.FilmInternalUtilities.Editor;
using UnityEngine;
using UnityEditor;

namespace Unity.StreamingImageSequence.Editor {

internal static class InspectorUtility {    
    internal static bool DrawFrameMarkersGUI(BaseTimelineClipSISDataPlayableAsset timelineClipSISDataPlayableAsset) 
    {        

        TimelineClipSISData timelineClipSISData = timelineClipSISDataPlayableAsset.GetBoundTimelineClipSISData();
        if (null == timelineClipSISData)
            return false;

        bool prevMarkerVisibility = timelineClipSISData.AreFrameMarkersRequested();        
        bool showFrameMarkers = EditorGUIDrawerUtility2.DrawUndoableGUI(
            timelineClipSISDataPlayableAsset, "Show Frame Markers",prevMarkerVisibility,
            /*guiFunc=*/ (bool prevValue)=>{
                return EditorGUILayout.Toggle("Show Frame Markers", prevMarkerVisibility);;                            
            }, 
            /*updateFunc=*/ (bool newValue) => {
                timelineClipSISData.RequestFrameMarkers(newValue);
                            
            }                
        );

        return showFrameMarkers;
            
    }

}

} //end namespace

