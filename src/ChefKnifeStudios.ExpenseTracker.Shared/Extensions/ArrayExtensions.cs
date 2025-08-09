using DocumentFormat.OpenXml.EMMA;
using System.Runtime.InteropServices;

namespace System;

public static class ArrayExtensions
{
    public static float[] BytesToFloats(this byte[] bytes)
    {
        return MemoryMarshal.Cast<byte, float>(bytes.AsSpan()).ToArray();
    }

    public static byte[] FloatsToBytes(this float[] floats)
    {
        return MemoryMarshal.AsBytes<float>(floats).ToArray();
    }
}
