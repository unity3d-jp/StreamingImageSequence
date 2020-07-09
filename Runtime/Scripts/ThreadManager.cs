using System.Collections.Generic;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.StreamingImageSequence
{
    
internal static class ThreadManager {

//----------------------------------------------------------------------------------------------------------------------
    
#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void InitInEditor() {
        EditorApplication.playModeStateChanged += ChangedPlayModeState;
        
        //Avoid calling StartThread() multiple times (with InitInRuntime()) 
        bool isPlayingOrWillChangePlaymode = EditorApplication.isPlayingOrWillChangePlaymode;
        if (!isPlayingOrWillChangePlaymode) {
            StartThread();
        }
        
        
    }
#endif
    
    [RuntimeInitializeOnLoadMethod]
    internal static void InitInRuntime() {
        LogUtility.LogDebug("ThreadManager::InitInRuntime()");        
        StartThread();
    }

//----------------------------------------------------------------------------------------------------------------------    
    internal static void Reset() {
        lock (m_backGroundTaskQueue) {
            m_backGroundTaskQueue.Clear();
        }        
    }

//----------------------------------------------------------------------------------------------------------------------    
#if UNITY_EDITOR


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

#endif  //UNITY_EDITOR

//----------------------------------------------------------------------------------------------------------------------
    public static bool QueueBackGroundTask(ITask task) {
        lock (m_backGroundTaskQueue) {
            m_backGroundTaskQueue.Enqueue(task);
        }
        return true;
    }               
    

//----------------------------------------------------------------------------------------------------------------------        
    static void StartThread() {
        //Debug.Log("Start Thread");
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
            ITask task = null;

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
        //Debug.Log("Stop Thread");

        m_shuttingDownThreads = true;
        for (int i = 0; i < NUM_THREAD; ++i)  {
            if (m_threads[i] != null) {
                m_threads[i].Join();
            }
        }
        
        m_shuttingDownThreads = false;
    }
//----------------------------------------------------------------------------------------------------------------------


    //Threads processes tasks
    const                   uint                   NUM_THREAD            = 3;
    private static readonly Thread[]               m_threads             = new Thread[NUM_THREAD];
    private static readonly Queue<ITask> m_backGroundTaskQueue = new Queue<ITask>();
   
    
    private static bool m_shuttingDownThreads;

}

}
