using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

#if UNITY_EDITOR        

namespace UnityEngine.StreamingImageSequence
{
internal class LoaderPeriodicJob : PeriodicJob { 

    public LoaderPeriodicJob(StreamingImageSequencePlayableMixer mixer) : base() {
        m_mixer = mixer;
    }

//----------------------------------------------------------------------------------------------------------------------    

    internal  override void Cleanup() {
    }  
    
//----------------------------------------------------------------------------------------------------------------------    
    internal  override void Execute() {

        //Only continue preloading images when we are not in play mode 
        if (Application.isPlaying)
            return;
        
        Assert.IsNotNull(m_mixer);

        var clipAssets = m_mixer.GetClipAssets();
        foreach (KeyValuePair<TimelineClip, StreamingImageSequencePlayableAsset> kv in clipAssets) {
            StreamingImageSequencePlayableAsset sisAsset = kv.Value;
            sisAsset.ContinuePreloadingImages();
        }

    }

    
//----------------------------------------------------------------------------------------------------------------------

    readonly StreamingImageSequencePlayableMixer  m_mixer;

}

} //end namespace

#endif //UNITY_EDITOR