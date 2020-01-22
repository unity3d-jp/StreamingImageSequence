namespace UnityEngine.StreamingImageSequence {

    public static class StreamingImageSequenceConstants {
        public const string DIALOG_HEADER = "StreamingImageSequence";

#if UNITY_STANDALONE_OSX
        public const TextureFormat TEXTURE_FORMAT = TextureFormat.RGBA32;
#elif UNITY_STANDALONE_WIN
        public const TextureFormat TEXTURE_FORMAT = TextureFormat.BGRA32;
#endif

        public const int READ_RESULT_NONE       = 0;
        public const int READ_RESULT_REQUESTED  = 1;
        public const int READ_RESULT_SUCCESS    = 2;

    }

} //end namespace