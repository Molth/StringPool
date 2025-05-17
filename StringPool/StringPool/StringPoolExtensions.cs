// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable ALL

namespace System.Buffers
{
    /// <summary>
    ///     Provides extensions for StringPool.
    /// </summary>
    public static class StringPoolExtensions
    {
        /// <summary>
        ///     Retrieves a buffer that is at least the requested length.
        /// </summary>
        /// <param name="pool">The <see cref="StringPool" /> instance to rent from.</param>
        /// <param name="buffer">The buffer of the array needed.</param>
        /// <returns>
        ///     An array that is at least <paramref name="buffer" /> 's length.
        /// </returns>
        /// <remarks>
        ///     This buffer is loaned to the caller and should be returned to the same pool via
        ///     <see cref="StringPool.Return" /> so that it may be reused in subsequent usage of <see cref="Rent" />.
        ///     It is not a fatal error to not return a rented buffer, but failure to do so may lead to
        ///     decreased application performance, as the pool may need to create a new buffer to replace
        ///     the one lost.
        /// </remarks>
        public static string Rent(this StringPool pool, ReadOnlySpan<char> buffer)
        {
            string array = pool.Rent(buffer.Length);

            Span<char> span = StringPool.AsSpan(array);
            buffer.CopyTo(span);

            return array;
        }
    }
}