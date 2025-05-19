// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;
using System.Threading;

// ReSharper disable ALL

namespace System.Buffers
{
    /// <summary>
    ///     Provides a resource pool that enables reusing instances of arrays.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Renting and returning buffers with an <see cref="StringPool" /> can increase performance
    ///         in situations where arrays are created and destroyed frequently, resulting in significant
    ///         memory pressure on the garbage collector.
    ///     </para>
    ///     <para>
    ///         This class is thread-safe.  All members may be used by multiple threads concurrently.
    ///     </para>
    ///     <para>
    ///         Environment variables configuration:
    ///     </para>
    ///     <list type="table">
    ///         <listheader>
    ///             <term>Variable</term>
    ///             <description>Description</description>
    ///         </listheader>
    ///         <item>
    ///             <term>DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXSTRINGSPERPARTITION</term>
    ///             <description>Maximum strings per partition (default: 256)</description>
    ///         </item>
    ///         <item>
    ///             <term>DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXPARTITIONCOUNT</term>
    ///             <description>
    ///                 Maximum number of partitions (default: <see cref="int.MaxValue" />, clamped to
    ///                 <see cref="Environment.ProcessorCount" />)
    ///             </description>
    ///         </item>
    ///     </list>
    /// </remarks>
    public abstract class StringPool
    {
        // Store the shared StringPool in a field of its derived sealed type so the Jit can "see" the exact type
        // when the Shared property is inlined which will allow it to devirtualize calls made on it.
        private static readonly SharedArrayPool s_shared = new SharedArrayPool();

        /// <summary>
        ///     Retrieves a shared <see cref="StringPool" /> instance.
        /// </summary>
        /// <remarks>
        ///     The shared pool provides a default implementation of <see cref="StringPool" />
        ///     that's intended for general applicability.  It maintains arrays of multiple sizes, and
        ///     may hand back a larger array than was actually requested, but will never hand back a smaller
        ///     array than was requested. Renting a buffer from it with <see cref="Rent" /> will result in an
        ///     existing buffer being taken from the pool if an appropriate buffer is available or in a new
        ///     buffer being allocated if one is not available.
        ///     The shared pool instance is created lazily on first access.
        /// </remarks>
        public static StringPool Shared => s_shared;

        /// <summary>
        ///     Creates a new <see cref="StringPool" /> instance using default configuration options.
        /// </summary>
        /// <returns>A new <see cref="StringPool" /> instance.</returns>
        public static StringPool Create() => new ConfigurableArrayPool();

        /// <summary>
        ///     Creates a new <see cref="StringPool" /> instance using custom configuration options.
        /// </summary>
        /// <param name="maxArrayLength">The maximum length of array instances that may be stored in the pool.</param>
        /// <param name="maxArraysPerBucket">
        ///     The maximum number of array instances that may be stored in each bucket in the pool.  The pool
        ///     groups arrays of similar lengths into buckets for faster access.
        /// </param>
        /// <returns>A new <see cref="StringPool" /> instance with the specified configuration options.</returns>
        /// <remarks>
        ///     The created pool will group arrays into buckets, with no more than <paramref name="maxArraysPerBucket" />
        ///     in each bucket and with those arrays not exceeding <paramref name="maxArrayLength" /> in length.
        /// </remarks>
        public static StringPool Create(int maxArrayLength, int maxArraysPerBucket) => new ConfigurableArrayPool(maxArrayLength, maxArraysPerBucket);

        /// <summary>
        ///     Retrieves a buffer that is at least the requested length.
        /// </summary>
        /// <param name="minimumLength">The minimum length of the array needed.</param>
        /// <returns>
        ///     An array that is at least <paramref name="minimumLength" /> in length.
        /// </returns>
        /// <remarks>
        ///     This buffer is loaned to the caller and should be returned to the same pool via
        ///     <see cref="Return" /> so that it may be reused in subsequent usage of <see cref="Rent" />.
        ///     It is not a fatal error to not return a rented buffer, but failure to do so may lead to
        ///     decreased application performance, as the pool may need to create a new buffer to replace
        ///     the one lost.
        /// </remarks>
        public abstract string Rent(int minimumLength);

