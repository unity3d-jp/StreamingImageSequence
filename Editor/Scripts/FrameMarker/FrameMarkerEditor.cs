using UnityEditor.Timeline;
using UnityEngine;
using Unity.StreamingImageSequence;
using UnityEngine.Timeline;

namespace Unity.StreamingImageSequence.Editor {

[CustomTimelineEditor(typeof(FrameMarker))]
class FrameMarkerEditor : MarkerEditor {

//----------------------------------------------------------------------------------------------------------------------    
    public override void DrawOverlay(IMarker m, MarkerUIStates uiState, MarkerOverlayRegion region)
    {
        FrameMarker marker = m as FrameMarker;
        if (null == marker)
            return;

        SISPlayableFrame playableFrame = marker.GetOwner();
        //Check invalid PlayableFrame. Perhaps because of unsupported Duplicate operation ?
        if (null == playableFrame)
            return;
        
        SISClipData timelineClipSISData = playableFrame.GetOwner();
        PlayableFramePropertyID inspectedPropertyID = timelineClipSISData.GetInspectedProperty();
        switch (inspectedPropertyID) {
            case PlayableFramePropertyID.USED: {
                
                if (playableFrame.IsLocked()) {
                    //At the moment, all locked frames are regarded as inactive 
                    if (playableFrame.IsUsed()) {
                        Graphics.DrawTexture(region.markerRegion, EditorTextures.GetInactiveCheckedTexture());
                    }
                    Rect lockRegion = region.markerRegion;
                    lockRegion.x -= 5;
                    lockRegion.y -= 8;
                    Graphics.DrawTexture(lockRegion, EditorTextures.GetLockTexture());                    
                } else {
                    if (playableFrame.IsUsed()) {
                        Graphics.DrawTexture(region.markerRegion, EditorTextures.GetCheckedTexture());
                    }
                    
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