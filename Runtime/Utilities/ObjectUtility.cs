namespace UnityEngine.StreamingImageSequence
{

internal static class ObjectUtility
{
    public static void Destroy(UnityEngine.Object obj, bool allowDestroyingAssets=false) {
#if UNITY_EDITOR   
        Object.DestroyImmediate(obj, allowDestroyingAssets);
#else
        Object.Destroy(obj);
#endif
    }

}

} //end namespace
