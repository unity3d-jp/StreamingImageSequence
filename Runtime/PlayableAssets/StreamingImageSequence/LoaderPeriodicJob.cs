using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence
{
internal class LoaderPeriodicJob : PeriodicJob { 

    public LoaderPeriodicJob(StreamingImageSequenceTrack track) : base()  {
        m_track = track;
    }

//----------------------------------------------------------------------------------------------------------------------    

    internal  override void Cleanup() {
    }  
    
//----------------------------------------------------------------------------------------------------------------------    
    internal  override void Execute() {
        Assert.IsNotNull(m_track);

        //Only continue preloading images when we are not in play mode 
        if (Application.isPlaying)
            return;

        IEnumerable<TimelineClip> clips = m_track.GetClips();
        foreach (TimelineClip clip in clips) {
            StreamingImageSequencePlayableAsset sisAsset  = clip.asset as StreamingImageSequencePlayableAsset;
            if (null == sisAsset) {
                continue;
            }

            if (!Application.isPlaying)
                sisAsset.ContinuePreloadingImages();
        }

    }

    
//----------------------------------------------------------------------------------------------------------------------

    readonly StreamingImageSequenceTrack m_track;

}

} //end namespace
