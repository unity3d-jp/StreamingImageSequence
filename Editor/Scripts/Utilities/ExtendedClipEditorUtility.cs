using UnityEngine;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

namespace Unity.FilmInternalUtilities.Editor {

internal static class ExtendedClipEditorUtility {
    
    internal static void ResetClipDataCurve<T>(BaseExtendedClipPlayableAsset<T> playableAsset, EditorCurveBinding curveBinding) 
        where T: BaseClipData, IAnimationCurveOwner
    {
                
        T clipData = playableAsset.GetBoundClipData();        
        Assert.IsNotNull(clipData);

        TimelineClip clip = clipData.GetOwner();
        Assert.IsNotNull(clip);
        
        AnimationCurve animationCurve = AnimationCurve.Linear(0, 0, (float) (clip.duration * clip.timeScale),1 );
        clipData.SetAnimationCurve(animationCurve);        
        SetTimelineClipCurve(clip, animationCurve, curveBinding);
        clip.clipIn = 0;
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
//----------------------------------------------------------------------------------------------------------------------    
    
    
    internal static ExtendedClipCurveStatus SetClipDataCurve<T>(TimelineClip srcClip, AnimationCurve srcCurve, 
        EditorCurveBinding srcCurveBinding) 
        where T: BaseClipData, IAnimationCurveOwner
    {
        BaseExtendedClipPlayableAsset<T> playableAsset = srcClip.asset as BaseExtendedClipPlayableAsset<T>;
        if (null == playableAsset) {
            return ExtendedClipCurveStatus.INVALID_ASSET;            
        }
        
        IAnimationCurveOwner clipData = playableAsset.GetBoundClipData() as IAnimationCurveOwner;
        if (null == clipData) {
            //The srcClip is not ready. Not deserialized yet
            return ExtendedClipCurveStatus.CLIP_DATA_NOT_BOUND;
        }
               
        clipData.SetAnimationCurve(srcCurve);            
        return ExtendedClipCurveStatus.OK;
    }

    
//----------------------------------------------------------------------------------------------------------------------    
    
    internal static void CreateTimelineClipCurve(TimelineClip clip, EditorCurveBinding curveBinding) {        
        clip.CreateCurves("Curves: " + clip.displayName);
                
        //Init initial linear srcCurve
        AnimationCurve curve = CreateDefaultAnimationCurve(clip);
        SetTimelineClipCurve(clip,curve, curveBinding);

    }
    
//----------------------------------------------------------------------------------------------------------------------    

    //Make sure that TimelineClip has a curve set
    internal static AnimationCurve ValidateTimelineClipCurve(TimelineClip clip, EditorCurveBinding curveBinding)         
    {
        AnimationCurve curve = null;
        if (null == clip.curves) {
            clip.CreateCurves("Curves: " + clip.displayName);
        } else {
            curve = AnimationUtility.GetEditorCurve(clip.curves, curveBinding);            
        }        
        
        if (null == curve) {
            curve = CreateDefaultAnimationCurve(clip);
            SetTimelineClipCurve(clip,curve, curveBinding);
        }

        return curve;
    }
    
//----------------------------------------------------------------------------------------------------------------------
    private static AnimationCurve CreateDefaultAnimationCurve(TimelineClip clip) {
        return AnimationCurve.Linear(0f,0f,(float)(clip.duration * clip.timeScale),1f);        
    }
    
//----------------------------------------------------------------------------------------------------------------------


    private static void SetTimelineClipCurve(TimelineClip destClip, AnimationCurve srcCurve, EditorCurveBinding curveBinding) {
        AnimationUtility.SetEditorCurve(destClip.curves, curveBinding, srcCurve);
        
#if AT_USE_TIMELINE_GE_1_5_0                    
        TimelineEditor.Refresh(RefreshReason.WindowNeedsRedraw );
#else         
        TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved ); //must use this for Pre- 1.5.0
#endif //AT_USE_TIMELINE_GE_1_5_0            
        
    }
}

} //end namespace

