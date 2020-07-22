using JetBrains.Annotations;
using System.IO;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using System.Collections.Generic;
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
            } else if (asset.GetImageFileNames() == null) {
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
                if (asset.HasImages()) {
                    clip.duration = asset.GetImageFileNames().Count * 0.125; // 8fps (standard limited animation)
                    clip.displayName = Path.GetFileName(asset.GetFolder());
                }
                clip.CreateCurves("Curves: " + clip.displayName);
            }


            TimelineClipSISData sisData = null;

            if (null == clonedFrom) {
                sisData = new TimelineClipSISData(clip);
                asset.BindTimelineClip(clip, sisData);
                return;
            }

            //Duplicate/Split process
            StreamingImageSequencePlayableAsset clonedFromAsset = clonedFrom.asset as StreamingImageSequencePlayableAsset;
            Assert.IsNotNull(clonedFromAsset);
            
            TimelineClipSISData otherSISData = clonedFromAsset.GetBoundTimelineClipSISData();
            sisData = new TimelineClipSISData(clip, otherSISData);
            asset.BindTimelineClip(clip, sisData);            
            clip.displayName = clonedFrom.displayName + " (Cloned)";
                           
               
            asset.OnClonedFrom(clonedFromAsset);

        }

//----------------------------------------------------------------------------------------------------------------------

        private static void InitializeAssetFromDefaultAsset(StreamingImageSequencePlayableAsset playableAsset,
            UnityEditor.DefaultAsset timelineDefaultAsset) 
        {
            string path = AssetDatabase.GetAssetPath(timelineDefaultAsset).Replace("\\","/");
            const bool ASK_TO_COPY = false;
            ImageSequenceImporter.ImportPictureFiles(ImageFileImporterParam.Mode.StreamingAssets, path, playableAsset, ASK_TO_COPY);
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
            const int VISIBLE_WIDTH_THRESHOLD = 10; //width that is too small will affect the placement of preview imgs
            if (rect.width <= VISIBLE_WIDTH_THRESHOLD)
                return;

            StreamingImageSequencePlayableAsset curAsset = clip.asset as StreamingImageSequencePlayableAsset;
            if (null == curAsset || !curAsset.HasImages())
                return;


            m_colorSpace = QualitySettings.activeColorSpace;
           
            
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
                
            }
        }
        
//----------------------------------------------------------------------------------------------------------------------
        void DrawPreviewImage(ref PreviewDrawInfo drawInfo, TimelineClip clip, StreamingImageSequencePlayableAsset sisAsset) {
            int imageIndex = sisAsset.LocalTimeToImageIndex(clip, drawInfo.LocalTime);
        
            IList<string> imageFileNames = sisAsset.GetImageFileNames();

            //Load
            string fullPath = sisAsset.GetFullPath(imageFileNames[imageIndex]);
            ImageLoader.GetImageDataInto(fullPath, StreamingImageSequenceConstants.IMAGE_TYPE_PREVIEW
                , out ImageData readResult);
            
            switch (readResult.ReadStatus) {
                case StreamingImageSequenceConstants.READ_STATUS_LOADING:
                    break;
                case StreamingImageSequenceConstants.READ_STATUS_SUCCESS: {
                    Texture2D tex = PreviewTextureFactory.GetOrCreate(fullPath, ref readResult);
                    if (null != tex) {
                        Graphics.DrawTexture(drawInfo.DrawRect, tex);
                    }
                    break;
                }
                default: {
                    ImageLoader.RequestLoadPreviewImage(fullPath, (int) drawInfo.DrawRect.width, (int) drawInfo.DrawRect.height);                    
                    break;
                }

            }
        
        }
        

//----------------------------------------------------------------------------------------------------------------------

        ColorSpace m_colorSpace = ColorSpace.Uninitialized;

    }
}
