using System;
using System.Runtime.InteropServices;

namespace UnityEngine.StreamingImageSequence {
//----------------------------------------------------------------------------------------------------------------------

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 8)]
internal struct ReadResult {
    public IntPtr Buffer;
    [MarshalAs(UnmanagedType.I4)]
    public readonly int Width;
    [MarshalAs(UnmanagedType.I4)]
    public readonly int Height;
    [MarshalAs(UnmanagedType.I4)]
    public readonly int ReadStatus;
};


} // end namespace
