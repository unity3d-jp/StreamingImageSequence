using System.Text;
using UnityEditor;
using UnityEngine;

public static class DebugMenu  {
    
[MenuItem("Debug/Find Loaded Textures")]
private static void DebugTextures() {

    Texture[] textures = Resources.FindObjectsOfTypeAll<Texture>();
    StringBuilder sb = new StringBuilder();
    sb.AppendLine($"Found {textures.Length} textures: ");
    foreach (Texture tex in textures) {
        sb.AppendLine($"    TexName: {tex.name} Dimension: ({tex.width},{tex.height})");
    }
    Debug.Log(sb.ToString());
}
    
}
