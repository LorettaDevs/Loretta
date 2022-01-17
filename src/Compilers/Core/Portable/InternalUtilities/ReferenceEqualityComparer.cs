// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Loretta.Utilities
{
    /// <summary>
    /// Compares objects based upon their reference identity.
    /// </summary>
    internal class ReferenceEqualityComparer : IEqualityComparer<object?>
    {
        public static readonly ReferenceEqualityComparer Instance = new();

        private ReferenceEqualityComparer()
        {
        }

        bool IEqualityComparer<object?>.Equals(object? a, object? b) => a == b;

        int IEqualityComparer<object?>.GetHashCode(object? a) => GetHashCode(a);

        public static int GetHashCode(object? a) =>
            // https://github.com/dotnet/roslyn/issues/41539
            RuntimeHelpers.GetHashCode(a!);
    }
}
