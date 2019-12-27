using JetBrains.Annotations;
using System.IO;
using UnityEditor;
using UnityEditor.StreamingImageSequence;
using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {
    [CustomTimelineEditor(typeof(StreamingImageSequencePlayableAsset)), UsedImplicitly]
    internal class MovieProxyPlayableAssetEditor : ClipEditor
    {
        private const string kNoFolderAssignedError = "No Folder assigned";
        private const string kNotStreamingAssetsFolderAssignedError = "Loading folder must be under Assets/StreamingAssets";
        private const string kFolderMissingError = "Assigned folder does not exist.";
        private const string kNoPicturesAssignedError = "No Pictures assigned";
//        readonly Dictionary<TimelineClip, WaveformPreview> m_PersistentPreviews = new Dictionary<TimelineClip, WaveformPreview>();
//        private ColorSpace m_ColorSpace = ColorSpace.Uninitialized;

        /// <inheritdoc/>
        public override ClipDrawOptions GetClipOptions(TimelineClip clip)
        {
            var clipOptions = base.GetClipOptions(clip);
            StreamingImageSequencePlayableAsset asset = clip.asset as StreamingImageSequencePlayableAsset;
            if (null == asset) {
                Debug.LogError("Asset is not a StreamingImageSequencePlayableAsset: " + clip.asset);
                return clipOptions;
            }

            string folder = asset.GetFolder();
            if (string.IsNullOrEmpty(folder)) {

                UnityEditor.DefaultAsset timelineDefaultAsset = asset.GetTimelineDefaultAsset();
                if (null!=timelineDefaultAsset) {
                    if (!InitializeAssetFromDefaultAsset(clip, asset)) {
                        clipOptions.errorText = kNotStreamingAssetsFolderAssignedError;
                    }
                } else {
                    clipOptions.errorText = kNoFolderAssignedError;
                }
            }  else if (!Directory.Exists(folder)) {
                clipOptions.errorText = kFolderMissingError;
            } else if (asset.GetImagePaths() == null) {
                clipOptions.errorText = kNoPicturesAssignedError;
            }
            clipOptions.tooltip = folder;
            
            return clipOptions;
        }

        /// <inheritdoc/>
        public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
        {
            StreamingImageSequencePlayableAsset asset = clip.asset as StreamingImageSequencePlayableAsset;
            if (null == asset) {
                Debug.LogError("Asset is not a StreamingImageSequencePlayableAsset: " + clip.asset);
                return;
            }

            InitializeAssetFromDefaultAsset(clip, asset);
        }
       
        private static bool InitializeAssetFromDefaultAsset(TimelineClip clip, StreamingImageSequencePlayableAsset asset) 
        {
            UnityEditor.DefaultAsset timelineDefaultAsset = asset.GetTimelineDefaultAsset();
            if (null == timelineDefaultAsset)
                return false;

            string path = AssetDatabase.GetAssetPath(timelineDefaultAsset).Replace("\\","/");
            if (!path.StartsWith("Assets/StreamingAssets/")) {
                return false;
            }


            ImageSequenceImporter.ImportPictureFiles(PictureFileImporterParam.Mode.StreamingAssets, path, asset);
            clip.duration = asset.GetImagePaths().Count * 0.125; // 8fps (standard limited animation)
            clip.displayName = Path.GetFileName(asset.GetFolder());
            return true;
        }

        /// <inheritdoc/>
        public override void OnClipChanged(TimelineClip clip)
        {
            base.OnClipChanged(clip);
        }

        /// <inheritdoc/>
        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region)
        {
            base.DrawBackground(clip, region);
            
//            if (!TimelineWindow.instance.state.showAudioWaveform)
//                return;
//
//            var rect = region.position;
//            if (rect.width <= 0)
//                return;
//
//            var audioClip = clip.asset as AudioClip;
//            if (audioClip == null)
//            {
//                var audioPlayableAsset = clip.asset as AudioPlayableAsset;
//                if (audioPlayableAsset != null)
//                    audioClip = audioPlayableAsset.clip;
//            }
//
//            if (audioClip == null)
//                return;
//
//            var quantizedRect = new Rect(Mathf.Ceil(rect.x), Mathf.Ceil(rect.y), Mathf.Ceil(rect.width), Mathf.Ceil(rect.height));
//            WaveformPreview preview;
//
//            if (QualitySettings.activeColorSpace != m_ColorSpace)
//            {
//                m_ColorSpace = QualitySettings.activeColorSpace;
//                m_PersistentPreviews.Clear();
//            }
//
//            if (!m_PersistentPreviews.TryGetValue(clip, out preview) || audioClip != preview.presentedObject)
//            {
//                preview = m_PersistentPreviews[clip] = WaveformPreviewFactory.Create((int)quantizedRect.width, audioClip);
//                Color waveColour = GammaCorrect(DirectorStyles.Instance.customSkin.colorAudioWaveform);
//                Color transparent = waveColour;
//                transparent.a = 0;
//                preview.backgroundColor = transparent;
//                preview.waveColor = waveColour;
//                preview.SetChannelMode(WaveformPreview.ChannelMode.MonoSum);
//                preview.updated += () => TimelineEditor.Refresh(RefreshReason.WindowNeedsRedraw);
//            }
//
//            preview.looping = clip.SupportsLooping();
//            preview.SetTimeInfo(region.startTime, region.endTime - region.startTime);
//            preview.OptimizeForSize(quantizedRect.size);
//
//            if (Event.current.type == EventType.Repaint)
//            {
//                preview.ApplyModifications();
//                preview.Render(quantizedRect);
//            }
        }

//        private static Color GammaCorrect(Color color)
//        {
//            return (QualitySettings.activeColorSpace == ColorSpace.Linear) ? color.gamma : color;
//        }
    }
}
