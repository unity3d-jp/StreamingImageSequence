using System;
using System.Runtime.InteropServices;

namespace Unity.StreamingImageSequence {
//----------------------------------------------------------------------------------------------------------------------

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 8)]
internal struct ImageData {
    public IntPtr RawData;
    [MarshalAs(UnmanagedType.I4)]
    public readonly int Width;
    [MarshalAs(UnmanagedType.I4)]
    public readonly int Height;
    [MarshalAs(UnmanagedType.I4)]
    public readonly int ReadStatus;
    [MarshalAs(UnmanagedType.I4)]
    public readonly int Format;

    internal ImageData(int readStatus) {
        RawData    = IntPtr.Zero;
        Width      = 0;
        Height     = 0;        
        ReadStatus = readStatus;
        Format     = StreamingImageSequenceConstants.IMAGE_FORMAT_RGBA32;

    }
};


} // end namespace