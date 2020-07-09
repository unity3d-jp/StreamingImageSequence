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
        foreach (IEditorUpdateJob job in m_requestedJobs) {
            m_mainThreadPeriodJobs.Add(job);
        }           
        m_requestedJobs.Clear();
        
        //Remove jobs
        foreach (IEditorUpdateJob job in m_toRemoveJobs) {
            m_mainThreadPeriodJobs.Remove(job);
            job.Cleanup();
        }           
        m_toRemoveJobs.Clear();
        
        //Execute
        foreach (IEditorUpdateJob job in m_mainThreadPeriodJobs) {
            job.Execute();
        }

    }
//----------------------------------------------------------------------------------------------------------------------

    public static bool AddPeriodicJob(IEditorUpdateJob job) {
        m_requestedJobs.Add(job);  
        return true;
    }

    public static bool RemovePeriodicJob(IEditorUpdateJob job) {
        //Check if the job hasn't been actually added yet
        if (m_requestedJobs.Contains(job)) {
            m_requestedJobs.Remove(job);
            return true;
        }
        
        Assert.IsTrue(m_mainThreadPeriodJobs.Contains(job));
        m_toRemoveJobs.Add(job);
        return true;
    }
    

//----------------------------------------------------------------------------------------------------------------------
    
    private static double m_lastUpdateInEditorTime;
       
    //"Jobs" are higher level than tasks
    private static readonly HashSet<IEditorUpdateJob> m_mainThreadPeriodJobs = new HashSet<IEditorUpdateJob>();
    private static readonly List<IEditorUpdateJob>    m_requestedJobs        = new List<IEditorUpdateJob>();
    private static readonly HashSet<IEditorUpdateJob> m_toRemoveJobs         = new HashSet<IEditorUpdateJob>();
}


} //end namespace


#endif  //UNITY_EDITOR
