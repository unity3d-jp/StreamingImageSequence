using UnityEditor.ShortcutManagement;
using UnityEngine;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence {

internal static class Shortcuts  {

    [Shortcut("StreamingImageSequence/Toggle Use Image Marker", null,  KeyCode.U)]
    static void ToggleUseImageMarker(ShortcutArguments args) {
        foreach (Object obj in Selection.objects) {
            UseImageMarker marker = obj as UseImageMarker;
            if (null == marker) {
                continue;
            }
            
            marker.SetImageUsed(!marker.IsImageUsed());

        }
        
    }    
    
}

} //end namespace