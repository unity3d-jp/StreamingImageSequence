using Unity.EditorCoroutines.Editor;
using UnityEditor.ShortcutManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.StreamingImageSequence;
using UnityEngine.Timeline;

namespace UnityEditor.StreamingImageSequence {

internal static class Shortcuts  {

    [Shortcut("StreamingImageSequence/Toggle Frame Marker", null,  KeyCode.U)]
    static void ToggleFrameMarker(ShortcutArguments args) {
        foreach (Object obj in Selection.objects) {
            FrameMarker marker = obj as FrameMarker;
            if (null == marker) {
                continue;
            }
            
            marker.SetFrameUsed(!marker.IsFrameUsed());

        }
        
    }    

    [Shortcut("StreamingImageSequence/Update Render Cache", null,  KeyCode.C, ShortcutModifiers.Alt)]
    static void UpdateRenderCache(ShortcutArguments args) {

        TimelineClip clip = TimelineEditor.selectedClip;
        if (null == clip)
            return;

        RenderCachePlayableAsset renderCachePlayableAsset = clip.asset as RenderCachePlayableAsset;
        if (null == renderCachePlayableAsset)
            return;

        //Loop time             
        EditorCoroutineUtility.StartCoroutineOwnerless(RenderCachePlayableAssetInspector.UpdateRenderCacheCoroutine(TimelineEditor.inspectedDirector, renderCachePlayableAsset));

    }    
    
}

} //end namespace