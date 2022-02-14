// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis.Lua
{
    public static partial class SyntaxFacts
    {
        private sealed class SyntaxKindEqualityComparer : IEqualityComparer<SyntaxKind>
        {
            public bool Equals(SyntaxKind x, SyntaxKind y) => x == y;

            public int GetHashCode(SyntaxKind obj) => (int) obj;
        }

        /// <summary>
        /// A custom equality comparer for <see cref="SyntaxKind"/>
        /// </summary>
        /// <remarks>
        /// PERF: The framework specializes EqualityComparer for enums, but only if the underlying type is System.Int32
        /// Since SyntaxKind's underlying type is System.UInt16, ObjectEqualityComparer will be chosen instead.
        /// </remarks>
        public static IEqualityComparer<SyntaxKind> EqualityComparer { get; } = new SyntaxKindEqualityComparer();
    }
}
