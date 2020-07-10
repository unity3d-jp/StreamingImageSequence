﻿using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.StreamingImageSequence {


internal static class ImageLoader  {

    
#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void ImageLoaderOnLoad() {
        
        EditorApplication.playModeStateChanged += ImageLoader_PlayModeStateChanged;
        
        bool isPlayingOrWillChangePlaymode = EditorApplication.isPlayingOrWillChangePlaymode;
        if (!isPlayingOrWillChangePlaymode) {
            InitImageLoaderInEditor();
        }        
    }
    

    static void ImageLoader_PlayModeStateChanged(PlayModeStateChange state) {
        if (PlayModeStateChange.EnteredEditMode != state)
            return;

        InitImageLoaderInEditor();
        StreamingImageSequencePlugin.ResetImageLoadOrder();
    }

    static void InitImageLoaderInEditor() {
        for (int i = 0; i < StreamingImageSequenceConstants.MAX_IMAGE_TYPES; ++i) {
            if (null != m_imageLoadEditorUpdateTasks[i]) {
                //Just in case: Elements of m_imageLoadEditorUpdateTasks should be back to null after entering edit mode
                continue;                 
            }
            
            ImageLoadEditorUpdateTask task = new ImageLoadEditorUpdateTask();
            EditorUpdateManager.AddEditorUpdateTask(task);
            m_imageLoadEditorUpdateTasks[i] = task;
        }

        m_isApplicationPlaying = false;
    }
    
#endif
    
//----------------------------------------------------------------------------------------------------------------------   
    [RuntimeInitializeOnLoadMethod]
    internal static void ImageLoaderOnRuntimeLoad() {
        m_isApplicationPlaying = true;
        StreamingImageSequencePlugin.ResetImageLoadOrder();
    }
    
//----------------------------------------------------------------------------------------------------------------------   

    internal static void RequestLoadFullImage(string imagePath) {
        FullImageLoadBGTask task = new FullImageLoadBGTask(imagePath);
        RequestLoadImageInternal(StreamingImageSequenceConstants.IMAGE_TYPE_FULL, task);
    }

    internal static void RequestLoadPreviewImage(string imagePath, int width, int height) {
        PreviewImageLoadBGTask task = new PreviewImageLoadBGTask(imagePath, width, height);
        RequestLoadImageInternal(StreamingImageSequenceConstants.IMAGE_TYPE_PREVIEW, task);
    }
//----------------------------------------------------------------------------------------------------------------------   
    
    private static void RequestLoadImageInternal(int index, BaseImageLoadBGTask imageLoadBGTask) {
               
#if UNITY_EDITOR        
        if (!Application.isPlaying) {
            imageLoadBGTask.SetRequestFrame(Time.frameCount);
            m_imageLoadEditorUpdateTasks[index].RequestLoadImage(imageLoadBGTask);            
            return;
        }
#endif
        

        ThreadManager.QueueBackGroundTask(imageLoadBGTask);
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    //Wrappers so that the code to decide the currentFrame is gathered in one place

    internal static void GetImageDataInto(string fileName, int imageType, out ImageData readResult) {
        StreamingImageSequencePlugin.GetImageDataInto(fileName,imageType, GetCurrentFrame(), 
            out readResult);            
    }

    internal static bool LoadAndAllocFullImage(string fileName) {
        return StreamingImageSequencePlugin.LoadAndAllocFullImage(fileName, GetCurrentFrame());
    }

    internal  static bool LoadAndAllocPreviewImage(string fileName, int width, int height) {
        return StreamingImageSequencePlugin.LoadAndAllocPreviewImage(fileName, width, height, GetCurrentFrame());
            
    }
    
    private static int GetCurrentFrame() {

#if UNITY_EDITOR            
        if (!m_isApplicationPlaying)
            return Time.frameCount;
#endif            
        return 0;

    }
    
//----------------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
    private static readonly ImageLoadEditorUpdateTask[] m_imageLoadEditorUpdateTasks 
        = new ImageLoadEditorUpdateTask[StreamingImageSequenceConstants.MAX_IMAGE_TYPES];

    private static bool m_isApplicationPlaying = false;

#endif
}

} //end namespace
