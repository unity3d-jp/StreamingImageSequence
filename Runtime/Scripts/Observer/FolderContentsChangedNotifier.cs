using System;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace UnityEngine.StreamingImageSequence
{
    
internal sealed class FolderContentsChangedNotifier : IObservable<string> {

#if UNITY_EDITOR
    [InitializeOnLoadMethod]
    static void FolderContentsChangedNotifier_OnEditorLoad() {        
        EditorSceneManager.sceneClosed += FolderContentsChangedNotifier_OnSceneClosed;
    }
    
    static void FolderContentsChangedNotifier_OnSceneClosed(Scene scene) {
        FolderContentsChangedNotifier.GetInstance().UnsubscribeAll();
    }
#endif

//----------------------------------------------------------------------------------------------------------------------    

    internal static FolderContentsChangedNotifier GetInstance() {
        return m_instance;
    }    
//----------------------------------------------------------------------------------------------------------------------    
        
    private FolderContentsChangedNotifier() {
        m_observers = new HashSet<IObserver<string>>();
    }
    
    public IDisposable Subscribe(IObserver<string> observer) {
        if (! m_observers.Contains(observer)) {
            m_observers.Add(observer);
        }
        return new ObservableUnsubscriber<string>(m_observers, observer);
    }

    internal bool Unsubscribe(IObserver<string> observer) {
        if (! m_observers.Contains(observer)) {
            return false;
        }

        m_observers.Remove(observer);
        return true;
    }

    private void UnsubscribeAll() {
        m_observers.Clear();
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    
    internal void Notify(string folder) {
        foreach (IObserver<string> observer in m_observers) {
            observer.OnNext(folder);
        }
        
    }
    
//----------------------------------------------------------------------------------------------------------------------    
    private readonly HashSet<IObserver<string>> m_observers;
    
    private static readonly FolderContentsChangedNotifier m_instance = new FolderContentsChangedNotifier();

}

} //end namespace
