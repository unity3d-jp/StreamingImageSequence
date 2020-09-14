namespace UnityEngine.StreamingImageSequence {

    [System.Serializable]
    internal struct ImageDimensionInt {
        public int Width;
        public int Height;

        public float CalculateRatio() {
            return Width / (float) Height;
        }

    }

} //end namespace