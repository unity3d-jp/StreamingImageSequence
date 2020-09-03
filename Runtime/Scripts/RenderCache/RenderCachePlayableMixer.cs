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
        var clipAssets = GetClipAssets();
        foreach (KeyValuePair<TimelineClip, RenderCachePlayableAsset> kv in clipAssets) {
            RenderCachePlayableAsset playableAsset = kv.Value;                
            playableAsset.OnGraphStart(playable);                
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

//----------------------------------------------------------------------------------------------------------------------
    
    protected override void ShowObjectV(bool show) {
        
    }    
    
}

} //end namespace