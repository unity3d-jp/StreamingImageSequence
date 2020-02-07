using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace UnityEngine.StreamingImageSequence {

internal class UseImageMarker : Marker, INotification {

    ~UseImageMarker() {
        m_playableAsset.HideImage(time);
    }

//----------------------------------------------------------------------------------------------------------------------    
    internal void SetPlayableAsset(StreamingImageSequencePlayableAsset playableAsset) {
        m_playableAsset = playableAsset;
    } 
    
    internal StreamingImageSequencePlayableAsset GetPlayableAsset() {  return m_playableAsset; }
    
//----------------------------------------------------------------------------------------------------------------------    
    public PropertyName id { get; } //use default implementation

    [SerializeField] private StreamingImageSequencePlayableAsset m_playableAsset = null;
}

} //end namespace