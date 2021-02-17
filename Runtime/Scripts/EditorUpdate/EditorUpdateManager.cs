#if UNITY_EDITOR

using System;
using System.Collections.Generic;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;

using Unity.FilmInternalUtilities;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Timeline;

namespace Unity.StreamingImageSequence
{
    

[InitializeOnLoad]
internal class EditorUpdateManager {

    static EditorUpdateManager() {
        EditorApplication.update           += EditorUpdateManager_Update;
        EditorSceneManager.sceneClosed     += EditorUpdateManager_OnSceneClosed;
        EditorSceneManager.newSceneCreated += EditorUpdateManager_OnSceneCreated;
        EditorSceneManager.sceneOpened     += EditorUpdateManager_OnSceneOpened;

        OnUnityEditorFocus += UpdateImageFolderPlayableAsset;

    }

    ~EditorUpdateManager() {
        StreamingImageSequencePlugin.ResetPlugin();        
    }

    private static void EditorUpdateManager_OnSceneClosed(Scene scene) {
        //Reset all imageLoading process when closing the scene
        ResetImageLoading();
    }

    private static void EditorUpdateManager_OnSceneCreated( Scene scene, NewSceneSetup setup, NewSceneMode mode) {
        //Reset all imageLoading process when creating a new scene
        ResetImageLoading();        
    }

    private static void EditorUpdateManager_OnSceneOpened( Scene scene, OpenSceneMode mode) {
        if (OpenSceneMode.Single != mode)
            return;
        
        
        ResetImageLoading();         
    }
   
//----------------------------------------------------------------------------------------------------------------------        

    static void EditorUpdateManager_Update() {
        UpdateEditorFocus();       
       
        double time = EditorApplication.timeSinceStartup;
        double timeDifference = time - m_lastUpdateInEditorTime;
        if (timeDifference < 0.016f) {
            return;
        }
             
        m_lastUpdateInEditorTime = time;

        //add requested jobs
        foreach (IUpdateTask job in m_requestedTasks) {
            m_mainThreadPeriodJobs.Add(job);
        }           
        m_requestedTasks.Clear();
        
        //Remove jobs
        foreach (IUpdateTask job in m_toRemoveTasks) {
            m_mainThreadPeriodJobs.Remove(job);
        }           
        m_toRemoveTasks.Clear();
        
        //Execute
        foreach (IUpdateTask job in m_mainThreadPeriodJobs) {
            job.Execute();
        }
        

    }

    internal static void ResetImageLoading() {
        ThreadManager.Reset();
        StreamingImageSequencePlugin.ResetPlugin();
        foreach (IUpdateTask job in m_mainThreadPeriodJobs) {
            job.Reset();
        }
        
        
    }    
//----------------------------------------------------------------------------------------------------------------------

    internal static void AddEditorUpdateTask(IUpdateTask job) {
        m_requestedTasks.Add(job);  
    }

    internal static void RemoveEditorUpdateTask(IUpdateTask job) {
        //Check if the job hasn't been actually added yet
        if (m_requestedTasks.Contains(job)) {
            m_requestedTasks.Remove(job);
            return;
        }
        
        Assert.IsTrue(m_mainThreadPeriodJobs.Contains(job));
        m_toRemoveTasks.Add(job);
    }
    

//----------------------------------------------------------------------------------------------------------------------


    static void UpdateEditorFocus() {

        bool isAppActive = UnityEditorInternal.InternalEditorUtility.isApplicationActive;
        if (!m_editorFocused && isAppActive) {
            m_editorFocused = true;
            OnUnityEditorFocus?.Invoke(true);

        } else if (m_editorFocused && !isAppActive) {
            m_editorFocused = false;
            OnUnityEditorFocus?.Invoke(false);
        }        
        
    }
    
//----------------------------------------------------------------------------------------------------------------------

    static void UpdateImageFolderPlayableAsset(bool isEditorFocused) {
        if (!isEditorFocused)
            return;

        if (null == TimelineEditor.inspectedAsset) {
            return;            
        }
        

        IEnumerable<TrackAsset> trackAssets = TimelineEditor.inspectedAsset.GetOutputTracks();            
        foreach (TrackAsset trackAsset in trackAssets) {
            BaseTrack baseTrack = trackAsset as BaseTrack;
            if (null == baseTrack)
                continue;

            if (!BitUtility.IsSet((int)baseTrack.GetCapsV(), (int) SISTrackCaps.IMAGE_FOLDER))
                continue;
                       
            IEnumerable<TimelineClip> clips = trackAsset.GetClips();
            foreach (TimelineClip clip in clips) {
                ImageFolderPlayableAsset imageFolderPlayableAsset = clip.asset as ImageFolderPlayableAsset;
                Assert.IsNotNull(imageFolderPlayableAsset);
                imageFolderPlayableAsset.Reload();                
            }
        }
        

    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    private static double m_lastUpdateInEditorTime;
       
    //"Jobs" are higher level than tasks
    private static readonly HashSet<IUpdateTask> m_mainThreadPeriodJobs = new HashSet<IUpdateTask>();
    private static readonly List<IUpdateTask>    m_requestedTasks        = new List<IUpdateTask>();
    private static readonly HashSet<IUpdateTask> m_toRemoveTasks         = new HashSet<IUpdateTask>();
    
    private static event Action<bool> OnUnityEditorFocus;
    private static bool m_editorFocused;    
}


} //end namespace


#endif  //UNITY_EDITOR
