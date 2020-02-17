using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace UnityEngine.StreamingImageSequence {

[Serializable]
[CustomStyle("UseImageMarker")]
internal class UseImageMarker : Marker, INotification {
//----------------------------------------------------------------------------------------------------------------------    

    internal void Init(PlayableFrame controller) {
        m_owner = controller;
    } 

    
//----------------------------------------------------------------------------------------------------------------------    
    internal void Refresh() {
        if (null == m_owner)
            return;
        
        TimelineClip clip = m_owner.GetPlayableAsset().GetTimelineClip();
        time = clip.start + m_owner.GetLocalTime();       
    }

//----------------------------------------------------------------------------------------------------------------------    
    internal PlayableFrame GetOwner() { return m_owner;}
    internal bool IsImageUsed() { return m_owner && m_owner.IsUsed(); }
    internal void SetImageUsed(bool used) { m_owner.SetUsed(used); }
    
//----------------------------------------------------------------------------------------------------------------------    
    public PropertyName id { get; } //use default implementation

    [SerializeField] private PlayableFrame m_owner;
       
    //[TODO-sin: 2020-2-7] Refresh the texture immediately when m_owner.useImage is modified
}

} //end namespace


//A visual representation (Marker) of PlayableFrame