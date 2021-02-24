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
    
    internal static ExtendedClipCurveStatus SetClipDataCurve<T>(TimelineClip srcClip, EditorCurveBinding srcCurveBinding) 
        where T: BaseClipData, IAnimationCurveOwner
    {
        BaseExtendedClipPlayableAsset<T> playableAsset = srcClip.asset as BaseExtendedClipPlayableAsset<T>;
        if (null == playableAsset) {
            return ExtendedClipCurveStatus.INVALID_ASSET;            
        }

        //Check if the curves is null, which may happen if the srcClip is created using code ?
        if (null == srcClip.curves) {
            CreateClipCurve(srcClip, srcCurveBinding);
        }        
        
        IAnimationCurveOwner clipData = playableAsset.GetBoundClipData() as IAnimationCurveOwner;
        if (null == clipData) {
            //The srcClip is not ready. Not deserialized yet
            return ExtendedClipCurveStatus.CLIP_DATA_NOT_BOUND;
        }
        
        
        //Always apply clipCurves to clipData
        AnimationCurve curve = AnimationUtility.GetEditorCurve(srcClip.curves, srcCurveBinding);        
        clipData.SetAnimationCurve(curve);
        return ExtendedClipCurveStatus.OK;
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    internal static void CreateClipCurve(TimelineClip clip, EditorCurveBinding curveBinding) {        
        clip.CreateCurves("Curves: " + clip.displayName);
        
        //Init initial linear srcCurve
        AnimationCurve curve = AnimationCurve.Linear(0f,0f,(float)clip.duration,1f);
        SetTimelineClipCurve(clip,curve, curveBinding);
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

