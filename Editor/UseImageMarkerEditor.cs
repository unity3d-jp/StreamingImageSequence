using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {

[CustomTimelineEditor(typeof(UseImageMarker))]
class UseImageMarkeEditor : MarkerEditor {
    private const string IMAGE_NOT_USED = "Image is not used";

    public override MarkerDrawOptions GetMarkerOptions(IMarker m) {
        MarkerDrawOptions options = base.GetMarkerOptions(m);
        UseImageMarker marker = m as UseImageMarker;
        if (!marker)
            return options;

        if (!marker.IsImageUsed()) {
            options.errorText = IMAGE_NOT_USED;
        }

        return options;
    }
}

} //end namespace