        /// <summary>
        ///     Returns to the pool an array that was previously obtained via <see cref="Rent" /> on the same
        ///     <see cref="StringPool" /> instance.
        /// </summary>
        /// <param name="array">
        ///     The buffer previously obtained from <see cref="Rent" /> to return to the pool.
        /// </param>
        /// <param name="clearArray">
        ///     If <c>true</c> and if the pool will store the buffer to enable subsequent reuse, <see cref="Return" />
        ///     will clear <paramref name="array" /> of its contents so that a subsequent consumer via <see cref="Rent" />
        ///     will not see the previous consumer's content.  If <c>false</c> or if the pool will release the buffer,
        ///     the array's contents are left unchanged.
        /// </param>
        /// <remarks>
        ///     Once a buffer has been returned to the pool, the caller gives up all ownership of the buffer
        ///     and must not use it. The reference returned from a given call to <see cref="Rent" /> must only be
        ///     returned via <see cref="Return" /> once.  The default <see cref="StringPool" />
        ///     may hold onto the returned buffer in order to rent it again, or it may release the returned buffer
        ///     if it's determined that the pool already has enough buffers stored.
        /// </remarks>
        public abstract void Return(string array, bool clearArray = true);

        /// <summary>
        ///     Provides a way to customize the behavior of the pool by specifying a memory pressure callback.
        /// </summary>
        /// <param name="getMemoryPressure">
        ///     A function pointer to a managed method that returns the current memory pressure as a double value.
        ///     This callback is used by the pool to determine when to adjust its behavior based on system memory conditions.
        /// </param>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>&lt; 0.7 -> low</description>
        ///         </item>
        ///         <item>
        ///             <description>[0.7, 0.9) -> medium</description>
        ///         </item>
        ///         <item>
        ///             <description>&gt;= 0.9 -> high</description>
        ///         </item>
        ///     </list>
        ///     The pool will be more aggressive about releasing buffers when memory pressure reaches medium or high.
        /// </remarks>
        public static unsafe void Custom(Func<double> getMemoryPressure)
        {
            if (getMemoryPressure == null)
                throw new ArgumentNullException(nameof(getMemoryPressure));

            if (!getMemoryPressure.Method.IsStatic)
                throw new ArgumentException("MustBeStatic", nameof(getMemoryPressure));

            Custom((delegate* managed<double>)getMemoryPressure.Method.MethodHandle.GetFunctionPointer());
        }

        /// <summary>
        ///     Provides a way to customize the behavior of the pool by specifying a memory pressure callback.
        /// </summary>
        /// <param name="getMemoryPressure">
        ///     A function pointer to a managed method that returns the current memory pressure as a double value.
        ///     This callback is used by the pool to determine when to adjust its behavior based on system memory conditions.
        /// </param>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>&lt; 0.7 -> low</description>
        ///         </item>
        ///         <item>
        ///             <description>[0.7, 0.9) -> medium</description>
        ///         </item>
        ///         <item>
        ///             <description>&gt;= 0.9 -> high</description>
        ///         </item>
        ///     </list>
        ///     The pool will be more aggressive about releasing buffers when memory pressure reaches medium or high.
        /// </remarks>
        public static unsafe void Custom(nint getMemoryPressure) => Utilities.Custom((delegate* managed<double>)getMemoryPressure);

        /// <summary>
        ///     Provides a way to customize the behavior of the pool by specifying a memory pressure callback.
        /// </summary>
        /// <param name="getMemoryPressure">
        ///     A function pointer to a managed method that returns the current memory pressure as a double value.
        ///     This callback is used by the pool to determine when to adjust its behavior based on system memory conditions.
        /// </param>
        /// <remarks>
        ///     <list type="bullet">
        ///         <item>
        ///             <description>&lt; 0.7 -> low</description>
        ///         </item>
        ///         <item>
        ///             <description>[0.7, 0.9) -> medium</description>
        ///         </item>
        ///         <item>
        ///             <description>&gt;= 0.9 -> high</description>
        ///         </item>
        ///     </list>
        ///     The pool will be more aggressive about releasing buffers when memory pressure reaches medium or high.
        /// </remarks>
        public static unsafe void Custom(delegate* managed<double> getMemoryPressure) => Utilities.Custom(getMemoryPressure);

