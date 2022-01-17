// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections;

namespace Loretta.Utilities
{
    internal partial class SpecializedCollections
    {
        private partial class ReadOnly
        {
            internal class Enumerable<TUnderlying> : IEnumerable
                where TUnderlying : IEnumerable
            {
                protected readonly TUnderlying Underlying;

                public Enumerable(TUnderlying underlying)
                {
                    Underlying = underlying;
                }

                public IEnumerator GetEnumerator() => Underlying.GetEnumerator();
            }
        }
    }
}
