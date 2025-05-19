// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

#pragma warning disable CS0169
#pragma warning disable CS1591
#pragma warning disable CS8602
#pragma warning disable CS8632

// ReSharper disable ALL

namespace System.Buffers
{
    /// <summary>
    ///     Represents a mutable string backed by a pooled buffer for high-performance scenarios.
    ///     This type allows direct manipulation of string data while reusing pooled memory buffers.
    ///     Implements <see cref="IDisposable" /> to return buffers to the pool and <see cref="IEquatable{T}" /> for
    ///     comparisons.
    /// </summary>
    /// <remarks>
    ///     The underlying string buffer's length is modified via unsafe operations to enable mutable-like behavior.
    ///     Callers must ensure proper disposal to maintain pool integrity.
    /// </remarks>
    public sealed class UnsafeString : IDisposable, IEquatable<string>, IEquatable<UnsafeString>
    {
        /// <summary>
        ///     The pooled string buffer. May be null after disposal or if uninitialized.
        ///     The buffer's internal length field is modified during operations.
        /// </summary>
        private string? _array;

        /// <summary>
        ///     The original length of the pooled buffer when rented from the string pool.
        ///     Used to restore the buffer's original state before returning it to the pool.
        /// </summary>
        private int _originalLength;

        /// <summary>
        ///     Releases the underlying buffer back to the string pool and resets internal state.
        /// </summary>
        /// <remarks>
        ///     Restores the original buffer length before returning it to ensure consistent pool behavior.
        ///     Subsequent operations will throw <see cref="NullReferenceException" /> if used after disposal.
        /// </remarks>
        public void Dispose()
        {
            string? array = _array;
            if (array == null)
                return;

            _array = null;

            // Restore original length via unsafe manipulation
            ref DummyString dummy = ref Unsafe.As<string, DummyString>(ref array);
            dummy._stringLength = _originalLength;
            StringPool.Shared.Return(array);

            _originalLength = 0;
        }

        /// <summary>
        ///     Determines reference equality between this instance's buffer and a string.
        /// </summary>
        public bool Equals(string? other) => _array == other;

        /// <summary>
        ///     Determines reference equality between this instance's buffer and another <see cref="UnsafeString" />'s buffer.
        /// </summary>
        public bool Equals(UnsafeString? other) => other != null && Equals(other._array);

        /// <summary>
        ///     Determines equality with any object, supporting both string and <see cref="UnsafeString" /> comparisons.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is string str)
                return Equals(str);

            if (obj is UnsafeString other)
                return Equals(other._array);

            return false;
        }

        /// <summary>
        ///     Returns the hash code of the underlying string buffer.
        /// </summary>
        public override int GetHashCode() => _array?.GetHashCode() ?? 0;

        /// <summary>
        ///     Returns the underlying string buffer or null if disposed.
        /// </summary>
        public override string? ToString() => _array;

        /// <summary>
        ///     Implicitly converts the <see cref="UnsafeString" /> into its underlying string buffer.
        /// </summary>
        public static implicit operator string?(UnsafeString? @string) => @string?._array;

        /// <summary>
        ///     Copies characters into the underlying buffer, resizing if necessary.
        /// </summary>
        /// <param name="buffer">The source character sequence to copy</param>
        /// <returns>
        ///     true if a new buffer was allocated during this operation,
        ///     false if existing capacity was sufficient
        /// </returns>
        public bool SetText(ReadOnlySpan<char> buffer)
        {
            bool bufferChanged = SetLength(buffer.Length, false);

            Span<char> destination = StringPool.AsSpan(_array!);
            buffer.CopyTo(destination);

            return bufferChanged;
        }

        /// <summary>
        ///     Adjusts the logical length of the string, potentially allocating a new buffer.
        /// </summary>
        /// <param name="length">The new logical length</param>
        /// <param name="copyIfGrow">
        ///     When true, preserves existing content when upgrading to a larger buffer.
        ///     Has no effect when shrinking or using existing capacity.
        /// </param>
        /// <returns>
        ///     true if a new buffer was allocated,
        ///     false if existing capacity was sufficient
        /// </returns>
        /// <exception cref="ArgumentOutOfRangeException">
        ///     Thrown if <paramref name="length" /> is negative
        /// </exception>
        public bool SetLength(int length, bool copyIfGrow = true)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length), "Must be non-negative");

            bool bufferChanged = false;
            string? array = _array;

            // Handle empty string initialization
            if (length == 0)
            {
                if (array == null)
                {
                    bufferChanged = true;
                    _array = string.Empty;
                }

                return bufferChanged;
            }

            // Reallocate if buffer is null or insufficient
            if (array == null || _originalLength < length)
            {
                bufferChanged = true;
                string newArray = StringPool.Shared.Rent(length);

                if (array != null)
                {
                    if (copyIfGrow)
                    {
                        ReadOnlySpan<char> source = array.AsSpan();
                        Span<char> destination = StringPool.AsSpan(newArray);
                        source.CopyTo(destination);
                    }

                    // Restore original length before returning old buffer
                    ref DummyString dummy = ref Unsafe.As<string, DummyString>(ref array);
                    dummy._stringLength = _originalLength;
                    StringPool.Shared.Return(array);
                }

                _array = array = newArray;
                _originalLength = array.Length;
            }

            // Update logical length via unsafe manipulation
            ref DummyString newDummy = ref Unsafe.As<string, DummyString>(ref array);
            newDummy._stringLength = length;

            return bufferChanged;
        }

        /// <summary>
        ///     Mimics the internal layout of System.String for unsafe field manipulation.
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private sealed class DummyString
        {
            /// <summary>
            ///     Maps to System.String._stringLength for unsafe length modification.
            /// </summary>
            public int _stringLength;

            /// <summary>
            ///     Maps to System.String._firstChar (unused but required for layout matching).
            /// </summary>
            private char _firstChar;
        }
    }
}