        /// <summary>
        ///     Configures the parameters for the shared <see cref="StringPool" /> instance before it is created.
        /// </summary>
        /// <param name="DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXSTRINGSPERPARTITION">
        ///     The maximum number of string instances that may be stored in each partition of the shared pool.
        /// </param>
        /// <param name="DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXPARTITIONCOUNT">
        ///     The maximum number of partitions in the shared pool.
        /// </param>
        /// <remarks>
        ///     <para>
        ///         This method must be called before the first access to <see cref="StringPool.Shared" /> property.
        ///         After the shared pool instance is created (on first access to <see cref="StringPool.Shared" />),
        ///         any subsequent calls to this method will have no effect.
        ///     </para>
        ///     <para>
        ///         These configuration values will be used when the shared pool is first created:
        ///         <list type="bullet">
        ///             <item>
        ///                 <description>Maximum strings per partition (default: 256)</description>
        ///             </item>
        ///             <item>
        ///                 <description>
        ///                     Maximum partition count (default: <see cref="int.MaxValue" />, clamped to
        ///                     <see cref="Environment.ProcessorCount" />)
        ///                 </description>
        ///             </item>
        ///         </list>
        ///     </para>
        /// </remarks>
        public static void Configure(int DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXSTRINGSPERPARTITION = 256, int DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXPARTITIONCOUNT = int.MaxValue)
        {
            if (DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXSTRINGSPERPARTITION <= 0)
                throw new ArgumentOutOfRangeException(nameof(DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXSTRINGSPERPARTITION), DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXSTRINGSPERPARTITION, "MustBePositive");

            if (DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXPARTITIONCOUNT <= 0)
                throw new ArgumentOutOfRangeException(nameof(DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXPARTITIONCOUNT), DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXPARTITIONCOUNT, "MustBePositive");

            Interlocked.Exchange(ref _DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXPARTITIONCOUNT, DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXPARTITIONCOUNT);
            Interlocked.Exchange(ref _DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXSTRINGSPERPARTITION, DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXSTRINGSPERPARTITION);

            Environment.SetEnvironmentVariable("DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXPARTITIONCOUNT", DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXPARTITIONCOUNT.ToString());
            Environment.SetEnvironmentVariable("DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXSTRINGSPERPARTITION", DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXSTRINGSPERPARTITION.ToString());
        }

        internal static int _DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXPARTITIONCOUNT;
        internal static int _DOTNET_SYSTEM_BUFFERS_SHAREDSTRINGPOOL_MAXSTRINGSPERPARTITION;

        /// <summary>
        ///     Creates a <see cref="Span{Char}" /> of the characters of a string.
        /// </summary>
        /// <param name="array">The string to create a <see cref="Span{Char}" />.</param>
        /// <returns>A <see cref="Span{Char}" /> representing the characters of the string.</returns>
        public static Span<char> AsSpan(string array)
        {
            ReadOnlySpan<char> span = array.AsSpan();
            return MemoryMarshal.CreateSpan(ref MemoryMarshal.GetReference(span), span.Length);
        }

        /// <summary>
        ///     Creates a <see cref="Memory{Char}" /> of the characters of a string.
        /// </summary>
        /// <param name="array">The string to create a <see cref="Memory{Char}" />.</param>
        /// <returns>A <see cref="Memory{Char}" /> representing the characters of the string.</returns>
        public static Memory<char> AsMemory(string array)
        {
            ReadOnlyMemory<char> memory = array.AsMemory();
            return MemoryMarshal.AsMemory(memory);
        }
    }
}