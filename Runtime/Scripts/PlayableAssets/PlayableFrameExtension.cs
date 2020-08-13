using System;

namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
internal static class PlayableFrameExtension {

    internal static void SetUsed(this SISPlayableFrame playableFrame, bool used) {
        playableFrame.SetBoolProperty(PlayableFramePropertyID.USED, used);
    }

    internal static bool IsUsed(this SISPlayableFrame playableFrame) {
        return null != playableFrame && playableFrame.GetBoolProperty(PlayableFramePropertyID.USED);
    }
    
    internal static void SetLocked(this SISPlayableFrame playableFrame, bool used) {
        playableFrame.SetBoolProperty(PlayableFramePropertyID.LOCKED, used);
    }

    internal static bool IsLocked(this SISPlayableFrame playableFrame) {
        return null != playableFrame && playableFrame.GetBoolProperty(PlayableFramePropertyID.LOCKED);
    }
    
}

} //end namespace

