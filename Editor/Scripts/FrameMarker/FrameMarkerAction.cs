using System.Collections.Generic;
using UnityEditor.Timeline.Actions;
using UnityEngine.StreamingImageSequence;
using UnityEngine.Timeline;

namespace UnityEditor.StreamingImageSequence {

[MenuEntry("Lock and Edit")]
class LockAndEditFrameMarkerAction : MarkerAction
{
    ///<inheritdoc />
    public override bool Execute(IEnumerable<IMarker> markers) {
        
        foreach (IMarker marker in markers) {
            FrameMarker frameMarker = marker as FrameMarker;
            if (null == frameMarker)
                return false;
            
            SISPlayableFrame playableFrame = frameMarker.GetOwner();
            RenderCachePlayableAsset playableAsset = playableFrame.GetTimelineClipAsset<RenderCachePlayableAsset>();            
            if (null == playableAsset)
                return false;
            
            FrameMarkerInspector.LockAndEditPlayableFrame(playableFrame, playableAsset);
            
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
            RenderCachePlayableAsset playableAsset = playableFrame.GetTimelineClipAsset<RenderCachePlayableAsset>();            
            if (null == playableAsset)
                return ActionValidity.NotApplicable;            

        }
        return ActionValidity.Valid;
    }
    
}


} //end namespace

