using UnityEngine.Playables;

namespace UnityEngine.StreamingImageSequence {

    /// <summary>
    /// A PlayableAsset that is used to cache render results of a camera.
    /// </summary>
    [System.Serializable]
    public class RenderCachePlayableAsset : PlayableAsset {


//----------------------------------------------------------------------------------------------------------------------

        /// <summary>
        /// Constructor
        /// </summary>
        public RenderCachePlayableAsset() {
            
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
}

