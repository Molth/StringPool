// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// ReSharper disable ALL

namespace System.Buffers
{
    internal static class Utilities
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string Create(int length)
        {
            string array = new string((char)0, length);

            // Explicitly clear the string to ensure all characters are zeroed,
            // preventing any potential platform-specific default values.
            Clear(array);

            return array;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Clear(string array)
        {
            ReadOnlySpan<char> readOnlySpan = array.AsSpan();
            Span<char> span = MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(readOnlySpan), readOnlySpan.Length);
            span.Clear();
        }

        private static unsafe delegate* managed<double> _getMemoryPressure;

        public static unsafe void Custom(delegate* managed<double> getMemoryPressure) => _getMemoryPressure = getMemoryPressure;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int SelectBucketIndex(int bufferSize)
        {
            // Buffers are bucketed so that a request between 2^(n-1) + 1 and 2^n is given a buffer of 2^n
            // Bucket index is log2(bufferSize - 1) with the exception that buffers between 1 and 16 bytes
            // are combined, and the index is slid down by 3 to compensate.
            // Zero is a valid bufferSize, and it is assigned the highest bucket index so that zero-length
            // buffers are not retained by the pool. The pool will return the Array.Empty singleton for these.
            return BitOperationsHelpers.Log2((uint)bufferSize - 1 | 15) - 3;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int GetMaxSizeForBucket(int binIndex)
        {
            int maxSize = 16 << binIndex;
            Debug.Assert(maxSize >= 0);
            return maxSize;
        }

        public enum MemoryPressure
        {
            Low,
            Medium,
            High
        }

        public static unsafe MemoryPressure GetMemoryPressure()
        {
            const double HighPressureThreshold = .90; // Percent of GC memory pressure threshold we consider "high"
            const double MediumPressureThreshold = .70; // Percent of GC memory pressure threshold we consider "medium"

            double t;

            delegate*<double> getMemoryPressure = _getMemoryPressure;
            if (getMemoryPressure != null)
            {
                t = getMemoryPressure();
                goto label;
            }

#if NETCOREAPP3_0_OR_GREATER
            GCMemoryInfo memoryInfo = GC.GetGCMemoryInfo();
            t = memoryInfo.MemoryLoadBytes / (double)memoryInfo.HighMemoryLoadThresholdBytes;
#else
            long workingSet = Environment.WorkingSet;
            if (workingSet == 0)
                return MemoryPressure.Low;

            t = GC.GetTotalMemory(false) / (double)workingSet;
#endif

            label:

            if (t >= HighPressureThreshold)
            {
                return MemoryPressure.High;
            }

            if (t >= MediumPressureThreshold)
            {
                return MemoryPressure.Medium;
            }

            return MemoryPressure.Low;
        }
    }
}