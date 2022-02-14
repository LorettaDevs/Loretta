// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.Utilities
{
    internal partial class SpecializedCollections
    {
        private static partial class ReadOnly
        {
            internal class Collection<TUnderlying, T> : Enumerable<TUnderlying, T>, ICollection<T>
                where TUnderlying : ICollection<T>
            {
                public Collection(TUnderlying underlying)
                    : base(underlying)
                {
                }

                public void Add(T item) => throw new NotSupportedException();

                public void Clear() => throw new NotSupportedException();

                public bool Contains(T item) => Underlying.Contains(item);

                public void CopyTo(T[] array, int arrayIndex) => Underlying.CopyTo(array, arrayIndex);

                public int Count => Underlying.Count;

                public bool IsReadOnly => true;

                public bool Remove(T item) => throw new NotSupportedException();
            }
        }
    }
}
