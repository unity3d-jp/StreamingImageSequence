using JetBrains.Annotations;
using UnityEditor.Timeline;
using Unity.StreamingImageSequence;
using UnityEngine.Timeline;

namespace Unity.StreamingImageSequence.Editor {

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
