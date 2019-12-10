using System.Collections.Generic;

namespace UnityEngine.StreamingImageSequence {
    [System.Serializable]
    public class StreamingImageSequencePlayableAssetParam
    {
        [System.Serializable]
        public struct StPicResolution
        {
            public int Width;
            public int Height;
        };
        [System.Serializable]
        public struct StQuadSize
        {
            public float sizX;
            public float sizY;
        }
        public int Version;
        public StPicResolution Resolution;
        public StQuadSize QuadSize;
        public string Folder;
        public List<string> Pictures;
    }

} //end namespace