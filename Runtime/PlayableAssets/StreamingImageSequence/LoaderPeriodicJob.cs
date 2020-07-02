using System.Collections.Generic;
using System.Reflection;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence
{
internal class LoaderPeriodicJob : PeriodicJob
{
    StreamingImageSequenceTrack m_track;



    public LoaderPeriodicJob(StreamingImageSequenceTrack track) : base(UpdateManager.JobOrder.Normal)
    {
        m_track = track;
    }

    public override void Initialize()
    {
    }

    public override void Cleanup() {
    }  
    
//----------------------------------------------------------------------------------------------------------------------    
    public override void Execute()
    {
        if ( m_track == null )
        {   // After discarding track, it becomes null as Equals is overriden.
            RemoveIfFinished();
            return;
        }

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


}

} //end namespace
