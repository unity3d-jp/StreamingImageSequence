using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {

[Serializable]
[CustomStyle("UseImageMarker")]
[HideInMenu]
internal class UseImageMarker : Marker, INotification {
//----------------------------------------------------------------------------------------------------------------------    
    internal void Init(SISPlayableFrame controller) {
        m_owner = controller;
    } 

    
//----------------------------------------------------------------------------------------------------------------------    
    internal void Refresh() {
        if (null == m_owner)
            return;
        
        TimelineClip clip = m_owner.GetOwner().GetClipOwner();
        time = clip.start + m_owner.GetLocalTime();       
    }

//----------------------------------------------------------------------------------------------------------------------    
    internal bool IsImageUsed() { return null!=m_owner && m_owner.IsUsed(); }
    internal void SetImageUsed(bool used) { m_owner.SetUsed(used); }
    
//----------------------------------------------------------------------------------------------------------------------    
    public PropertyName id { get; } //use default implementation

    private SISPlayableFrame m_owner;
       
    //[TODO-sin: 2020-2-7] Refresh the texture immediately when m_owner.useImage is modified
}

} //end namespace


//A visual representation (Marker) of SISPlayableFrame