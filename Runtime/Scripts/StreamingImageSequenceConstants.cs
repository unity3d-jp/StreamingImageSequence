using System.IO;

namespace Unity.StreamingImageSequence {

    internal static class StreamingImageSequenceConstants {

        internal const string PACKAGE_NAME = "com.unity.streaming-image-sequence";        
        
        public const string DIALOG_HEADER = "StreamingImageSequence";

        internal const int READ_STATUS_UNAVAILABLE   = -1;
        internal const int READ_STATUS_IDLE          = 0;
        internal const int READ_STATUS_LOADING       = 1;
        internal const int READ_STATUS_SUCCESS       = 2;
        internal const int READ_STATUS_FAIL          = 3;
        internal const int READ_STATUS_OUT_OF_MEMORY = 4;
        
        internal const int READ_STATUS_USE_EDITOR_API = 100;
        

        public const int IMAGE_FORMAT_RGBA32 = 0;
        public const int IMAGE_FORMAT_BGRA32 = 1;

        public const int IMAGE_TYPE_FULL      = 0;
        public const int IMAGE_TYPE_PREVIEW   = 1;
        public const int MAX_IMAGE_TYPES      = IMAGE_TYPE_PREVIEW + 1;

        internal const string MENU_PATH = "Assets/Streaming Image Sequence/";

        
        
        private const string SHADERS_PATH = "Packages/com.unity.streaming-image-sequence/Runtime/Shaders";
        internal static readonly string TRANSPARENT_BG_COLOR_SHADER_PATH = Path.Combine(SHADERS_PATH, "TransparentBGColor.shader");
        internal static readonly string LINEAR_TO_GAMMA_SHADER_PATH = Path.Combine(SHADERS_PATH, "LinearToGamma.shader");
        

    }

} //end namespace