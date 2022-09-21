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

    internal static void Reset() {        
        foreach (IEditorTask job in m_activeEditorTasks) {
            job.Reset();
        }
    }    
    
//----------------------------------------------------------------------------------------------------------------------    
#region Image Loading
    
    
    static void InitImageLoaderForEditMode() {
        for (int i = 0; i < StreamingImageSequenceConstants.MAX_IMAGE_TYPES; ++i) {
            if (null != m_imageLoadEditorUpdateTasks[i]) {
                //Just in case: Elements of m_imageLoadEditorUpdateTasks should be back to null after entering edit mode
                continue;                 
            }
            
            ImageLoadEditorTask task = new ImageLoadEditorTask();
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
        foreach (IEditorTask job in m_requestedTasks) {
            m_activeEditorTasks.Add(job);
        }           
        m_requestedTasks.Clear();
        
        //Remove jobs
        foreach (IEditorTask job in m_toRemoveTasks) {
            m_activeEditorTasks.Remove(job);
            m_lastTaskExecuteTime.Remove(job);
        }
        m_toRemoveTasks.Clear();
        
        //Execute
        foreach (IEditorTask job in m_activeEditorTasks) {
            if (m_lastTaskExecuteTime.TryGetValue(job, out double lastExecuteTime)) {
                if ((time - lastExecuteTime) <= job.GetExecutionFrequency())
                    continue;
            }
            
            job.Execute();
            m_lastTaskExecuteTime[job] = time;
        }
        

    }


    internal static void AddUpdateTask(IEditorTask job) {
        m_requestedTasks.Add(job);  
    }

    internal static void RemoveUpdateTask(IEditorTask job) {
        //Check if the job hasn't been actually added yet
        if (m_requestedTasks.Contains(job)) {
            m_requestedTasks.Remove(job);
            return;
        }
        
        Assert.IsTrue(m_activeEditorTasks.Contains(job));
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
    
    private static readonly ImageLoadEditorTask[] m_imageLoadEditorUpdateTasks 
        = new ImageLoadEditorTask[StreamingImageSequenceConstants.MAX_IMAGE_TYPES];

    
    private static double m_lastUpdateInEditorTime;
       
    //"Jobs" are higher level than tasks
    private static readonly HashSet<IEditorTask> m_activeEditorTasks    = new HashSet<IEditorTask>();
    private static readonly List<IEditorTask>    m_requestedTasks       = new List<IEditorTask>();
    private static readonly HashSet<IEditorTask> m_toRemoveTasks        = new HashSet<IEditorTask>();
    
    private static readonly Dictionary<IEditorTask, double> m_lastTaskExecuteTime = new Dictionary<IEditorTask, double>();
    
    
    private static event Action<bool> OnUnityEditorFocus;
    private static bool               m_editorFocused;    
    
}

/* [Note-sin: 2021-2-26]
 * In EditMode, the user can move the time slider as he/she wishes, so we have to try to load everything, while trying
 *     to optimize (deleting old requested tasks, etc). This is done by calling SetImageLoadTaskHandler()
 * In Playmode, the images are loaded sequentially, so the optimization is not necessary, and what is requested should
 *     be loaded.
 *
 * Ref: SISPlayableMixerEditorTask
 */ 

} //end namespace
