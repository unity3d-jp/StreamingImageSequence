using System;
using System.Collections.Generic;

namespace UnityEngine.StreamingImageSequence
{
    
internal sealed class FolderContentsChangedNotifier : IObservable<string> {

    internal static FolderContentsChangedNotifier GetInstance() {
        return m_instance;
    }    
//----------------------------------------------------------------------------------------------------------------------    
        
    private FolderContentsChangedNotifier() {
        m_observers = new List<IObserver<string>>();
    }
    
    public IDisposable Subscribe(IObserver<string> observer) {
        if (! m_observers.Contains(observer)) {
            m_observers.Add(observer);
        }
        return new ObservableUnsubscriber<string>(m_observers, observer);
    }

    
//----------------------------------------------------------------------------------------------------------------------    
    private readonly List<IObserver<string>> m_observers;
    
    private static readonly FolderContentsChangedNotifier m_instance = new FolderContentsChangedNotifier();

}

} //end namespace
