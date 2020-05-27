using UnityEngine.Playables;

namespace UnityEngine.StreamingImageSequence {

/// <summary>
/// A PlayableAsset that is used to cache render results of a camera.
/// </summary>
[System.Serializable]
internal class RenderCachePlayableAsset : PlayableAsset {


//----------------------------------------------------------------------------------------------------------------------

    /// <summary>
    /// Constructor
    /// </summary>
    public RenderCachePlayableAsset() {
        
    }

    internal void Refresh() {
        //Move the timeline to start of the clip until the end.
        //Access the camera, and store the rendered results somewhere
        
    }
    
    
//---------------------------------------------------------------------------------------------------------------------
    
    #region PlayableAsset functions override
    /// <inheritdoc/>
    public sealed override Playable CreatePlayable(PlayableGraph graph, GameObject go) {
        //Dummy. We just need to implement this from PlayableAsset because folder D&D support. See notes below
        return Playable.Null;
    }
    
    #endregion         
    
    


}

} //end namespace

