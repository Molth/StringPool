// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
#if NET5_0_OR_GREATER
using System.Numerics;
#else
using System.Runtime.InteropServices;
#endif

// ReSharper disable ALL

namespace System.Buffers
{
    internal static class BitOperationsHelpers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Log2(ulong value)
        {
#if NET5_0_OR_GREATER
            return BitOperations.Log2(value);
#else
            value |= 1UL;
            uint num = (uint)(value >> 32);
            return num == 0U ? Log2((uint)value) : 32 + Log2(num);
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Log2(uint value)
        {
#if NET5_0_OR_GREATER
            return BitOperations.Log2(value);
#else
            value |= 1;
            value |= value >> 1;
            value |= value >> 2;
            value |= value >> 4;
            value |= value >> 8;
            value |= value >> 16;
            return Unsafe.AddByteOffset(ref MemoryMarshal.GetReference(Log2DeBruijn), (nint)(int)((value * 130329821U) >> 27));
#endif
        }

#if !NET5_0_OR_GREATER
        private static ReadOnlySpan<byte> Log2DeBruijn => new byte[32]
        {
            0, 9, 1, 10, 13, 21, 2, 29,
            11, 14, 16, 18, 22, 25, 3, 30,
            8, 12, 20, 28, 15, 17, 24, 7,
            19, 27, 23, 6, 26, 5, 4, 31
        };
#endif
    }
}