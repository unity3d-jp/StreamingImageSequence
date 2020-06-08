using System.Collections.Generic;
using UnityEngine;
using System;
using System.Reflection;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.StreamingImageSequence;

namespace UnityEditor.StreamingImageSequence
{
#if UNITY_EDITOR
    [InitializeOnLoad]

    internal class EditorPeriodicJob : PeriodicJob
    {

        static EditorPeriodicJob()
        {
            new EditorPeriodicJob();
        }

        public EditorPeriodicJob() : base(UpdateManager.JobOrder.Normal)
        {
            UpdateManager.AddPeriodicJob(this);
        }

        private  void Reinitialize()
        {
        }
        public override void Initialize()
        {

        }

        public override void Reset()
        {
        }

        public override void Cleanup()
        {

        }
        public override void Execute()
        {
            if ( UpdateManager.IsPluginResetting())
            {

                return;
            }

            LogUtility.LogDebug("EditorPeriodicJob::Executing");
            UpdateManager.GetStreamingAssetPath(); // must be executed in main thread.

            PlayableDirector  currentDirector = UpdateManager.GetCurrentDirector();
            if (currentDirector == null) {
                return;
            }

            //ShowOverwrapWindows();
            List<TrackAsset> trackList = UpdateManager.GetTrackList(currentDirector);
            if (trackList == null)
            {
                return;
            }
            ProcessTracks(trackList);
        }


        static void ProcessTracks(List<TrackAsset> trackList)
        {
            foreach (var track in trackList)
            {
                if (track.GetType() == typeof(GroupTrack))
                {
                    // Draw TrackGroupLeftSide
                    ProcessTrackGroup(track as GroupTrack);
                }
                else if (track.GetType() == typeof(StreamingImageSequenceTrack))
                {
                    // StreamingImageSequence Track
                     ProcessStreamingImageSequenceTrack( track as StreamingImageSequenceTrack);
                }
            }
        }

        private static void ProcessStreamingImageSequenceTrack(StreamingImageSequenceTrack  track)
        {
        }

        private static void ProcessTrackGroup(GroupTrack trackGroup)
        {
            List<TrackAsset> list = UpdateManager.GetTrackList(trackGroup);
            if (list.Count >= 1)
            {
                ProcessTracks(list);
            }
        }



    }
#endif
}
