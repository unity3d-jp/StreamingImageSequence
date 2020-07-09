using System.Collections.Generic;
using UnityEditor;

namespace UnityEngine.StreamingImageSequence {


internal static class ImageLoader  {

    
#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void ImageLoaderOnLoad() {
        
        EditorApplication.playModeStateChanged += ImageLoader_PlayModeStateChanged;
        
        bool isPlayingOrWillChangePlaymode = EditorApplication.isPlayingOrWillChangePlaymode;
        if (!isPlayingOrWillChangePlaymode) {
            Init();
        }        
    }
    

    static void ImageLoader_PlayModeStateChanged(PlayModeStateChange state) {
        if (PlayModeStateChange.EnteredEditMode != state)
            return;

        Init();
    }

    static void Init() {
        for (int i = 0; i < StreamingImageSequenceConstants.MAX_IMAGE_TYPES; ++i) {
            ImageLoadEditorUpdateTask task = new ImageLoadEditorUpdateTask();
            EditorUpdateManager.AddEditorUpdateTask(task);
            m_imageLoadEditorUpdateTasks[i] = task;
        }
    }
    
#endif
    
//----------------------------------------------------------------------------------------------------------------------   

    internal static void RequestLoadFullImage(string imagePath) {
        FullImageLoadBGTask task = new FullImageLoadBGTask(imagePath, Time.frameCount);
        RequestLoadImageInternal(StreamingImageSequenceConstants.IMAGE_TYPE_FULL, task);
    }

    internal static void RequestLoadPreviewImage(string imagePath, int width, int height) {
        PreviewImageLoadBGTask task = new PreviewImageLoadBGTask(imagePath, Time.frameCount, width, height);
        RequestLoadImageInternal(StreamingImageSequenceConstants.IMAGE_TYPE_PREVIEW, task);
    }
//----------------------------------------------------------------------------------------------------------------------   
    
    private static void RequestLoadImageInternal(int index, BaseImageLoadBGTask imageLoadBGTask) {
        
#if UNITY_EDITOR        
        if (!Application.isPlaying) {
            m_imageLoadEditorUpdateTasks[index].RequestLoadImage(imageLoadBGTask);
            return;
        }
#endif

        ThreadManager.QueueBackGroundTask(imageLoadBGTask);

    }
    
//----------------------------------------------------------------------------------------------------------------------

#if UNITY_EDITOR
    private static readonly ImageLoadEditorUpdateTask[] m_imageLoadEditorUpdateTasks 
        = new ImageLoadEditorUpdateTask[StreamingImageSequenceConstants.MAX_IMAGE_TYPES];

#endif
}

} //end namespace
