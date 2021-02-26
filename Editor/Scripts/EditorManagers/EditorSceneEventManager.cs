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
        EditorApplicationManager.ResetImageLoading(); 
    }

    private static void EditorSceneEventManager_OnSceneCreated( Scene scene, NewSceneSetup setup, NewSceneMode mode) {
        EditorApplicationManager.ResetImageLoading();         
    }

    private static void EditorSceneEventManager_OnSceneOpened( Scene scene, OpenSceneMode mode) {
        if (OpenSceneMode.Single != mode)
            return;
        EditorApplicationManager.ResetImageLoading();         
    }
   
    
//----------------------------------------------------------------------------------------------------------------------
   
       
}


} //end namespace


