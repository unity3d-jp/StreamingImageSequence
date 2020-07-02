using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Playables;
using System.Reflection;
using UnityEngine.Timeline;
using System;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.StreamingImageSequence
{
    
#if UNITY_EDITOR
    [InitializeOnLoad]
#endif
    internal class UpdateManager
    {
        public enum JobOrder
        {
            Top,
            AboveNormal,
            Normal,
            BelowNormal,
            Final
        }
        
        public delegate void SetupBeforePlay();
        public delegate void SetupAfterPlay();
        public delegate void ResetDelegate();

        public static double m_lastUpdateInEditorTime;
        
        //Threads processes tasks
        const uint NUM_THREAD = 3;
        private static readonly Thread[] m_threads = new Thread[NUM_THREAD];
        private static Thread mainThread = Thread.CurrentThread;
        private static readonly Queue<BackGroundTask> m_backGroundTaskQueue = new Queue<BackGroundTask>();
        
        //"Jobs" are higher level than tasks
        private static readonly HashSet<PeriodicJob> m_mainThreadPeriodJobs = new HashSet<PeriodicJob>();
        private static readonly List<PeriodicJob> m_requestedJobs = new List<PeriodicJob>();
        private static readonly HashSet<PeriodicJob> m_toRemoveJobs = new HashSet<PeriodicJob>();
        
        private static bool m_shuttingDownThreads;
        private static Dictionary<PlayableDirector, PlayableDirectorStatus> s_directorStatusDictiornary = new Dictionary<PlayableDirector, PlayableDirectorStatus>();
        private static string s_AppDataPath;
        private static bool m_isResettingPlugin = false;
        
        static UpdateManager()
        {
#if UNITY_EDITOR
            InitInEditor();
#endif  //UNITY_EDITOR
        }
#if UNITY_EDITOR
        public static void ResetPlugin() {
            StreamingImageSequencePlugin.ResetPlugin();
            m_isResettingPlugin = true;

            lock (m_backGroundTaskQueue) {
                m_backGroundTaskQueue.Clear();
            }
            
            StreamingImageSequencePlugin.UnloadAllImages();
            m_isResettingPlugin = false;

        }
#endif

        public static bool IsPluginResetting() {
            return m_isResettingPlugin;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        internal static void InitInRuntime()
        {

#if !UNITY_EDITOR
           UpdateManager.GetStreamingAssetPath(); // must be executed in main thread.          
           LogUtility.LogDebug("InitInRuntime()");
           StartThread();
#endif
        }


#if UNITY_EDITOR
        static void InitInEditor() {
            EditorApplication.playModeStateChanged += ChangedPlayModeState;
            EditorApplication.update += UpdateInEditor;

            StartThread();
        }


        static void ChangedPlayModeState(PlayModeStateChange state) {
            if (EditorApplication.isPaused ) {
                return;
            }

            switch (state) {
                case PlayModeStateChange.ExitingEditMode: {
                    StopThread();
                    // Util.Log("Play button was pressed.");
                    break;
                }
                case PlayModeStateChange.EnteredPlayMode: {
                    // Util.Log("Play was started.");
                    break;
                }
                case PlayModeStateChange.ExitingPlayMode: {
                    // started to play.
                    // Util.Log("Stop is pressed");
                    break;
                }
                case PlayModeStateChange.EnteredEditMode: {
                    // Util.Log("Play  stopped.");
                    break;
                }
            }
        }
        
//----------------------------------------------------------------------------------------------------------------------        

        static void UpdateInEditor() {
            
            double time = EditorApplication.timeSinceStartup;
            double timeDifference = time - m_lastUpdateInEditorTime;
            if (timeDifference < 0.016f) {
                return;
            }

            if (m_isResettingPlugin)
                return;
            
            m_lastUpdateInEditorTime = time;

            //add requested jobs
            foreach (PeriodicJob job in m_requestedJobs) {
                m_mainThreadPeriodJobs.Add(job);
            }           
            m_requestedJobs.Clear();
            
            //Remove jobs
            foreach (PeriodicJob job in m_toRemoveJobs) {
                m_mainThreadPeriodJobs.Remove(job);
                job.Cleanup();
            }           
            m_toRemoveJobs.Clear();
            
            //Execute
            foreach (PeriodicJob job in m_mainThreadPeriodJobs) {
                job.Execute();
            }

        }

#endif  //UNITY_EDITOR

//----------------------------------------------------------------------------------------------------------------------
        public static bool QueueBackGroundTask(BackGroundTask task) {
            lock (m_backGroundTaskQueue) {
                m_backGroundTaskQueue.Enqueue(task);
            }
            return true;
        }
        
//----------------------------------------------------------------------------------------------------------------------
        
        public static bool AddPeriodicJob(PeriodicJob job) {
            m_requestedJobs.Add(job);  
            return true;
        }

        public static bool RemovePeriodicJob(PeriodicJob job) {
            m_toRemoveJobs.Add(job);
            return true;
        }
        
        public static bool IsMainThread()
        {
            return (mainThread == Thread.CurrentThread);
        }

//----------------------------------------------------------------------------------------------------------------------        
        static void StartThread() {
            for (int i = 0; i < NUM_THREAD; ++i) {
                m_threads[i] = new Thread(UpdateFunction);
                m_threads[i].Start();
            }
        }
             
//----------------------------------------------------------------------------------------------------------------------        

    	static void UpdateFunction() {
            int id = Thread.CurrentThread.ManagedThreadId;

            while (!m_shuttingDownThreads) {

                LogUtility.LogDebug("alive " + id);
                BackGroundTask task = null;

                lock (m_backGroundTaskQueue) {
                    
                    if (m_backGroundTaskQueue.Count > 0) {
                        task = m_backGroundTaskQueue.Dequeue();
                    }                    
                }               
                
                if (null!=task)  {
                    task.Execute();
                } else {
                    const int SLEEP_IN_MS = 33;
                    Thread.Sleep(SLEEP_IN_MS);                    
                }
                
            }
        }

//----------------------------------------------------------------------------------------------------------------------
        
        static void StopThread() {

            m_shuttingDownThreads = true;
            for (int i = 0; i < NUM_THREAD; ++i)  {
                if (m_threads[i] != null) {
                    m_threads[i].Join();
                }
            }
            
            m_shuttingDownThreads = false;
        }
//----------------------------------------------------------------------------------------------------------------------
        
        static public string GetApplicationDataPath()
        {
            
            // Application.dataPath cant be used in back thread, so we cache it hire.
            if (s_AppDataPath == null)
            {
                s_AppDataPath = Application.dataPath;
            }
            return s_AppDataPath;
        }


        static public string GetProjectFolder()
        {
            Regex regAssetFolder = new Regex("/Assets$");
            var strPorjectFolder = UpdateManager.GetApplicationDataPath(); //  Application.dataPath; cant use this in back thread;
            strPorjectFolder = regAssetFolder.Replace(strPorjectFolder, "");

            return strPorjectFolder;
        }

        static public string ToRelativePath( string strAbsPath )
        {
            string newPath = strAbsPath.Remove(0, GetProjectFolder().Length);
            while ( newPath.StartsWith("/"))
            {
                newPath = newPath.Remove(0, 1);
            }
            return newPath;
        }

#if UNITY_EDITOR
        static public PlayableDirector GetCurrentDirector()
        {

            EditorWindow timelineWindow = UpdateManager.GetTimelineWindow();

            if (timelineWindow == null)
            {
                return null;
            }
            var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;
            var type = timelineWindow.GetType();
            var info = type.GetProperty("state", bf);
            var val = info.GetValue(timelineWindow, null);

            type = val.GetType();
            info = type.GetProperty("currentDirector", bf);

            // newer than 2018.3?
            if ( info == null )
            {
                info = type.GetProperty("masterSequence",bf);
                val = info.GetValue(val, null);

                type = val.GetType();
                info = type.GetProperty("director", bf);
            }
            val = info.GetValue(val, null);

            return val as PlayableDirector;
        }

        static public EditorWindow GetTimelineWindow()
        {
            EditorWindow timelineWindow = null;

            var sequenceWindowArray = Resources.FindObjectsOfTypeAll<EditorWindow>();
            if (sequenceWindowArray == null)
            {
                return null;
            }
            foreach (var w in sequenceWindowArray)
            {
                if (w.GetType().ToString() == "UnityEditor.Timeline.TimelineWindow")
                {
                    timelineWindow = w;
                    break;
                }
            }
            return timelineWindow;
        }
#endif

        static public List<TrackAsset> GetTrackList(PlayableDirector director)
        {
            TimelineAsset playbleAsset = director.playableAsset as TimelineAsset;
            return GetTrackList(playbleAsset);
        }


        static public List<TrackAsset> GetTrackList(TimelineAsset timelineAsset)
        {
            Assert.IsTrue(timelineAsset == true);
            var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField | BindingFlags.GetProperty;
            var type = timelineAsset.GetType();

            var info = type.GetProperty("tracks", bf);  // 2017.1 tracks
            if (info == null)
            {   //  newer version
                info = type.GetProperty("trackObjects", bf);
            }
            Assert.IsTrue(info.PropertyType.IsGenericType);
            var list = info.GetValue(timelineAsset, null);
            var trackAssetList = list as List<TrackAsset>;
            if (trackAssetList != null)
            {
                return trackAssetList;
            }


            var scriptableObjectList = list as List<ScriptableObject>;
            var ret = new List<TrackAsset>();
            foreach (var asset in scriptableObjectList) 
            {
                ret.Add(asset as TrackAsset);
            }

            return ret;
        }

        static public List<TrackAsset> GetTrackList(GroupTrack groupTrack)
        {

            var bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField;
            var type = groupTrack.GetType().BaseType;
            var fieldInfo = type.GetField("m_Children", bf);
            var clips = fieldInfo.GetValue(groupTrack);
            var trackAssetList = clips as List<TrackAsset>;
            if (trackAssetList != null )
            {
                return trackAssetList;  // 2017.1 tracks
            }
            // later version. 
            var scriptableObjectList = clips as List<ScriptableObject>;
            var ret = new List<TrackAsset>();
            foreach (var asset in scriptableObjectList)
            {
                ret.Add(asset as TrackAsset);
            }
            return ret;
        }

        public static bool IsDirectorIdle(PlayableDirector director)
        {
            if (director == null)
            {
                return true;
            }

            if (s_directorStatusDictiornary.ContainsKey(director))
            {


            }
            else
            {
                s_directorStatusDictiornary.Add(director, new PlayableDirectorStatus());
            }
            if (director.time - s_directorStatusDictiornary[director].m_lastDirectorTick < 0.01f)
            {
                s_directorStatusDictiornary[director].m_lastDirectorTick = director.time;
                return true;
            }
            s_directorStatusDictiornary[director].m_lastDirectorTick = director.time;
            return false;
        }

    }

    internal class PlayableDirectorStatus
    {
        public double m_lastDirectorTick = -0.02f;
    }


    internal abstract class BackGroundTask
    {
        public abstract void Execute();
    }

}
