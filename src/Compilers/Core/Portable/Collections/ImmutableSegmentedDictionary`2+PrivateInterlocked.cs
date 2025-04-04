// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Runtime.CompilerServices;

namespace Loretta.CodeAnalysis.Collections
{
    internal readonly partial struct ImmutableSegmentedDictionary<TKey, TValue>
    {
        /// <summary>
        /// Private helper class for use only by <see cref="RoslynImmutableInterlocked"/>.
        /// </summary>
        internal static class PrivateInterlocked
        {
            internal static ImmutableSegmentedDictionary<TKey, TValue> VolatileRead(
                in ImmutableSegmentedDictionary<TKey, TValue> location)
            {
                var dictionary = Volatile.Read(ref Unsafe.AsRef(in location._dictionary));

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse // Don't know enough to ensure it's like this.
                if (dictionary is null) return default(ImmutableSegmentedDictionary<TKey, TValue>);

                return new ImmutableSegmentedDictionary<TKey, TValue>(dictionary);
            }

            internal static ImmutableSegmentedDictionary<TKey, TValue> InterlockedExchange(
                ref ImmutableSegmentedDictionary<TKey, TValue> location,
                ImmutableSegmentedDictionary<TKey, TValue>     value)
            {
                var dictionary = Interlocked.Exchange(ref Unsafe.AsRef(in location._dictionary), value._dictionary);

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse // Don't know enough to ensure it's like this.
                if (dictionary is null) return default(ImmutableSegmentedDictionary<TKey, TValue>);

                return new ImmutableSegmentedDictionary<TKey, TValue>(dictionary);
            }

            internal static ImmutableSegmentedDictionary<TKey, TValue> InterlockedCompareExchange(
                ref ImmutableSegmentedDictionary<TKey, TValue> location,
                ImmutableSegmentedDictionary<TKey, TValue>     value,
                ImmutableSegmentedDictionary<TKey, TValue>     comparand)
            {
                var dictionary = Interlocked.CompareExchange(
                    ref Unsafe.AsRef(in location._dictionary),
                    value._dictionary,
                    comparand._dictionary);

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse // Don't know enough to ensure it's like this.
                if (dictionary is null) return default(ImmutableSegmentedDictionary<TKey, TValue>);

                return new ImmutableSegmentedDictionary<TKey, TValue>(dictionary);
            }
        }
    }
}
