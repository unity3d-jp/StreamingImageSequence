using System;
using System.Collections.Generic;

namespace UnityEngine.StreamingImageSequence
{
    
internal class PlayableAssetFolderBroadcaster : IObservable<string> {

    internal static PlayableAssetFolderBroadcaster GetInstance() {
        return m_instance;
    }    
//----------------------------------------------------------------------------------------------------------------------    
        
    private PlayableAssetFolderBroadcaster() {
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
    
    private static readonly PlayableAssetFolderBroadcaster m_instance = new PlayableAssetFolderBroadcaster();

}

} //end namespace
