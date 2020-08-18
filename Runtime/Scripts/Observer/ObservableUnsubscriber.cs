using System;
using System.Collections.Generic;

namespace UnityEngine.StreamingImageSequence
{

//For subscribing a particular observer from where it has been registered into
internal class ObservableUnsubscriber<T> : IDisposable {

    internal ObservableUnsubscriber(ICollection<IObserver<T>> allObservers, IObserver<T> observer) {
        this.m_allObservers = allObservers;
        this.m_observer  = observer;
    }

    public void Dispose() {
        if (m_allObservers.Contains(m_observer))
            m_allObservers.Remove(m_observer);
    }
    
//----------------------------------------------------------------------------------------------------------------------
    
    private readonly ICollection<IObserver<T>> m_allObservers;
    private readonly IObserver<T>       m_observer;
}
} //end namespace
