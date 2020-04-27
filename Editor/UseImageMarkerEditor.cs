using UnityEditor.Timeline;
using UnityEngine.StreamingImageSequence;
using UnityEngine.Timeline;

namespace UnityEditor.StreamingImageSequence {

[CustomTimelineEditor(typeof(UseImageMarker))]
class UseImageMarkerEditor : MarkerEditor {

//----------------------------------------------------------------------------------------------------------------------    
    public override void DrawOverlay(IMarker m, MarkerUIStates uiState, MarkerOverlayRegion region)
    {
        UseImageMarker marker = m as UseImageMarker;
        if (null == marker)
            return;
            
        if (marker.IsImageUsed()) {
            UnityEngine.Graphics.DrawTexture(region.markerRegion, EditorTextures.GetCheckedTexture());
        }
        
    }
}

} //end namespace