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

    public override void Reset()
    {
        Debug.Assert(UpdateManager.IsPluginResetting());
        if (m_track == null)
        {   
            return;
        }


        var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;
        var type = m_track.GetType();
        var info = type.GetProperty("clips", bf);
        var val = info.GetValue(m_track, null);
        TimelineClip[] clipList = val as TimelineClip[];

        foreach (var cl in clipList)
        {
            var asset = cl.asset;

            // You might want to use "as" rather than compare type.
            // "as" sometimes fail on first importing time for project.
            if (asset.GetType() == typeof(StreamingImageSequencePlayableAsset))
            {
                var timelineAsset = (StreamingImageSequencePlayableAsset)asset;
                timelineAsset.Reset();

                continue;
            }
        }
    }
    
    
    public override void Execute()
    {
        if ( m_track == null )
        {   // After discarding track, it becomes null as Equals is overriden.
            RemoveIfFinished();
            return;
        }


        if (m_track != null )
        {
            var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;
            var type = m_track.GetType();
            var info = type.GetProperty("clips", bf);
            var val = info.GetValue(m_track, null);
            TimelineClip[] clipList = val as TimelineClip[];

            foreach (var cl in clipList)
            {
                var asset = cl.asset;

                // You might want to use "as" rather than compare type.
                // "as" sometimes fail on first importing time for project.
                if ( asset.GetType() == typeof(StreamingImageSequencePlayableAsset) )
                {
                    StreamingImageSequencePlayableAsset timelineAsset = (StreamingImageSequencePlayableAsset)asset;
                    if (!Application.isPlaying)
                        timelineAsset.ContinuePreloadingImages();

                    continue;
                }

                // important.
                // in order to check strictly,
                // null check of asset value must be here later than above asset.GetType() as operator == null means the object is destroyed.
                if (asset == null) {
                    Debug.LogError("StreamingImageSequencePlayableAsset on " + cl.displayName + " is broken.");
                    continue;
                }
            }



        }
    }


}

} //end namespace
