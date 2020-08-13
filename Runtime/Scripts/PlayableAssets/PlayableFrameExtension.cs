using System;

namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
internal static class PlayableFrameExtension {

    internal static void SetUsed(this SISPlayableFrame playableFrame, bool used) {
        playableFrame.SetBoolProperty(PlayableFramePropertyName.USED, used);
    }

    internal static bool IsUsed(this SISPlayableFrame playableFrame) {
        return null != playableFrame && playableFrame.GetBoolProperty(PlayableFramePropertyName.USED);
    }
    
    internal static void SetLocked(this SISPlayableFrame playableFrame, bool used) {
        playableFrame.SetBoolProperty(PlayableFramePropertyName.LOCKED, used);
    }

    internal static bool IsLocked(this SISPlayableFrame playableFrame) {
        return null != playableFrame && playableFrame.GetBoolProperty(PlayableFramePropertyName.LOCKED);
    }
    
}

} //end namespace

