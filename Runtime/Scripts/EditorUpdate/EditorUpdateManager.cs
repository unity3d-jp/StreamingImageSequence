using System.Collections.Generic;
using UnityEngine.Assertions;

#if UNITY_EDITOR
using UnityEditor;

namespace UnityEngine.StreamingImageSequence
{
    

[InitializeOnLoad]
internal class EditorUpdateManager {
    
    static EditorUpdateManager() {
        EditorApplication.update               += EditorUpdateManager_Update;        
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
//----------------------------------------------------------------------------------------------------------------------

    public static bool AddEditorUpdateTask(ITask job) {
        m_requestedTasks.Add(job);  
        return true;
    }

    public static bool RemoveEditorUpdateTask(ITask job) {
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
