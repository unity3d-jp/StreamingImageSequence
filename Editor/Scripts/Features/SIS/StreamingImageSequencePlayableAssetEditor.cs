using JetBrains.Annotations;
using System.IO;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence {
    [CustomTimelineEditor(typeof(StreamingImageSequencePlayableAsset)), UsedImplicitly]
    internal class StreamingImageSequencePlayableAssetEditor : ClipEditor {
        private const string NO_FOLDER_ASSIGNED_ERROR = "No Folder assigned";
        private const string FOLDER_MISSING_ERROR = "Assigned folder does not exist.";
        private const string NO_PICTURES_ASSIGNED_ERROR = "No Pictures assigned";

//----------------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public override ClipDrawOptions GetClipOptions(TimelineClip clip) {
            var clipOptions = base.GetClipOptions(clip);
            StreamingImageSequencePlayableAsset asset = clip.asset as StreamingImageSequencePlayableAsset;
            if (null == asset) {
                Debug.LogError("Asset is not a StreamingImageSequencePlayableAsset: " + clip.asset);
                return clipOptions;
            }

            string folder = asset.GetFolder();
            if (string.IsNullOrEmpty(folder)) {
                clipOptions.errorText = NO_FOLDER_ASSIGNED_ERROR;
            }  else if (!Directory.Exists(folder)) {
                clipOptions.errorText = FOLDER_MISSING_ERROR;
            } else if (asset.GetNumImages() <=0) {
                clipOptions.errorText = NO_PICTURES_ASSIGNED_ERROR;
            }
            clipOptions.tooltip = folder;
            
            return clipOptions;
        }

//----------------------------------------------------------------------------------------------------------------------

        /// <inheritdoc/>
        public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom) {
            StreamingImageSequencePlayableAsset asset = clip.asset as StreamingImageSequencePlayableAsset;
            if (null == asset) {
                Debug.LogError("Asset is not a StreamingImageSequencePlayableAsset: " + clip.asset);
                return;
            }
            
            StreamingImageSequenceTrack sisTrack = track as StreamingImageSequenceTrack;
            Assert.IsNotNull(sisTrack);
            

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
                int numImages = asset.GetNumImages();
                if (numImages > 0) {
                    clip.duration = numImages * 0.125; // 8fps (standard limited animation)
                    clip.displayName = Path.GetFileName(asset.GetFolder());
                }
                clip.CreateCurves("Curves: " + clip.displayName);
            }


            TimelineClipSISData sisData = null;
            asset.InitTimelineClipCurve(clip);
            
            if (null == clonedFrom) {
                sisData = new TimelineClipSISData(clip);
                asset.BindTimelineClipSISData(sisData);
                return;
            }

            //Duplicate/Split process
            StreamingImageSequencePlayableAsset clonedFromAsset = clonedFrom.asset as StreamingImageSequencePlayableAsset;
            Assert.IsNotNull(clonedFromAsset);
            
            TimelineClipSISData otherSISData = clonedFromAsset.GetBoundTimelineClipSISData();
            sisData = new TimelineClipSISData(clip, otherSISData);
            asset.BindTimelineClipSISData(sisData);
            clip.displayName = clonedFrom.displayName + " (Cloned)";

        }

//----------------------------------------------------------------------------------------------------------------------

        private static void InitializeAssetFromDefaultAsset(StreamingImageSequencePlayableAsset playableAsset,
            UnityEditor.DefaultAsset timelineDefaultAsset) 
        {
            string path = AssetDatabase.GetAssetPath(timelineDefaultAsset).Replace("\\","/");
            const bool ASK_TO_COPY = false;
            ImageSequenceImporter.ImportImages(path, playableAsset, ASK_TO_COPY);
        }

//----------------------------------------------------------------------------------------------------------------------
        /// <inheritdoc/>
        public override void OnClipChanged(TimelineClip clip) {
            base.OnClipChanged(clip);
                        
            StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;
            Assert.IsNotNull(sisAsset);
            sisAsset.RefreshPlayableFrames();            
        }

//----------------------------------------------------------------------------------------------------------------------

        /// <inheritdoc/>
        public override void DrawBackground(TimelineClip clip, ClipBackgroundRegion region) {
            base.DrawBackground(clip, region);
            
            Rect rect = region.position;
            if (rect.width <= SISEditorConstants.MIN_PREVIEW_REGION_WIDTH)
                return;

            StreamingImageSequencePlayableAsset curAsset = clip.asset as StreamingImageSequencePlayableAsset;
            if (null == curAsset || curAsset.GetNumImages() <=0)
                return;

            
            if (Event.current.type == EventType.Repaint) {
                PreviewClipInfo clipInfo = new PreviewClipInfo() {
                    Duration = clip.duration,
                    TimeScale = clip.timeScale,
                    ClipIn = clip.clipIn,
                    FramePerSecond = clip.parentTrack.timelineAsset.editorSettings.fps,
                    ImageDimensionRatio = curAsset.GetOrUpdateDimensionRatio(),
                    VisibleLocalStartTime =  region.startTime,
                    VisibleLocalEndTime   = region.endTime,
                    VisibleRect = rect,
                };
                
                PreviewUtility.EnumeratePreviewImages(ref clipInfo, (PreviewDrawInfo drawInfo) => {
                    DrawPreviewImage(ref drawInfo, clip, curAsset);
                });
                
                //For hiding frame marker automatically
                TimelineClipSISData timelineClipSISData = curAsset.GetBoundTimelineClipSISData();
                if (null != timelineClipSISData) {                
                    timelineClipSISData.UpdateTimelineWidthPerFrame(rect.width, region.endTime-region.startTime, 
                        clipInfo.FramePerSecond, clipInfo.TimeScale);
                }                                
            }
        }
        
//----------------------------------------------------------------------------------------------------------------------
        void DrawPreviewImage(ref PreviewDrawInfo drawInfo, TimelineClip clip, StreamingImageSequencePlayableAsset sisAsset) {
            int imageIndex = sisAsset.LocalTimeToImageIndex(clip, drawInfo.LocalTime);       
            string imagePath = sisAsset.GetImageFilePath(imageIndex);
            PreviewUtility.DrawPreviewImage(ref drawInfo, imagePath);
        
        }
        

//----------------------------------------------------------------------------------------------------------------------

    }
}
