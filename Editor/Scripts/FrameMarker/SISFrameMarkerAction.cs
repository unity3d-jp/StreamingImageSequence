#if AT_USE_TIMELINE_GE_1_4_0            

using System.Collections.Generic;
using Unity.StreamingImageSequence;
using UnityEngine.Timeline;

using UnityEditor.Timeline.Actions;

namespace Unity.StreamingImageSequence.Editor {

[MenuEntry("Edit")]

internal class SISFrameMarkerAction_Edit : MarkerAction
{
    ///<inheritdoc />
    public override bool Execute(IEnumerable<IMarker> markers) {
        
        foreach (IMarker marker in markers) {
            FrameMarker frameMarker = marker as FrameMarker;
            if (null == frameMarker)
                return false;
            
            SISPlayableFrame playableFrame = frameMarker.GetOwner();
            StreamingImageSequencePlayableAsset playableAsset = playableFrame.GetTimelineClipAsset<StreamingImageSequencePlayableAsset>();            
            if (null == playableAsset)
                return false;
            
            FrameMarkerInspector.EditPlayableFrame(playableFrame, playableAsset);
            
        }

        return true;
    }

//----------------------------------------------------------------------------------------------------------------------    
    ///<inheritdoc />
    public override ActionValidity Validate(IEnumerable<IMarker> markers) {        
        foreach(IMarker marker in markers) {
            FrameMarker frameMarker = marker as FrameMarker;
            if (null == frameMarker)
                return ActionValidity.NotApplicable;
            
            
            SISPlayableFrame playableFrame = frameMarker.GetOwner();
            StreamingImageSequencePlayableAsset playableAsset = playableFrame.GetTimelineClipAsset<StreamingImageSequencePlayableAsset>();            
            if (null == playableAsset)
                return ActionValidity.NotApplicable;            

        }
        return ActionValidity.Valid;
    }
    
}


} //end namespace


#endif //AT_USE_TIMELINE_GE_1_4_0
