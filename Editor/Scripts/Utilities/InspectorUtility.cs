using System;
using Unity.FilmInternalUtilities;
using Unity.FilmInternalUtilities.Editor;
using UnityEngine;
using UnityEditor;

namespace Unity.StreamingImageSequence.Editor {

internal static class InspectorUtility {    
    internal static bool DrawFrameMarkersGUI(BaseExtendedClipPlayableAsset<SISClipData> clipDataPlayableAsset) 
    {        

        SISClipData clipData = clipDataPlayableAsset.GetBoundClipData();
        if (null == clipData)
            return false;

        bool prevMarkerVisibility = clipData.AreFrameMarkersRequested();        
        bool showFrameMarkers = EditorGUIDrawerUtility.DrawUndoableGUI(
            clipDataPlayableAsset, "Show Frame Markers",prevMarkerVisibility,
            /*guiFunc=*/ (bool prevValue)=>{ return EditorGUILayout.Toggle("Show Frame Markers", prevValue); }, 
            /*updateFunc=*/ (bool newValue) => { clipData.RequestFrameMarkers(newValue); }                
        );

        return showFrameMarkers;
            
    }

}

} //end namespace

