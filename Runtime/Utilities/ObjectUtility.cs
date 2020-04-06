#if UNITY_EDITOR   
using UnityEditor;
#endif

namespace UnityEngine.StreamingImageSequence
{

internal static class ObjectUtility
{

    internal static T CreateScriptableObjectInstance<T>() where T : ScriptableObject{
        T obj = ScriptableObject.CreateInstance<T>();
#if UNITY_EDITOR
        Undo.RegisterCreatedObjectUndo(obj, "Creating " + typeof(T));
#endif
        return obj;
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    internal static void Destroy(UnityEngine.Object obj) {
#if UNITY_EDITOR   
        Undo.DestroyObjectImmediate(obj);
#else
        Object.Destroy(obj);
#endif
    }

}

} //end namespace
