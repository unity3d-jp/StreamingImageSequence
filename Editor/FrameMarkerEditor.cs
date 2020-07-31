using UnityEditor.Timeline;
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
            
        if (marker.IsFrameUsed()) {
            UnityEngine.Graphics.DrawTexture(region.markerRegion, EditorTextures.GetCheckedTexture());
        }
        
    }
}

} //end namespace