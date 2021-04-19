using System;
using Unity.FilmInternalUtilities;
using Unity.FilmInternalUtilities.Editor;
using UnityEngine;
using UnityEditor;

namespace Unity.StreamingImageSequence.Editor {

internal static class InspectorUtility {    
    internal static bool DrawFrameMarkersGUI<T>(BaseExtendedClipPlayableAsset<T> clipDataPlayableAsset) where T: PlayableFrameClipData 
    {        

        T clipData = clipDataPlayableAsset.GetBoundClipData();
        if (null == clipData)
            return false;

        bool showFrameMarkers = EditorGUIDrawerUtility.DrawUndoableGUI(
            clipDataPlayableAsset, "Show Frame Markers",
            /*guiFunc=*/ () => EditorGUILayout.Toggle("Show Frame Markers", clipData.AreFrameMarkersRequested()), 
            /*updateFunc=*/ (bool newValue) => { clipData.RequestFrameMarkers(newValue); }                
        );

        return showFrameMarkers;
            
    }

}

} //end namespace

