using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Assertions;

namespace Unity.MovieProxy
{
#if UNITY_EDITOR
    [InitializeOnLoad]

    public class EditorPeriodicJob : PeriodicJob
    {
 
        static Dictionary<MovieProxyPlayableAsset, BGJobCacheParam> m_MovieProxyPlayableAssetToColorArray = new Dictionary<MovieProxyPlayableAsset, BGJobCacheParam>();

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
            m_MovieProxyPlayableAssetToColorArray = new Dictionary<MovieProxyPlayableAsset, BGJobCacheParam>();
        }
        public override void Initialize()
        {

        }

        public override void Reset()
        {
             m_MovieProxyPlayableAssetToColorArray = null;
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
            if ( m_MovieProxyPlayableAssetToColorArray == null )
            {
                Reinitialize();
            }

            Util.Log("EditorPeriodicJob::Executing");
            UpdateManager.GetStreamingAssetPath(); // must be executed in main thread.

            PlayableDirector  currentDirector = UpdateManager.GetCurrentDirector();
            if (currentDirector == null)
            {
                PluginUtil.HideAllOverwrapWindows();
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
                 else if (track.GetType() == typeof(MovieProxyTrack))
                {
                    // MovieProxy Track
                     ProcessMovieProxyTrack( track as MovieProxyTrack);
                }
            }
        }

        private static void ProcessMovieProxyTrack(MovieProxyTrack  track)
        {
            foreach (var clip in track.GetClips())
            {

                // You might want to use "as" rather than compare type.
                // "as" sometimes fail on first importing time for project.
                if (clip.asset.GetType() != typeof(MovieProxyPlayableAsset))
                {
                    Debug.LogError("MovieProxyPlayableAsset is broken:" + clip.asset.name);
                    continue;

                }

                /*
                if (clip.asset == null)
                {
                    Debug.LogError("MovieProxyPlayableAsset on " + clip.displayName + " is broken.");
                    continue;
                }*/

                MovieProxyPlayableAsset asset = (MovieProxyPlayableAsset)clip.asset;
                int length = asset.Pictures.Length;
                if (m_MovieProxyPlayableAssetToColorArray.ContainsKey(asset))
                {


                }
                else
                {
                    m_MovieProxyPlayableAssetToColorArray.Add(asset, new BGJobCacheParam(asset));
                }
                var param = m_MovieProxyPlayableAssetToColorArray[asset];
                int allAreLoaded = PluginUtil.GetAllAreLoaded(asset.GetInstanceID());

                if (allAreLoaded == 0)
                {
                    new BGJobCacheChecker(m_MovieProxyPlayableAssetToColorArray[asset]);
                    UInt32[] colorArray = m_MovieProxyPlayableAssetToColorArray[asset].m_collorArray;
                    if (colorArray == null)
                    {
                        return;
                    }
                    PluginUtil.SetOverwrapWindowData(asset.GetInstanceID(), colorArray, colorArray.Length);
                    if (param.m_allLoaded)
                    {
                        PluginUtil.SetAllAreLoaded(asset.GetInstanceID(), 1);
                    }
                }
                else
                {
                    PluginUtil.HideOverwrapWindow(asset.GetInstanceID());
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
                if (assetType != typeof(MovieProxyPlayableAsset))
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
                int isLoaded = PluginUtil.GetAllAreLoaded(clip.asset.GetInstanceID());

                if ((rect.y - treeviewPos.y > 0.0f - offsetY && rect.y - treeviewPos.y < trackbounds.height - trackbounds.y && width > 0.0f - offsetY)
                      && isLoaded == 0)
                { 
                       PluginUtil.ShowOverwrapWindow(clip.asset.GetInstanceID(), (int)startX, (int)startY, (int)width, 1, forceChange); // (int)rect.height);
                }
                else
                {
                  PluginUtil.HideOverwrapWindow(clip.asset.GetInstanceID());
                }
            }
#endif
        }




    }

#endif
}
