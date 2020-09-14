namespace UnityEngine.StreamingImageSequence {

internal static class GameObjectExtensions {

    public static T GetOrAddComponent<T>(this GameObject gameObject) where T : Component {
        return gameObject.GetComponent<T>() ?? gameObject.AddComponent<T>();
    }

}

} //end namespace
