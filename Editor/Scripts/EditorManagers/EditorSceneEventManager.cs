using UnityEngine.SceneManagement;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace Unity.StreamingImageSequence.Editor {

[InitializeOnLoad]
internal class EditorSceneEventManager {

    static EditorSceneEventManager() {
        EditorSceneManager.sceneClosed     += EditorSceneEventManager_OnSceneClosed;
        EditorSceneManager.newSceneCreated += EditorSceneEventManager_OnSceneCreated;
        EditorSceneManager.sceneOpened     += EditorSceneEventManager_OnSceneOpened;
    }


    private static void EditorSceneEventManager_OnSceneClosed(Scene scene) {        
        ResetPluginAndTasks();         
    }

    private static void EditorSceneEventManager_OnSceneCreated( Scene scene, NewSceneSetup setup, NewSceneMode mode) {
        ResetPluginAndTasks();         
    }

    private static void EditorSceneEventManager_OnSceneOpened( Scene scene, OpenSceneMode mode) {
        if (OpenSceneMode.Single != mode)
            return;
        ResetPluginAndTasks();         
    }
//----------------------------------------------------------------------------------------------------------------------

    internal static void ResetPluginAndTasks() {
        ThreadManager.Reset();
        StreamingImageSequencePlugin.ResetPlugin();
        EditorApplicationManager.Reset();
        
    }
   
    
   
       
}


} //end namespace


