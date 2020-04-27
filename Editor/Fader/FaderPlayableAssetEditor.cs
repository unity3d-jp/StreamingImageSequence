using JetBrains.Annotations;
using UnityEditor.Timeline;
using UnityEngine.StreamingImageSequence;
using UnityEngine.Timeline;

namespace UnityEditor.StreamingImageSequence {

    [CustomTimelineEditor(typeof(FaderPlayableAsset)), UsedImplicitly]
    internal class FaderPlayableAssetEditor : ClipEditor {
        /// <inheritdoc/>
        public override ClipDrawOptions GetClipOptions(TimelineClip clip) {
            ClipDrawOptions clipOptions = base.GetClipOptions(clip);
            FaderPlayableAsset asset = clip.asset as FaderPlayableAsset;
            if (null == asset)
                return clipOptions;

            clipOptions.highlightColor = asset.GetColor();            
            return clipOptions;
        }

    }
}
