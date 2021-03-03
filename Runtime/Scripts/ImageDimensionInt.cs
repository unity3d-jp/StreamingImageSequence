namespace Unity.StreamingImageSequence {

    [System.Serializable]
    internal struct ImageDimensionInt {
        public int Width;
        public int Height;

        public float CalculateRatio() {
            return Width / (float) Height;
        }

        public override string ToString() {
            return $"({Width}, {Height})";
        }
    }

} //end namespace