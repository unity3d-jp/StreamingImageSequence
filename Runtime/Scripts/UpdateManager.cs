using System.Collections.Generic;
using UnityEngine.Assertions;
using System.Text.RegularExpressions;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace UnityEngine.StreamingImageSequence
{
    
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
internal class UpdateManager {
    
    static UpdateManager() {
        EditorApplication.update               += UpdateInEditor;        
    }


#if UNITY_EDITOR

   
//----------------------------------------------------------------------------------------------------------------------        

    static void UpdateInEditor() {
        
        double time = EditorApplication.timeSinceStartup;
        double timeDifference = time - m_lastUpdateInEditorTime;
        if (timeDifference < 0.016f) {
            return;
        }

       
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
//----------------------------------------------------------------------------------------------------------------------

    public static bool AddPeriodicJob(PeriodicJob job) {
        m_requestedJobs.Add(job);  
        return true;
    }

    public static bool RemovePeriodicJob(PeriodicJob job) {
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

#endif  //UNITY_EDITOR

         

   
//----------------------------------------------------------------------------------------------------------------------

    private static string GetApplicationDataPath() {
        
        // Application.dataPath cant be used in back thread, so we cache it hire.
        if (s_AppDataPath == null)
        {
            s_AppDataPath = Application.dataPath;
        }
        return s_AppDataPath;
    }


    public static string GetProjectFolder()
    {
        Regex regAssetFolder = new Regex("/Assets$");
        var strPorjectFolder = UpdateManager.GetApplicationDataPath(); //  Application.dataPath; cant use this in back thread;
        strPorjectFolder = regAssetFolder.Replace(strPorjectFolder, "");

        return strPorjectFolder;
    }

    public static string ToRelativePath( string strAbsPath )
    {
        string newPath = strAbsPath.Remove(0, GetProjectFolder().Length);
        while ( newPath.StartsWith("/"))
        {
            newPath = newPath.Remove(0, 1);
        }
        return newPath;
    }






//----------------------------------------------------------------------------------------------------------------------
    
    private static double m_lastUpdateInEditorTime;
       
    //"Jobs" are higher level than tasks
    private static readonly HashSet<PeriodicJob> m_mainThreadPeriodJobs = new HashSet<PeriodicJob>();
    private static readonly List<PeriodicJob>    m_requestedJobs        = new List<PeriodicJob>();
    private static readonly HashSet<PeriodicJob> m_toRemoveJobs         = new HashSet<PeriodicJob>();
    
    private static string s_AppDataPath;
    
}

} //end namespace
