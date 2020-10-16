namespace Unity.StreamingImageSequence {

    internal static class StreamingImageSequenceConstants {
        public const string DIALOG_HEADER = "StreamingImageSequence";

        public const int READ_STATUS_UNAVAILABLE   = -1;
        public const int READ_STATUS_IDLE          = 0;
        public const int READ_STATUS_LOADING       = 1;
        public const int READ_STATUS_SUCCESS       = 2;
        public const int READ_STATUS_FAIL          = 3;
        public const int READ_STATUS_OUT_OF_MEMORY = 4;

        public const int IMAGE_FORMAT_RGBA32 = 0;
        public const int IMAGE_FORMAT_BGRA32 = 1;

        public const int IMAGE_TYPE_FULL      = 0;
        public const int IMAGE_TYPE_PREVIEW   = 1;
        public const int MAX_IMAGE_TYPES      = IMAGE_TYPE_PREVIEW + 1;

        internal const string MENU_PATH = "Assets/Streaming Image Sequence/";


    }

} //end namespace