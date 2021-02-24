using System;
using Unity.FilmInternalUtilities;
using Unity.FilmInternalUtilities.Editor;
using UnityEngine;
using UnityEditor;

namespace Unity.StreamingImageSequence.Editor {

internal static class InspectorUtility {    
    internal static bool DrawFrameMarkersGUI(BaseExtendedClipPlayableAsset<PlayableFrameClipData> sisClipDataPlayableAsset) 
    {        

        PlayableFrameClipData sisClipData = sisClipDataPlayableAsset.GetBoundClipData();
        if (null == sisClipData)
            return false;

        bool prevMarkerVisibility = sisClipData.AreFrameMarkersRequested();        
        bool showFrameMarkers = EditorGUIDrawerUtility.DrawUndoableGUI(
            sisClipDataPlayableAsset, "Show Frame Markers",prevMarkerVisibility,
            /*guiFunc=*/ (bool prevValue)=>{ return EditorGUILayout.Toggle("Show Frame Markers", prevValue); }, 
            /*updateFunc=*/ (bool newValue) => { sisClipData.RequestFrameMarkers(newValue); }                
        );

        return showFrameMarkers;
            
    }

}

} //end namespace

