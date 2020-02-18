using System;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {
    
[Serializable]
internal class PlayableFrame : ScriptableObject {

    internal void Init(StreamingImageSequencePlayableAsset asset, double localTime) {
        m_useImage = true;
        m_playableAsset = asset;
        m_localTime = localTime;

        if (null == m_marker) {
            TimelineClip timelineClip = m_playableAsset.GetTimelineClip();
            m_marker = timelineClip.parentTrack.CreateMarker<UseImageMarker>(localTime);
        }

        m_marker.Init(this);
    }

    internal StreamingImageSequencePlayableAsset GetPlayableAsset() {  return m_playableAsset; }

//----------------------------------------------------------------------------------------------------------------------
    internal bool IsUsed() { return m_useImage; }
    internal void SetUsed(bool used) { m_useImage = used; }
    internal double GetLocalTime() { return m_localTime; }

//----------------------------------------------------------------------------------------------------------------------

    [SerializeField] private bool m_useImage;
    [SerializeField] private double m_localTime;
    [SerializeField] private StreamingImageSequencePlayableAsset m_playableAsset = null; 
    [SerializeField] private UseImageMarker m_marker = null; //ScriptableObject -> Marker -> UseImageMarker

}


} //end namespace


//A structure to store if we should use the image at a particular frame