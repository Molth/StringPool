// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.InteropServices;

#pragma warning disable CS0169
#pragma warning disable CS1591
#pragma warning disable CS8602
#pragma warning disable CS8632

// ReSharper disable ALL

namespace System.Buffers
{
    /// <summary>
    ///     Mimics the internal layout of System.String for unsafe field manipulation.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    internal sealed class DummyString
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