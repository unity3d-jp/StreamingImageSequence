using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;

namespace UnityEngine.StreamingImageSequence
{
    

[InitializeOnLoad]
internal class EditorUpdateManager {

    static EditorUpdateManager() {
        EditorApplication.update           += EditorUpdateManager_Update;
        EditorSceneManager.sceneClosed     += EditorUpdateManager_OnSceneClosed;
        EditorSceneManager.newSceneCreated += EditorUpdateManager_OnSceneCreated;
    }

    ~EditorUpdateManager() {
        StreamingImageSequencePlugin.ResetPlugin();        
    }

    static void EditorUpdateManager_OnSceneClosed(SceneManagement.Scene scene) {
        //Reset all imageLoading process when closing the scene
        ResetImageLoading();
    }

    static void EditorUpdateManager_OnSceneCreated( SceneManagement.Scene scene, NewSceneSetup setup, NewSceneMode mode) {
        //Reset all imageLoading process when creating a new scene
        ResetImageLoading();        
    }
    
   
//----------------------------------------------------------------------------------------------------------------------        

    static void EditorUpdateManager_Update() {
        
       
        double time = EditorApplication.timeSinceStartup;
        double timeDifference = time - m_lastUpdateInEditorTime;
        if (timeDifference < 0.016f) {
            return;
        }
             
        m_lastUpdateInEditorTime = time;

        //add requested jobs
        foreach (ITask job in m_requestedTasks) {
            m_mainThreadPeriodJobs.Add(job);
        }           
        m_requestedTasks.Clear();
        
        //Remove jobs
        foreach (ITask job in m_toRemoveTasks) {
            m_mainThreadPeriodJobs.Remove(job);
        }           
        m_toRemoveTasks.Clear();
        
        //Execute
        foreach (ITask job in m_mainThreadPeriodJobs) {
            job.Execute();
        }

    }

    internal static void ResetImageLoading() {
        StreamingImageSequencePlugin.ResetPlugin();
        ThreadManager.Reset();            
        
    }    
//----------------------------------------------------------------------------------------------------------------------

    internal static bool AddEditorUpdateTask(ITask job) {
        m_requestedTasks.Add(job);  
        return true;
    }

    internal static bool RemoveEditorUpdateTask(ITask job) {
        //Check if the job hasn't been actually added yet
        if (m_requestedTasks.Contains(job)) {
            m_requestedTasks.Remove(job);
            return true;
        }
        
        Assert.IsTrue(m_mainThreadPeriodJobs.Contains(job));
        m_toRemoveTasks.Add(job);
        return true;
    }
    

//----------------------------------------------------------------------------------------------------------------------
    
    private static double m_lastUpdateInEditorTime;
       
    //"Jobs" are higher level than tasks
    private static readonly HashSet<ITask> m_mainThreadPeriodJobs = new HashSet<ITask>();
    private static readonly List<ITask>    m_requestedTasks        = new List<ITask>();
    private static readonly HashSet<ITask> m_toRemoveTasks         = new HashSet<ITask>();
}


} //end namespace


#endif  //UNITY_EDITOR
