using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence {

internal static class Shortcuts  {

    [Shortcut("StreamingImageSequence/Toggle Frame Marker", null,  KeyCode.U)]
    static void ToggleUseImageMarker(ShortcutArguments args) {
        foreach (Object obj in Selection.objects) {
            FrameMarker marker = obj as FrameMarker;
            if (null == marker) {
                continue;
            }
            
            marker.SetFrameUsed(!marker.IsFrameUsed());

        }
        
    }    
    
}

} //end namespace