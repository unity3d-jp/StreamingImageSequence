using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {

// A PlayableBehaviour for RenderCachePlayableAsset that is attached to a Track via CreateTrackMixer() 
internal class RenderCachePlayableMixer : BasePlayableMixer<RenderCachePlayableAsset> {




//----------------------------------------------------------------------------------------------------------------------

    #region IPlayableBehaviour interfaces


    public override void OnGraphStart(Playable playable) {
        //Need to bind TimelineClips first
        IEnumerable<TimelineClip> clips = GetClips();
        foreach (TimelineClip clip in clips) {
            RenderCachePlayableAsset asset = clip.asset as RenderCachePlayableAsset;
            Assert.IsNotNull(asset);
            
            asset.BindTimelineClip(clip);
        }

    }


    #endregion

//----------------------------------------------------------------------------------------------------------------------    
    

    protected override void InitInternalV(GameObject gameObject) {
    }
    
//----------------------------------------------------------------------------------------------------------------------    

    protected override void ProcessActiveClipV(RenderCachePlayableAsset asset,
        double directorTime, TimelineClip activeClip) 
    {

    }
    
}

} //end namespace