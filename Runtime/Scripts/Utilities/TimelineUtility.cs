using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {


internal static class TimelineUtility {

//----------------------------------------------------------------------------------------------------------------------
    internal static int CalculateNumFrames(TimelineClip clip) {
        float fps       = clip.parentTrack.timelineAsset.editorSettings.fps;
        int   numFrames = Mathf.RoundToInt((float)(clip.duration * fps));
        return numFrames;
            
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    internal static double CalculateTimePerFrame(TimelineClip clip) {
        return CalculateTimePerFrame(clip.parentTrack);
    }

//----------------------------------------------------------------------------------------------------------------------
    internal static double CalculateTimePerFrame(TrackAsset  trackAsset) {
        float  fps          = trackAsset.timelineAsset.editorSettings.fps;
        double timePerFrame = 1.0f / fps;
        return timePerFrame;
    }
    
//----------------------------------------------------------------------------------------------------------------------
    internal  static void ResetTimelineCurve(TimelineClip clip) {
        AnimationCurve animationCurve = new AnimationCurve();
        StreamingImageSequencePlayableAsset.ValidateAnimationCurve(ref animationCurve, (float) (clip.duration * clip.timeScale));
        StreamingImageSequencePlayableAsset.RefreshTimelineClipCurve(clip, animationCurve);
        clip.clipIn    = 0;
    }
    
    
//----------------------------------------------------------------------------------------------------------------------
   

}

} //ena namespace
