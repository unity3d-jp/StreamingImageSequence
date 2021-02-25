using JetBrains.Annotations;
using System.IO;
using Unity.FilmInternalUtilities;
using Unity.FilmInternalUtilities.Editor;
using UnityEditor.Timeline;
using UnityEngine.Timeline;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEditor;

namespace Unity.StreamingImageSequence.Editor {

[CustomTimelineEditor(typeof(StreamingImageSequencePlayableAsset)), UsedImplicitly]
internal class StreamingImageSequencePlayableAssetEditor : ImageFolderPlayableAssetEditor<SISClipData> 
{
    private const string NO_FOLDER_ASSIGNED_ERROR = "No Folder assigned";
    private const string FOLDER_MISSING_ERROR = "Assigned folder does not exist.";
    private const string NO_PICTURES_ASSIGNED_ERROR = "No Pictures assigned";

//----------------------------------------------------------------------------------------------------------------------
    /// <inheritdoc/>
    public override ClipDrawOptions GetClipOptions(TimelineClip clip) {
        ClipDrawOptions clipOptions = base.GetClipOptions(clip);
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

        //[Note-sin: 2021-2-25] Track can be null during copy and paste
        if (null == track) {
            //This callback occurs before the clip is assigned to the track, but we need the track for creating curves.
            clip.TryMoveToTrack(track);             
        }
        
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
                SISUserSettings userSettings = SISUserSettings.GetInstance();
                
                clip.duration = (double) (numImages) / (userSettings.GetDefaultSISPlayableAssetFPS()); 
                clip.displayName = Path.GetFileName(asset.GetFolder());
            }

            ExtendedClipEditorUtility.CreateTimelineClipCurve(clip,StreamingImageSequencePlayableAsset.GetTimeCurveBinding());
        }
        
        if (null == clonedFrom) {
            asset.BindClipData(new SISClipData(clip));
            return;
        }

        //Duplicate/Split process
        StreamingImageSequencePlayableAsset clonedFromAsset = clonedFrom.asset as StreamingImageSequencePlayableAsset;
        Assert.IsNotNull(clonedFromAsset);
        
        SISClipData otherSISData = clonedFromAsset.GetBoundClipData();

        if (null == otherSISData) {            
            asset.BindClipData(new SISClipData(clip)); //[Note-sin: 2021-2-25] can be null during copy and paste
            return;
        }
        
        SISClipData sisData = new SISClipData(clip, otherSISData);
        asset.BindClipData(sisData);
        clip.displayName = clonedFrom.displayName + " (Cloned)";

    }

    
//----------------------------------------------------------------------------------------------------------------------
    //Called when a clip is changed by the Editor. (TrimStart, TrimEnd, etc)    
    public override void OnClipChanged(TimelineClip clip) {       
        base.OnClipChanged(clip);
        
        StreamingImageSequencePlayableAsset sisAsset = clip.asset as StreamingImageSequencePlayableAsset;               
        if (null == sisAsset) {
            Debug.LogError("[SIS] Clip Internal Error: Invalid Asset");
            return;
        } 
        
        EditorCurveBinding      curveBinding = StreamingImageSequencePlayableAsset.GetTimeCurveBinding();
        AnimationCurve          curve = ExtendedClipEditorUtility.ValidateTimelineClipCurve(clip, curveBinding);
                
        //Always set the curve to clipData
        ExtendedClipEditorUtility.SetClipDataCurve<SISClipData>(sisAsset, curve);

        
    }

//----------------------------------------------------------------------------------------------------------------------    
    
    
    
    
//----------------------------------------------------------------------------------------------------------------------

    private static void InitializeAssetFromDefaultAsset(StreamingImageSequencePlayableAsset playableAsset,
        UnityEditor.DefaultAsset timelineDefaultAsset) 
    {
        string path = AssetDatabase.GetAssetPath(timelineDefaultAsset).Replace("\\","/");
        const bool ASK_TO_COPY = false;
        ImageSequenceImporter.ImportImages(path, playableAsset, ASK_TO_COPY);
    }

    
//----------------------------------------------------------------------------------------------------------------------
    protected override void DrawPreviewImageV(ref PreviewDrawInfo drawInfo, TimelineClip clip, 
        ImageFolderPlayableAsset<SISClipData> playableAsset) 
    {
        StreamingImageSequencePlayableAsset sisAsset = playableAsset as StreamingImageSequencePlayableAsset;
        Assert.IsNotNull(sisAsset);
        
        int imageIndex = sisAsset.LocalTimeToImageIndex(clip, drawInfo.LocalTime);       
        string imagePath = sisAsset.GetImageFilePath(imageIndex);
        PreviewUtility.DrawPreviewImage(ref drawInfo, imagePath);
    
    }
    

//----------------------------------------------------------------------------------------------------------------------

}

} //end namespace
