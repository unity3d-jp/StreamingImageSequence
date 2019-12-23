﻿using System.Collections.Generic;

namespace UnityEngine.StreamingImageSequence {
    [System.Serializable]
    public class StreamingImageSequencePlayableAssetParam
    {
        public int Version;
        public ImageDimensionInt Resolution;
        public ImageDimensionFloat QuadSize;
        public string Folder;
        public List<string> Pictures;
    }

} //end namespace