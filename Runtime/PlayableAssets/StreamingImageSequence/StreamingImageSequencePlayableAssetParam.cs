using System.Collections.Generic;

namespace UnityEngine.StreamingImageSequence {
    [System.Serializable]
    internal class StreamingImageSequencePlayableAssetParam {
        public ImageDimensionInt Resolution;
        public string Folder;
        public List<string> Pictures;
    }

} //end namespace