using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.StreamingImageSequence;
using UnityEngine.Timeline;

namespace UnityEditor.StreamingImageSequence {

[CustomTimelineEditor(typeof(FrameMarker))]
class FrameMarkerEditor : MarkerEditor {

//----------------------------------------------------------------------------------------------------------------------    
    public override void DrawOverlay(IMarker m, MarkerUIStates uiState, MarkerOverlayRegion region)
    {
        FrameMarker marker = m as FrameMarker;
        if (null == marker)
            return;


        SISPlayableFrame playableFrame = marker.GetOwner();
        TimelineClipSISData timelineClipSISData = playableFrame.GetOwner();
        PlayableFramePropertyID inspectedPropertyID = timelineClipSISData.GetInspectedProperty();
        switch (inspectedPropertyID) {
            case PlayableFramePropertyID.USED: {
                if (playableFrame.IsUsed()) {
                    Graphics.DrawTexture(region.markerRegion, EditorTextures.GetCheckedTexture());
                }
                
                if (playableFrame.IsLocked()) {
                    Rect lockRegion = region.markerRegion;
                    lockRegion.x -= 5;
                    lockRegion.y -= 8;
                    Graphics.DrawTexture(lockRegion, EditorTextures.GetLockTexture());                    
                }
                break;
            }
            case PlayableFramePropertyID.LOCKED: {
                if (playableFrame.IsLocked()) {
                    Graphics.DrawTexture(region.markerRegion, EditorTextures.GetLockTexture());                    
                }
                break;
            }
            
        }
        
    }
}

} //end namespace