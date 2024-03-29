﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.Utilities
{
    internal partial class SpecializedCollections
    {
        private partial class ReadOnly
        {
            internal class Set<TUnderlying, T> : Collection<TUnderlying, T>, ISet<T>, IReadOnlySet<T>
                where TUnderlying : ISet<T>
            {
                public Set(TUnderlying underlying)
                    : base(underlying)
                {
                }

                public new bool Add(T item) => throw new NotSupportedException();

                public void ExceptWith(IEnumerable<T> other) => throw new NotSupportedException();

                public void IntersectWith(IEnumerable<T> other) => throw new NotSupportedException();

                public bool IsProperSubsetOf(IEnumerable<T> other) => Underlying.IsProperSubsetOf(other);

                public bool IsProperSupersetOf(IEnumerable<T> other) => Underlying.IsProperSupersetOf(other);

                public bool IsSubsetOf(IEnumerable<T> other) => Underlying.IsSubsetOf(other);

                public bool IsSupersetOf(IEnumerable<T> other) => Underlying.IsSupersetOf(other);

                public bool Overlaps(IEnumerable<T> other) => Underlying.Overlaps(other);

                public bool SetEquals(IEnumerable<T> other) => Underlying.SetEquals(other);

                public void SymmetricExceptWith(IEnumerable<T> other) => throw new NotSupportedException();

                public void UnionWith(IEnumerable<T> other) => throw new NotSupportedException();
            }
        }
    }
}
