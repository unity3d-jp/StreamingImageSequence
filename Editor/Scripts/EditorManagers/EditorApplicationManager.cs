using UnityEditor;
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

/* [Note-sin: 2021-2-26]
 * In EditMode, the user can move the time slider as he/she wishes, so we have to try to load everything, while trying
 *     to optimize (deleting old requested tasks, etc). This is done by calling SetImageLoadTaskHandler()
 * In Playmode, the images are loaded sequentially, so the optimization is not necessary, and what is requested should
 *     be loaded.
 *
 * Ref: SISPlayableMixerUpdateTask
 */ 

} //end namespace
