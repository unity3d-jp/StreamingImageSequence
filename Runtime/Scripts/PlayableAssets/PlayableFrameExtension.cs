using System;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
internal static class PlayableFrameExtension {

    internal static void SetUsed(this SISPlayableFrame playableFrame, bool used) {
        playableFrame.SetBoolProperty(PlayableFramePropertyID.USED, used);
    }

    internal static bool IsUsed(this SISPlayableFrame playableFrame) {
        return null != playableFrame && playableFrame.GetBoolProperty(PlayableFramePropertyID.USED);
    }
//----------------------------------------------------------------------------------------------------------------------    
    
    internal static void SetLocked(this SISPlayableFrame playableFrame, bool used) {
        playableFrame.SetBoolProperty(PlayableFramePropertyID.LOCKED, used);
    }

    internal static bool IsLocked(this SISPlayableFrame playableFrame) {
        return null != playableFrame && playableFrame.GetBoolProperty(PlayableFramePropertyID.LOCKED);
    }
    
    internal static T GetTimelineClipAsset<T>(this SISPlayableFrame playableFrame) where T : Object {
        
        TimelineClip     timelineClip  = playableFrame.GetOwner().GetOwner();
        if (null == timelineClip)
            return null;
        
        T clipAsset = timelineClip.asset as T;
        return clipAsset;
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
}

} //end namespace

