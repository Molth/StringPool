// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

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
    public struct UnsafeValueString : IDisposable, IEquatable<string>, IEquatable<UnsafeValueString>, IEquatable<UnsafeString>
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
        ///     Determines reference equality between this instance's buffer and another <see cref="UnsafeValueString" />'s buffer.
        /// </summary>
        public bool Equals(UnsafeValueString other) => (string?)this == (string?)other;

        /// <summary>
        ///     Determines reference equality between this instance's buffer and another <see cref="UnsafeString" />'s buffer.
        /// </summary>
        public bool Equals(UnsafeString? other) => (string?)this == (string?)other;

        /// <summary>
        ///     Determines equality with any object, supporting both string and <see cref="UnsafeValueString" /> comparisons.
        /// </summary>
        public override bool Equals(object? obj)
        {
            if (obj is string str)
                return (string?)this == str;

            if (obj is UnsafeValueString other)
                return (string?)this == (string?)other;

            if (obj is UnsafeString @string)
                return (string?)this == (string?)@string;

            return false;
        }

        /// <summary>
        ///     Returns the hash code of the underlying string buffer.
        /// </summary>
        public override int GetHashCode() => ((string?)this)?.GetHashCode() ?? 0;

        /// <summary>
        ///     Returns the underlying string buffer or null if disposed.
        /// </summary>
        public override string? ToString() => _array;

        /// <summary>
        ///     Copies characters into the underlying buffer, resizing if necessary.
        /// </summary>
        /// <param name="buffer">The source character sequence to copy</param>
        /// <returns>
        ///     true if a new buffer was rented during this operation,
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
        ///     Adjusts the logical length of the string, potentially renting a new buffer.
        /// </summary>
        /// <param name="length">The new logical length</param>
        /// <param name="copyIfGrow">
        ///     When true, preserves existing content when upgrading to a larger buffer.
        ///     Has no effect when shrinking or using existing capacity.
        /// </param>
        /// <returns>
        ///     true if a new buffer was rented,
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

            // Re-rent if buffer is null or insufficient
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
        ///     Implicitly converts the <see cref="UnsafeValueString" /> into its underlying string buffer.
        /// </summary>
        public static implicit operator string?(UnsafeValueString @string) => @string._array;

        /// <summary>
        ///     Determines whether two <see cref="UnsafeValueString" /> instances reference the same underlying buffer.
        /// </summary>
        /// <param name="a">The first <see cref="UnsafeValueString" /> to compare</param>
        /// <param name="b">The second <see cref="UnsafeValueString" /> to compare</param>
        /// <returns>
        ///     true if both instances reference the same underlying buffer or are both null;
        ///     otherwise, false
        /// </returns>
        public static bool operator ==(UnsafeValueString a, UnsafeValueString b) => (string?)a == (string?)b;

        /// <summary>
        ///     Determines whether two <see cref="UnsafeValueString" /> instances reference different underlying buffers.
        /// </summary>
        /// <param name="a">The first <see cref="UnsafeValueString" /> to compare</param>
        /// <param name="b">The second <see cref="UnsafeValueString" /> to compare</param>
        /// <returns>
        ///     true if the instances reference different underlying buffers or if one is null while the other is not;
        ///     otherwise, false
        /// </returns>
        public static bool operator !=(UnsafeValueString a, UnsafeValueString b) => (string?)a != (string?)b;

        /// <summary>
        ///     Determines whether an <see cref="UnsafeValueString" /> and a string reference the same underlying buffer.
        /// </summary>
        /// <param name="a">The <see cref="UnsafeValueString" /> to compare</param>
        /// <param name="b">The string to compare</param>
        /// <returns>
        ///     true if the <see cref="UnsafeValueString" />'s buffer and the string are reference equal or both null;
        ///     otherwise, false
        /// </returns>
        public static bool operator ==(UnsafeValueString a, string? b) => (string?)a == b;

        /// <summary>
        ///     Determines whether an <see cref="UnsafeValueString" /> and a string reference different underlying buffers.
        /// </summary>
        /// <param name="a">The <see cref="UnsafeValueString" /> to compare</param>
        /// <param name="b">The string to compare</param>
        /// <returns>
        ///     true if the <see cref="UnsafeValueString" />'s buffer and the string are not reference equal or if one is null
        ///     while the
        ///     other is not;
        ///     otherwise, false
        /// </returns>
        public static bool operator !=(UnsafeValueString a, string? b) => (string?)a != b;

        /// <summary>
        ///     Determines whether a string and an <see cref="UnsafeValueString" /> reference the same underlying buffer.
        /// </summary>
        /// <param name="a">The string to compare</param>
        /// <param name="b">The <see cref="UnsafeValueString" /> to compare</param>
        /// <returns>
        ///     true if the string and the <see cref="UnsafeValueString" />'s buffer are reference equal or both null;
        ///     otherwise, false
        /// </returns>
        public static bool operator ==(string? a, UnsafeValueString b) => a == (string?)b;

        /// <summary>
        ///     Determines whether a string and an <see cref="UnsafeValueString" /> reference different underlying buffers.
        /// </summary>
        /// <param name="a">The string to compare</param>
        /// <param name="b">The <see cref="UnsafeValueString" /> to compare</param>
        /// <returns>
        ///     true if the string and the <see cref="UnsafeValueString" />'s buffer are not reference equal or if one is null
        ///     while the
        ///     other is not;
        ///     otherwise, false
        /// </returns>
        public static bool operator !=(string? a, UnsafeValueString b) => a != (string?)b;

        /// <summary>
        ///     Determines whether an <see cref="UnsafeValueString" /> and an <see cref="UnsafeString" /> reference the same
        ///     underlying buffer.
        /// </summary>
        /// <param name="a">The <see cref="UnsafeValueString" /> to compare</param>
        /// <param name="b">The <see cref="UnsafeString" /> to compare</param>
        /// <returns>
        ///     true if both instances reference the same underlying buffer or are both null;
        ///     otherwise, false
        /// </returns>
        public static bool operator ==(UnsafeValueString a, UnsafeString b) => (string?)a == (string?)b;

        /// <summary>
        ///     Determines whether an <see cref="UnsafeValueString" /> and an <see cref="UnsafeString" /> reference different
        ///     underlying buffers.
        /// </summary>
        /// <param name="a">The <see cref="UnsafeValueString" /> to compare</param>
        /// <param name="b">The <see cref="UnsafeString" /> to compare</param>
        /// <returns>
        ///     true if the instances reference different underlying buffers or if one is null while the other is not;
        ///     otherwise, false
        /// </returns>
        public static bool operator !=(UnsafeValueString a, UnsafeString b) => (string?)a != (string?)b;
    }
}