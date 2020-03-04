using JetBrains.Annotations;
using System.IO;
using UnityEditor;
using UnityEditor.StreamingImageSequence;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using System.Collections.Generic;

namespace UnityEngine.StreamingImageSequence {
    [CustomTimelineEditor(typeof(StreamingImageSequencePlayableAsset)), UsedImplicitly]
    internal class StreamingImageSequencePlayableAssetEditor : ClipEditor
    {
        private const string kNoFolderAssignedError = "No Folder assigned";
        private const string kNotStreamingAssetsFolderAssignedError = "Loading folder must be under Assets/StreamingAssets";
        private const string kFolderMissingError = "Assigned folder does not exist.";
        private const string kNoPicturesAssignedError = "No Pictures assigned";

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
                    if (!InitializeAssetFromDefaultAsset(asset, timelineDefaultAsset)) {
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
            Debug.Log("OnCreate");
            StreamingImageSequencePlayableAsset asset = clip.asset as StreamingImageSequencePlayableAsset;
            if (null == asset) {
                Debug.LogError("Asset is not a StreamingImageSequencePlayableAsset: " + clip.asset);
                return;
            }

            //This callback occurs before the clip is assigned to the track, but we need the track for creating curves.
            clip.parentTrack = track; 
            
            //If we have a default asset, and clonedFrom is NULL, which means this is created by user interaction,
            //such as Folder D&D
            UnityEditor.DefaultAsset timelineDefaultAsset = asset.GetTimelineDefaultAsset();
            if (null != timelineDefaultAsset && null == clonedFrom) {
                InitializeAssetFromDefaultAsset(asset, timelineDefaultAsset);
            }

            //If the clip already has curves (because of cloning, etc), then we don't set anything
            if (null == clip.curves) {
                if (asset.HasImages()) {
                    clip.duration = asset.GetImagePaths().Count * 0.125; // 8fps (standard limited animation)
                    clip.displayName = Path.GetFileName(asset.GetFolder());
                }
                clip.CreateCurves("Curves: " + clip.displayName);
            }

            asset.SetTimelineClip(clip);
            asset.ValidateAnimationCurve();

        }

//----------------------------------------------------------------------------------------------------------------------

        private static bool InitializeAssetFromDefaultAsset(StreamingImageSequencePlayableAsset playableAsset,
            UnityEditor.DefaultAsset timelineDefaultAsset) 
        {
            string path = AssetDatabase.GetAssetPath(timelineDefaultAsset).Replace("\\","/");
            if (!path.StartsWith("Assets/StreamingAssets/")) {
                return false;
            }
            ImageSequenceImporter.ImportPictureFiles(ImageFileImporterParam.Mode.StreamingAssets, path, playableAsset);

            return true;
        }

        /// <inheritdoc/>
        public override void OnClipChanged(TimelineClip clip)
        {
            base.OnClipChanged(clip);
        }

//----------------------------------------------------------------------------------------------------------------------

        /// <inheritdoc/>
        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region) {
            base.DrawBackground(clip, region);
            
            var rect = region.position;
            const int VISIBLE_WIDTH_THRESHOLD = 10; //width that is too small will affect the placement of preview imgs
            if (rect.width <= VISIBLE_WIDTH_THRESHOLD)
                return;

            StreamingImageSequencePlayableAsset curAsset = clip.asset as StreamingImageSequencePlayableAsset;
            if (null == curAsset || !curAsset.HasImages())
                return;

            Rect quantizedRect = new Rect(Mathf.Ceil(rect.x), Mathf.Ceil(rect.y), Mathf.Ceil(rect.width), Mathf.Ceil(rect.height));

            if (QualitySettings.activeColorSpace != m_colorSpace) {
                m_colorSpace = QualitySettings.activeColorSpace;
                m_persistentPreviews.Clear();
            }

            if (!m_persistentPreviews.TryGetValue(clip, out StreamingImageSequencePreview preview)) {
                preview = m_persistentPreviews[clip] = new StreamingImageSequencePreview(curAsset);
            }

            if (Event.current.type == EventType.Repaint) {
                preview.SetVisibleLocalTime(region.startTime, region.endTime);
                preview.Render(quantizedRect);
            }
        }

//----------------------------------------------------------------------------------------------------------------------
        readonly Dictionary<TimelineClip, StreamingImageSequencePreview> m_persistentPreviews 
            = new Dictionary<TimelineClip, StreamingImageSequencePreview>();

        ColorSpace m_colorSpace = ColorSpace.Uninitialized;

    }
}
