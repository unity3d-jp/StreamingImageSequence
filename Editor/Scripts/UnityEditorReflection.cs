using System.Reflection;
using UnityEditor;
using System;

namespace Unity.StreamingImageSequence.Editor {

internal static class UnityEditorReflection {
    
    internal static readonly MethodInfo SCROLLABLE_TEXT_AREA_METHOD 
        = typeof(EditorGUI).GetMethod("ScrollableTextAreaInternal", BindingFlags.Static | BindingFlags.NonPublic);

    
    internal static readonly Type TIMELINE_EDITOR_CLIP_TYPE = Type.GetType("UnityEditor.Timeline.EditorClip, Unity.Timeline.Editor");
    
}

} //end namespace