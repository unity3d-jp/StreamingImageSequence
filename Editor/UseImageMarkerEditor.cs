using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {

[CustomTimelineEditor(typeof(UseImageMarker))]
class UseImageMarkerEditor : MarkerEditor {

//----------------------------------------------------------------------------------------------------------------------    
    public override void DrawOverlay(IMarker m, MarkerUIStates uiState, MarkerOverlayRegion region)
    {
        UseImageMarker marker = m as UseImageMarker;
        if (null == marker)
            return;
            
        if (marker.IsImageUsed()) {
            Graphics.DrawTexture(region.markerRegion, EditorTextures.GetCheckedTexture());
        }
        
    }
}

} //end namespace