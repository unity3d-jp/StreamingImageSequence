using Unity.EditorCoroutines.Editor;
using UnityEditor.ShortcutManagement;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.StreamingImageSequence;
using UnityEngine.Timeline;

namespace UnityEditor.StreamingImageSequence {

internal static class Shortcuts  {

    [Shortcut(SISEditorConstants.SHORTCUT_TOGGLE_FRAME_MARKER, null,  KeyCode.U)]
    static void ToggleFrameMarker(ShortcutArguments args) {
        foreach (Object obj in Selection.objects) {
            FrameMarker marker = obj as FrameMarker;
            if (null == marker) {
                continue;
            }
            FrameMarkerInspector.ToggleMarkerValueByContext(marker);

        }        
    }    
    
//----------------------------------------------------------------------------------------------------------------------    
    [Shortcut(SISEditorConstants.SHORTCUT_LOCK_AND_EDIT_FRAME, null,  KeyCode.E, ShortcutModifiers.Alt)]
    static void LockAndEditFrame(ShortcutArguments args) {
        
        foreach (Object obj in Selection.objects) {
            FrameMarker frameMarker = obj as FrameMarker;
            if (null == frameMarker) {
                continue;
            }
            SISPlayableFrame playableFrame       = frameMarker.GetOwner();
            RenderCachePlayableAsset playableAsset = playableFrame.GetTimelineClipAsset<RenderCachePlayableAsset>();            
            if (null == playableAsset)
                return;
        
            FrameMarkerInspector.LockAndEditPlayableFrame(playableFrame, playableAsset);

        }        
                        
    }    

//----------------------------------------------------------------------------------------------------------------------    
    
    [Shortcut(SISEditorConstants.SHORTCUT_UPDATE_RENDER_CACHE, null,  KeyCode.C, ShortcutModifiers.Alt)]
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