// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Loretta.Utilities
{
    /// <summary>
    /// Very cheap trivial comparer that never matches the keys,
    /// should only be used in empty dictionaries.
    /// </summary>
    internal sealed class EmptyComparer : IEqualityComparer<object>
    {
        public static readonly EmptyComparer Instance = new EmptyComparer();

        private EmptyComparer()
        {
        }

        bool IEqualityComparer<object>.Equals(object? a, object? b)
        {
            RoslynDebug.Assert(false, "Are we using empty comparer with nonempty dictionary?");
            return false;
        }

        int IEqualityComparer<object>.GetHashCode(object s)
        {
            // dictionary will call this often
            return 0;
        }
    }
}
