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

#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        private const string DRAW_OVER_WINDOW_DLL   = "DrawOverWindow";
#endif

        // Implemented in Loader dll
        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern bool LoadAndAllocFullTexture([MarshalAs(UnmanagedType.LPStr)]string fileName);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern bool LoadAndAllocPreviewTexture([MarshalAs(UnmanagedType.LPStr)]string fileName, int width, int height);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern void NativeFree(IntPtr ptr);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern bool GetNativeTextureInfo([MarshalAs(UnmanagedType.LPStr)]string fileName, out ReadResult tResult, int textureType);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int ResetNativeTexture([MarshalAs(UnmanagedType.LPStr)]string fileName);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        internal static extern void ListLoadedTextures(int textureType, DelegateStringFunc func);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern void ResetPlugin();

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern void ResetAllLoadedTextures();


#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        [DllImport(DRAW_OVER_WINDOW_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern IntPtr TestDraw(int posX, int posY);

        [DllImport(DRAW_OVER_WINDOW_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern void LoadAndShowBitMap(int posX, int posY, [MarshalAs(UnmanagedType.LPStr)]string fileName);

        [DllImport(DRAW_OVER_WINDOW_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern void ShowOverwrapWindow(int sInstanceID, int posX, int posY, int sWidth, int sHeight, int forceDraw);

        [DllImport(DRAW_OVER_WINDOW_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern void HideOverwrapWindow(int sInstanceID);
        [DllImport(DRAW_OVER_WINDOW_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern void SetOverwrapWindowData(int sInstanceID, UInt32[] byteArray, int length );
        [DllImport(DRAW_OVER_WINDOW_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern void HideAllOverwrapWindows();

        [DllImport(DRAW_OVER_WINDOW_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern void SetAllAreLoaded(int sInstanceID,int flag);
        [DllImport(DRAW_OVER_WINDOW_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern int GetAllAreLoaded(int sInstanceID);

        [DllImport(DRAW_OVER_WINDOW_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern void ResetOverwrapWindows();

#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

        public static IntPtr TestDraw(int posX, int posY) { return IntPtr.Zero; }       
        public static void LoadAndShowBitMap(int posX, int posY, [MarshalAs(UnmanagedType.LPStr)]string fileName) { }
        public static void ShowOverwrapWindow(int sInstanceID, int posX, int posY, int sWidth, int sHeight, int forceDraw) { }
        public static void HideOverwrapWindow(int sInstanceID) { }
        public static void SetOverwrapWindowData(int sInstanceID, UInt32[] byteArray, int length ) { }
        public static void HideAllOverwrapWindows() { }
        public static void SetAllAreLoaded(int sInstanceID,int flag) {}
        public static int GetAllAreLoaded(int sInstanceID) { return 0; }
        public static void ResetOverwrapWindows() { }
#endif //Platform-dependent support


#endif //UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX


//----------------------------------------------------------------------------------------------------------------------
    }

}
