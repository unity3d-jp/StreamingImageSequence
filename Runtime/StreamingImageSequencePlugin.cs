using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;

namespace UnityEngine.StreamingImageSequence
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 8)]
    public struct ReadResult
    {
        public IntPtr Buffer;
        [MarshalAs(UnmanagedType.I4)]
        public int Width;
        [MarshalAs(UnmanagedType.I4)]
        public int Height;
        [MarshalAs(UnmanagedType.I4)]
        public int ReadStatus;
    };

    public enum LoadStatus
    {
        Uninitialized,
        Requested,
        Loaded,
    };

//----------------------------------------------------------------------------------------------------------------------
    public static class StreamingImageSequencePlugin {

//only support Windows and OSX
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX


#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        private const string LOADER_DLL             = "Loader";
        private const string DRAWER_DLL             = "Drawer";
        private const string DRAW_OVER_WINDOW_DLL   = "DrawOverWindow";
#elif UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
        private const string LOADER_DLL             = "Project";
        private const string DRAWER_DLL             = "Project";
        private const string DRAW_OVER_WINDOW_DLL   = "Project";
#endif

        // Implemented in Loader dll
        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static IntPtr LoadAndAlloc([MarshalAs(UnmanagedType.LPStr)]string fileName);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static void NativeFree(IntPtr ptr);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static IntPtr GetNativTextureInfo([MarshalAs(UnmanagedType.LPStr)]string fileName, out ReadResult tResult);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static int ResetNativeTexture([MarshalAs(UnmanagedType.LPStr)]string fileName);

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static void ResetPlugin();

        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static void  DoneResetPlugin();
        [DllImport(LOADER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static int   IsPluginResetting();

        // Implemented in Drawer dll
        [DllImport(DRAWER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static void SetNativeTexturePtr(IntPtr Texture, UInt32 uWidth, UInt32 height, Int32 sObjectID);

        [DllImport(DRAWER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static void SetLoadedTexture([MarshalAs(UnmanagedType.LPStr)]string fileName, Int32 sObjectID);

        [DllImport(DRAWER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static void ResetLoadedTexture(Int32 sObjectID);

        [DllImport(DRAWER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public extern static void ResetAllLoadedTexture();

        [DllImport(DRAWER_DLL, CharSet = CharSet.Unicode, ExactSpelling = true)]
        public static extern IntPtr GetRenderEventFunc();

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

        public static  IntPtr TestDraw(int posX, int posY) { return IntPtr.Zero; }       
        public static  void LoadAndShowBitMap(int posX, int posY, [MarshalAs(UnmanagedType.LPStr)]string fileName) { }
        public static  void ShowOverwrapWindow(int sInstanceID, int posX, int posY, int sWidth, int sHeight, int forceDraw) { }
        public static  void HideOverwrapWindow(int sInstanceID) { }
        public static  void SetOverwrapWindowData(int sInstanceID, UInt32[] byteArray, int length ) { }
        public static  void HideAllOverwrapWindows() { }
        public static  void SetAllAreLoaded(int sInstanceID,int flag) {}
        public static  int GetAllAreLoaded(int sInstanceID) { return 0; }
        public static  void ResetOverwrapWindows() { }
#endif


#endif //UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX

    }

}
