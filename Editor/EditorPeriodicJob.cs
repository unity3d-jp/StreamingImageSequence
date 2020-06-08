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
            ShowOverwrapWindows();
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



        private static void ShowOverwrapWindows()
        {
            var timeLineWindow = UpdateManager.GetTimelineWindow();
            Rect windowPos = ((EditorWindow)timeLineWindow).position;
            //       Rect windowPos = GetWindowPosition(timeLineWindow);


            // Get Treeview
            var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;
            var type = timeLineWindow.GetType();
            var info = type.GetProperty("treeView", bf);
            var treeview = info.GetValue(timeLineWindow, null);
#if UNITY_2019_2_OR_NEWER
#else
            // Get tracksBounds
            info = type.GetProperty("tracksBounds", bf);
            Rect trackbounds = (Rect)info.GetValue(timeLineWindow, null);

            if (treeview == null)
            {
                return;
            }


            // Get ScrollPosition
            type = treeview.GetType();
            info = type.GetProperty("scrollPosition", bf);
            Vector2 treeviewPos = (Vector2)info.GetValue(treeview, null);



            // Get Tree view
            type = treeview.GetType();
            info = type.GetProperty("allClipGuis", bf);
            var allClipGui = info.GetValue(treeview, null);
            IEnumerable en = allClipGui as IEnumerable;

            //        Util.Log("treeviewPos.y:: " + treeviewPos.y);
            //        Util.Log("trackbounds.y:: " + trackbounds.y);
            //        Util.Log("window.Pos.y:: " + windowPos.y);
            //        Util.Log("window.Pos.x:: " + windowPos.x);
            float offsetY = 24.0f;
            foreach (var obj in en)
            {
                type = obj.GetType();
                info = type.GetProperty("boundingRect", bf);
                Rect rect = (Rect)info.GetValue(obj, null);

                info = type.GetProperty("clip", bf);
                var clip = info.GetValue(obj, null) as UnityEngine.Timeline.TimelineClip;
                var assetType = clip.asset.GetType();
                if (assetType != typeof(StreamingImageSequencePlayableAsset))
                {
                    continue;
                }
                float startX = windowPos.x + rect.x;
                float startY = trackbounds.y + windowPos.y + rect.y - treeviewPos.y + offsetY;
                float width = rect.width;
                int forceChange = 0;
                if (startX <= trackbounds.x + windowPos.x)
                {
                    var orgStartX = startX;
                    startX = trackbounds.x + windowPos.x;
                    width -= startX - orgStartX;
                }

                if (startX + width > windowPos.x + windowPos.width)
                {
                    width -= startX + width - (windowPos.x + windowPos.width);
                }
                int isLoaded = StreamingImageSequencePlugin.GetAllAreLoaded(clip.asset.GetInstanceID());

                if ((rect.y - treeviewPos.y > 0.0f - offsetY && rect.y - treeviewPos.y < trackbounds.height - trackbounds.y && width > 0.0f - offsetY)
                      && isLoaded == 0)
                { 
                       StreamingImageSequencePlugin.ShowOverwrapWindow(clip.asset.GetInstanceID(), (int)startX, (int)startY, (int)width, 1, forceChange); // (int)rect.height);
                }
                else
                {
                  StreamingImageSequencePlugin.HideOverwrapWindow(clip.asset.GetInstanceID());
                }
            }
#endif
        }
    }
#endif
}
