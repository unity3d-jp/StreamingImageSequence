using System.Reflection;
using UnityEditor;
using System;

namespace Unity.StreamingImageSequence.Editor {

internal static class UnityEditorReflection {
    
    //[TODO-sin: 2021-9-10] Move to FIU
    internal static readonly Type TIMELINE_EDITOR_CLIP_TYPE = Type.GetType("UnityEditor.Timeline.EditorClip, Unity.Timeline.Editor");
    internal static readonly PropertyInfo TIMELINE_EDITOR_CLIP_PROPERTY = TIMELINE_EDITOR_CLIP_TYPE.GetProperty("clip"); 
    
}

} //end namespace