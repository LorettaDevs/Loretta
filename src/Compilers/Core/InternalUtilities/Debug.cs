// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace Loretta.Utilities
{
    internal static class RoslynDebug
    {
        [Conditional("DEBUG")]
        [DoesNotReturn]
        public static void Fail(string? message) =>
            Debug.Fail(message);

        [Conditional("DEBUG")]
        [DoesNotReturn]
        public static void Fail(string? message, string? detailMessage) =>
            Debug.Fail(message, detailMessage);

        /// <inheritdoc cref="RoslynDebug.Assert(bool)"/>
        [Conditional("DEBUG")]
        public static void Assert([DoesNotReturnIf(false)] bool b) =>
            Debug.Assert(b);

        /// <inheritdoc cref="RoslynDebug.Assert(bool, string)"/>
        [Conditional("DEBUG")]
        public static void Assert([DoesNotReturnIf(false)] bool b, string message) =>
            Debug.Assert(b, message);

        [Conditional("DEBUG")]
        public static void AssertNotNull<T>([NotNull] T value) =>
            Assert(value is object, "Unexpected null reference");
    }
}
