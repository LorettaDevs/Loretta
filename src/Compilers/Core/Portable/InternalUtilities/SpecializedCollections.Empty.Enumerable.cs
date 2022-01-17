// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.Generic;

namespace Loretta.Utilities
{
    internal partial class SpecializedCollections
    {
        private partial class Empty
        {
            internal class Enumerable<T> : IEnumerable<T>
            {
                // PERF: cache the instance of enumerator. 
                // accessing a generic static field is kinda slow from here,
                // but since empty enumerables are singletons, there is no harm in having 
                // one extra instance field
                private readonly IEnumerator<T> _enumerator = Enumerator<T>.Instance;

                public IEnumerator<T> GetEnumerator() => _enumerator;

                System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
            }
        }
    }
}
