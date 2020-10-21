using System;
using System.Runtime.InteropServices;

namespace Unity.StreamingImageSequence {

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
        
//----------------------------------------------------------------------------------------------------------------------

        private const string SIS_DLL = "StreamingImageSequence";

        [DllImport(SIS_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern bool LoadAndAllocFullImage([MarshalAs(UnmanagedType.LPStr)]string fileName, int frame);

        [DllImport(SIS_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern bool LoadAndAllocPreviewImage([MarshalAs(UnmanagedType.LPStr)]string fileName, int width, int height, int frame);

        [DllImport(SIS_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern void GetImageDataInto([MarshalAs(UnmanagedType.LPStr)]string fileName, int imageType, int frame, out ImageData tResult);

        [DllImport(SIS_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        private static extern int UnloadImage([MarshalAs(UnmanagedType.LPStr)]string fileName);

        [DllImport(SIS_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern void UnloadAllImages();

        [DllImport(SIS_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern void ListLoadedImages(int textureType, DelegateStringFunc func);
               


        [DllImport(SIS_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int GetImageLoadOrder(int imageType);        
        
        [DllImport(SIS_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern void SetMaxImagesMemory(int maxImageMemoryMB);
        
        [DllImport(SIS_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern int GetUsedImagesMemory();
        
            
        [DllImport(SIS_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern void ResetPlugin();

        [DllImport(SIS_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern void ResetImageLoadOrder();


       
//----------------------------------------------------------------------------------------------------------------------

    }

}
