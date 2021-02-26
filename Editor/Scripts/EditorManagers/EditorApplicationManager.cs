﻿using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

namespace Unity.StreamingImageSequence.Editor {


static class EditorApplicationManager  {

    [InitializeOnLoadMethod]
    static void EditorApplicationManager_OnEditorLoad() {
        
        EditorApplication.playModeStateChanged += EditorApplicationManager_PlayModeStateChanged;
        
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

    static void InitImageLoaderForEditMode() {
        for (int i = 0; i < StreamingImageSequenceConstants.MAX_IMAGE_TYPES; ++i) {
            if (null != m_imageLoadEditorUpdateTasks[i]) {
                //Just in case: Elements of m_imageLoadEditorUpdateTasks should be back to null after entering edit mode
                continue;                 
            }
            
            ImageLoadEditorUpdateTask task = new ImageLoadEditorUpdateTask();
            EditorUpdateManager.AddEditorUpdateTask(task);
            m_imageLoadEditorUpdateTasks[i] = task;
        }
        
    
        ImageLoader.SetImageLoadTaskHandler(RequestLoadImageInEditMode);
    }

//----------------------------------------------------------------------------------------------------------------------    
    static bool RequestLoadImageInEditMode(int imageType, BaseImageLoadBGTask task) {

        Assert.IsFalse(Application.isPlaying);
        
        if (null == m_imageLoadEditorUpdateTasks[imageType])
            return false;
        return m_imageLoadEditorUpdateTasks[imageType].RequestLoadImage(task);            
    }
    
    
//----------------------------------------------------------------------------------------------------------------------
    
    private static readonly ImageLoadEditorUpdateTask[] m_imageLoadEditorUpdateTasks 
        = new ImageLoadEditorUpdateTask[StreamingImageSequenceConstants.MAX_IMAGE_TYPES];

}

} //end namespace
