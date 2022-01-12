// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Loretta.Utilities
{
    internal static class LorettaDebug
    {
        [Conditional("DEBUG")]
        [DoesNotReturn]
#pragma warning disable CS8763 // A method marked [DoesNotReturn] should not return.
        public static void Fail(string? message) =>
            Debug.Fail(message);
#pragma warning restore CS8763 // A method marked [DoesNotReturn] should not return.

        [Conditional("DEBUG")]
        [DoesNotReturn]
#pragma warning disable CS8763 // A method marked [DoesNotReturn] should not return.
        public static void Fail(string? message, string? detailMessage) =>
            Debug.Fail(message, detailMessage);
#pragma warning restore CS8763 // A method marked [DoesNotReturn] should not return.

        /// <inheritdoc cref="LorettaDebug.Assert(bool)"/>
        [Conditional("DEBUG")]
        public static void Assert([DoesNotReturnIf(false)] bool b) =>
            Debug.Assert(b);

        /// <inheritdoc cref="LorettaDebug.Assert(bool, string)"/>
        [Conditional("DEBUG")]
        public static void Assert([DoesNotReturnIf(false)] bool b, string message) =>
            Debug.Assert(b, message);

        [Conditional("DEBUG")]
        public static void AssertNotNull<T>([NotNull] T value) =>
            Assert(value is object, "Unexpected null reference");
    }
}
