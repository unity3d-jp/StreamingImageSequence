﻿using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

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
        Assert.IsNotNull(m_mixer);

        //Only continue preloading images when we are not in play mode 
        if (Application.isPlaying)
            return;

        IEnumerable<TimelineClip> clips = m_mixer.GetClips();
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

    readonly StreamingImageSequencePlayableMixer  m_mixer;

}

} //end namespace
