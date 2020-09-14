using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.StreamingImageSequence;
using UnityEngine.Timeline;

namespace UnityEditor.StreamingImageSequence {

[CustomTimelineEditor(typeof(StreamingImageSequenceTrack))]
internal class StreamingImageSequenceTrackEditor : TrackEditor {
    public override TrackDrawOptions GetTrackOptions(TrackAsset track, Object binding) {
        TrackDrawOptions options = base.GetTrackOptions(track, binding);
        options.errorText = null;
        
        return options;
    }
}

}