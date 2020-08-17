using System;
using System.Runtime.InteropServices;

namespace UnityEngine.StreamingImageSequence {

    //Delegates
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void DelegateStringFunc([MarshalAs(UnmanagedType.LPStr)] string str);


//----------------------------------------------------------------------------------------------------------------------
    internal static class StreamingImageSequencePlugin {

        internal static event Action<string> OnImageUnloaded = null;
        
        internal static void UnloadImageAndNotify(string imagePath) {
            UnloadImage(imagePath);
            OnImageUnloaded?.Invoke(imagePath);
        }
        

//only support Windows and OSX
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX


        private const string LOADER_DLL = "Loader";

        // Implemented in Loader dll
        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern bool LoadAndAllocFullImage([MarshalAs(UnmanagedType.LPStr)]string fileName, int frame);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern bool LoadAndAllocPreviewImage([MarshalAs(UnmanagedType.LPStr)]string fileName, int width, int height, int frame);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern void GetImageDataInto([MarshalAs(UnmanagedType.LPStr)]string fileName, int imageType, int frame, out ImageData tResult);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int UnloadImage([MarshalAs(UnmanagedType.LPStr)]string fileName);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern void UnloadAllImages();

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern void ListLoadedImages(int textureType, DelegateStringFunc func);
               


        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int GetImageLoadOrder(int imageType);        
        
        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern void SetMaxImagesMemory(int maxImageMemoryMB);
        
        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int GetUsedImagesMemory();
        
            
        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern void ResetPlugin();

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern void ResetImageLoadOrder();

        
#endif //UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX



       
//----------------------------------------------------------------------------------------------------------------------

    }

}
