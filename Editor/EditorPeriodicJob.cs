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
        private static Dictionary<StreamingImageSequencePlayableAsset, BGJobCacheParam> m_streamingImageSequencePlayableAssetToColorArray = new Dictionary<StreamingImageSequencePlayableAsset, BGJobCacheParam>();

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
            m_streamingImageSequencePlayableAssetToColorArray = new Dictionary<StreamingImageSequencePlayableAsset, BGJobCacheParam>();
        }
        public override void Initialize()
        {

        }

        public override void Reset()
        {
             m_streamingImageSequencePlayableAssetToColorArray = null;
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
            if ( m_streamingImageSequencePlayableAssetToColorArray == null )
            {
                Reinitialize();
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
            foreach (var clip in track.GetClips())
            {
                var asset = clip.asset as StreamingImageSequencePlayableAsset;
                if (null == asset)
                    continue;

                if (!asset.Verified)
                {
                    continue;
                }
                
                var length = asset.GetImagePaths().Count;
                if (m_streamingImageSequencePlayableAssetToColorArray.ContainsKey(asset))
                {


                }
                else
                {
                    m_streamingImageSequencePlayableAssetToColorArray.Add(asset, new BGJobCacheParam(asset));
                }
                var param = m_streamingImageSequencePlayableAssetToColorArray[asset];
                int allAreLoaded = StreamingImageSequencePlugin.GetAllAreLoaded(asset.GetInstanceID());

                if (allAreLoaded == 0)
                {
                    new BGJobCacheChecker(m_streamingImageSequencePlayableAssetToColorArray[asset]);
                    if (param.m_allLoaded)
                    {
                        StreamingImageSequencePlugin.SetAllAreLoaded(asset.GetInstanceID(), 1);
                    }
                }


            }
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
