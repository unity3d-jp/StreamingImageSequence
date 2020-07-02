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
        // hmmmm. seemingly kinda garbage makes invalid track.
        // we need to remove such  tracks by checking those are  really included in the timeline asset.
        if (m_track.parent == null )
        {
            RemoveIfFinished();
            return; // ???
        }
        List<TrackAsset> tracks = null;

        if ( m_track.parent.GetType() == typeof(GroupTrack))
        {
            tracks = UpdateManager.GetTrackList(m_track.parent as GroupTrack);
        }
        else if (m_track.parent.GetType() == typeof(TimelineAsset))
        {
            tracks = UpdateManager.GetTrackList(m_track.parent as TimelineAsset);
        }
        bool flgFound = false;
        if (tracks != null)
        {
            foreach (var track in tracks)
            {
                if (track == m_track)
                {
                    flgFound = true;
                    break;
                }
            }
        }
        if (flgFound == false) // the track is not included. It must be the garbage.
        {
            m_track = null;
            RemoveIfFinished();
            return;
        }
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
