using System.Collections.Generic;

namespace UnityEngine.StreamingImageSequence {
    [System.Serializable]
    public class StreamingImageSequencePlayableAssetParam
    {
        public int Version;
        public ImageDimension<int> Resolution;
        public ImageDimension<float> QuadSize;
        public string Folder;
        public List<string> Pictures;
    }

} //end namespace