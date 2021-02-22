using System;
using Unity.FilmInternalUtilities;
using Unity.FilmInternalUtilities.Editor;
using UnityEngine;
using UnityEditor;

namespace Unity.StreamingImageSequence.Editor {

internal static class InspectorUtility {    
    internal static bool DrawFrameMarkersGUI(BaseExtendedClipPlayableAsset<SISClipData> timelineClipSISDataPlayableAsset) 
    {        

        SISClipData timelineClipSISData = timelineClipSISDataPlayableAsset.GetBoundClipData();
        if (null == timelineClipSISData)
            return false;

        bool prevMarkerVisibility = timelineClipSISData.AreFrameMarkersRequested();        
        bool showFrameMarkers = EditorGUIDrawerUtility.DrawUndoableGUI(
            timelineClipSISDataPlayableAsset, "Show Frame Markers",prevMarkerVisibility,
            /*guiFunc=*/ (bool prevValue)=>{ return EditorGUILayout.Toggle("Show Frame Markers", prevValue); }, 
            /*updateFunc=*/ (bool newValue) => { timelineClipSISData.RequestFrameMarkers(newValue); }                
        );

        return showFrameMarkers;
            
    }

}

} //end namespace

