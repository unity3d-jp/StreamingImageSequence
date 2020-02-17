using System;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.UI;

namespace UnityEngine.StreamingImageSequence {

[Serializable]
internal class UseImageMarker : Marker, INotification {


//----------------------------------------------------------------------------------------------------------------------    
    internal void Setup(StreamingImageSequencePlayableAsset playableAsset, ImageAtFrameInfo info) {
        m_playableAsset = playableAsset;
        m_info = info;
    } 
    
    internal StreamingImageSequencePlayableAsset GetPlayableAsset() {  return m_playableAsset; }
    
//----------------------------------------------------------------------------------------------------------------------    
    internal void Refresh() {
        if (null == m_playableAsset)
            return;
        
        TimelineClip clip = m_playableAsset.GetTimelineClip();
        time = clip.start + m_info.GetLocalTime();
        
    }

//----------------------------------------------------------------------------------------------------------------------    
    internal bool IsImageUsed() { return m_info.IsUsed(); }
    internal void SetImageUsed(bool used) { m_info.SetUsed(used); }
    
//----------------------------------------------------------------------------------------------------------------------    
    public PropertyName id { get; } //use default implementation

    [SerializeField] private StreamingImageSequencePlayableAsset m_playableAsset = null;
    [SerializeField] private ImageAtFrameInfo m_info;
       
    //[TODO-sin: 2020-2-7] Refresh the texture immediately when m_info.useImage is modified
}

} //end namespace


//A visual representation (Marker) of ImageAtFrameInfo