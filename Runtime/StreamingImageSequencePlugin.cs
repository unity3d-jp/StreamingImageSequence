using System;
using System.Runtime.InteropServices;

namespace UnityEngine.StreamingImageSequence {

    //Delegates
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateStringFunc([MarshalAs(UnmanagedType.LPStr)] string str);

//----------------------------------------------------------------------------------------------------------------------
    internal static class StreamingImageSequencePlugin {

//only support Windows and OSX
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX


        private const string LOADER_DLL = "Loader";

        // Implemented in Loader dll
        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern bool LoadAndAllocFullImage([MarshalAs(UnmanagedType.LPStr)]string fileName, int frame);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern bool LoadAndAllocPreviewImage([MarshalAs(UnmanagedType.LPStr)]string fileName, int width, int height, int frame);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern bool GetImageData([MarshalAs(UnmanagedType.LPStr)]string fileName, int imageType, int frame, out ImageData tResult);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int UnloadImage([MarshalAs(UnmanagedType.LPStr)]string fileName);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern void UnloadAllImages();

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern void ListLoadedImages(int textureType, DelegateStringFunc func);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern void ResetPlugin();


#endif //UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX


//----------------------------------------------------------------------------------------------------------------------
    }

}
