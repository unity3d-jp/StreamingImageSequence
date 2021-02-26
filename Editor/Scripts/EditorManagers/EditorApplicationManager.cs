using System;
using System.Collections.Generic;
using Unity.FilmInternalUtilities;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Timeline;

namespace Unity.StreamingImageSequence.Editor {


static class EditorApplicationManager  {

    [InitializeOnLoadMethod]
    static void EditorApplicationManager_OnEditorLoad() {
        
        EditorApplication.update               += EditorApplicationManager_Update;
        EditorApplication.playModeStateChanged += EditorApplicationManager_PlayModeStateChanged;
        OnUnityEditorFocus                     += UpdateImageFolderPlayableAsset;
        
        bool isPlayingOrWillChangePlaymode = EditorApplication.isPlayingOrWillChangePlaymode;
        if (!isPlayingOrWillChangePlaymode) {
            InitImageLoaderForEditMode();
        }        
    }
    

    static void EditorApplicationManager_PlayModeStateChanged(PlayModeStateChange state) {
        ImageLoader.SetImageLoadTaskHandler(null);
        if (PlayModeStateChange.EnteredEditMode != state)
            return;

        InitImageLoaderForEditMode();
        StreamingImageSequencePlugin.ResetImageLoadOrder();
    }
    
//----------------------------------------------------------------------------------------------------------------------    
#region Image Loading
    
    internal static void ResetImageLoading() {
        ThreadManager.Reset();
        StreamingImageSequencePlugin.ResetPlugin();        
        foreach (IUpdateTask job in m_mainThreadPeriodJobs) {
            job.Reset();
        }
    }    
    
    static void InitImageLoaderForEditMode() {
        for (int i = 0; i < StreamingImageSequenceConstants.MAX_IMAGE_TYPES; ++i) {
            if (null != m_imageLoadEditorUpdateTasks[i]) {
                //Just in case: Elements of m_imageLoadEditorUpdateTasks should be back to null after entering edit mode
                continue;                 
            }
            
            ImageLoadEditorUpdateTask task = new ImageLoadEditorUpdateTask();
            AddUpdateTask(task);
            m_imageLoadEditorUpdateTasks[i] = task;
        }
        
    
        ImageLoader.SetImageLoadTaskHandler(RequestLoadImageInEditMode);
    }

    static bool RequestLoadImageInEditMode(int imageType, BaseImageLoadBGTask task) {

        Assert.IsFalse(Application.isPlaying);
        
        if (null == m_imageLoadEditorUpdateTasks[imageType])
            return false;
        return m_imageLoadEditorUpdateTasks[imageType].RequestLoadImage(task);            
    }
    
#endregion Image Loading

//----------------------------------------------------------------------------------------------------------------------

#region Update
    
    static void EditorApplicationManager_Update() {
        UpdateEditorFocus();       
       
        double time           = EditorApplication.timeSinceStartup;
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


    internal static void AddUpdateTask(IUpdateTask job) {
        m_requestedTasks.Add(job);  
    }

    internal static void RemoveUpdateTask(IUpdateTask job) {
        //Check if the job hasn't been actually added yet
        if (m_requestedTasks.Contains(job)) {
            m_requestedTasks.Remove(job);
            return;
        }
        
        Assert.IsTrue(m_mainThreadPeriodJobs.Contains(job));
        m_toRemoveTasks.Add(job);
    }

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
    
 #endregion Update

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
                IReloader imageFolderPlayableAsset = clip.asset as IReloader;
                Assert.IsNotNull(imageFolderPlayableAsset);
                imageFolderPlayableAsset.Reload();                
            }
        }
        

    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    private static readonly ImageLoadEditorUpdateTask[] m_imageLoadEditorUpdateTasks 
        = new ImageLoadEditorUpdateTask[StreamingImageSequenceConstants.MAX_IMAGE_TYPES];

    
    private static double m_lastUpdateInEditorTime;
       
    //"Jobs" are higher level than tasks
    private static readonly HashSet<IUpdateTask> m_mainThreadPeriodJobs = new HashSet<IUpdateTask>();
    private static readonly List<IUpdateTask>    m_requestedTasks       = new List<IUpdateTask>();
    private static readonly HashSet<IUpdateTask> m_toRemoveTasks        = new HashSet<IUpdateTask>();
    
    private static event Action<bool> OnUnityEditorFocus;
    private static bool               m_editorFocused;    
    
}

/* [Note-sin: 2021-2-26]
 * In EditMode, the user can move the time slider as he/she wishes, so we have to try to load everything, while trying
 *     to optimize (deleting old requested tasks, etc). This is done by calling SetImageLoadTaskHandler()
 * In Playmode, the images are loaded sequentially, so the optimization is not necessary, and what is requested should
 *     be loaded.
 *
 * Ref: SISPlayableMixerUpdateTask
 */ 

} //end namespace
