using System.Collections.Generic;
using System.Threading;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.StreamingImageSequence
{
    
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
internal class ThreadManager {
    
    //Threads processes tasks
    const uint NUM_THREAD = 3;
    private static readonly Thread[] m_threads = new Thread[NUM_THREAD];
    private static readonly Queue<IBackGroundTask> m_backGroundTaskQueue = new Queue<IBackGroundTask>();
   
    
    private static bool m_shuttingDownThreads;
    
    static ThreadManager()
    {
#if UNITY_EDITOR
        EditorApplication.playModeStateChanged += ChangedPlayModeState;
        StartThread();
#endif  //UNITY_EDITOR
    }


    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    internal static void InitInRuntime() {

#if !UNITY_EDITOR
       LogUtility.LogDebug("InitInRuntime()");
       StartThread();
#endif
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
    public static bool QueueBackGroundTask(IBackGroundTask task) {
        lock (m_backGroundTaskQueue) {
//                Debug.Log("Background task count: " + m_backGroundTaskQueue.Count);
            m_backGroundTaskQueue.Enqueue(task);
        }
        return true;
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
            IBackGroundTask task = null;

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



}

}
