using UnityEditor.Timeline;
using UnityEngine;
using Unity.StreamingImageSequence;
using UnityEngine.Timeline;

namespace Unity.StreamingImageSequence.Editor {

[CustomTimelineEditor(typeof(FaderTrack))]
internal class FaderTrackEditor : TrackEditor {
    public override TrackDrawOptions GetTrackOptions(TrackAsset track, Object binding) {
        TrackDrawOptions options = base.GetTrackOptions(track, binding);
        options.errorText = null;
        
        return options;
    }
}

}