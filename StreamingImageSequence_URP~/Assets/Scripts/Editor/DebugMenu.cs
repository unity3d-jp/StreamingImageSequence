using System.Text;
using UnityEditor;
using UnityEditor.Timeline;
using UnityEngine;

public static class DebugMenu  {
    
    [MenuItem("Debug/Find Loaded Textures")]
    private static void FindLoadedTextures() {

        Texture[] textures = Resources.FindObjectsOfTypeAll<Texture>();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Found {textures.Length} textures: ");
        foreach (Texture tex in textures) {
            sb.AppendLine($"    TexName: {tex.name, -40}, Dimension: ({tex.width},{tex.height})");
        }
        Debug.Log(sb.ToString());
    }

//----------------------------------------------------------------------------------------------------------------------    
    [MenuItem("Debug/Find Loaded Objects")]
    private static void FindLoadedObjects() {

        Object[] objects = Resources.FindObjectsOfTypeAll<Object>();
        StringBuilder sb = new StringBuilder();
        sb.AppendLine($"Found {objects.Length} objects: ");
        foreach (Object obj in objects) {
            sb.AppendLine($"    ObjectName: {obj.name,-40}, Type: {obj.GetType().ToString()}");
        }
        Debug.Log(sb.ToString());
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    [MenuItem("Debug/CollectGarbage")]
    private static void CollectGarbage() {
        System.GC.Collect();            
    }

//----------------------------------------------------------------------------------------------------------------------    
    [MenuItem("Debug/UnloadUnusedAssets GC")]
    private static void UnloadUnusedAssets() {
        Resources.UnloadUnusedAssets();
    }

//----------------------------------------------------------------------------------------------------------------------    
    [MenuItem("Debug/Refresh TimelineEditor")]
    private static void RefreshTimelineEditor() {
        TimelineEditor.Refresh(RefreshReason.ContentsAddedOrRemoved);
    }
    
}
