﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.Utilities
{
    internal static class ImmutableArrayExtensions
    {
        internal static ImmutableArray<T> ToImmutableArrayOrEmpty<T>(this IEnumerable<T>? items)
            => items == null ? ImmutableArray<T>.Empty : ImmutableArray.CreateRange(items);

        internal static ImmutableArray<T> ToImmutableArrayOrEmpty<T>(this ImmutableArray<T> items)
            => items.IsDefault ? ImmutableArray<T>.Empty : items;

        // same as Array.BinarySearch but the ability to pass arbitrary value to the comparer without allocation
        internal static int BinarySearch<TElement, TValue>(this ImmutableArray<TElement> array, TValue value, Func<TElement, TValue, int> comparer)
        {
            int low = 0;
            int high = array.Length - 1;

            while (low <= high)
            {
                int middle = low + ((high - low) >> 1);
                int comparison = comparer(array[middle], value);

                if (comparison == 0)
                {
                    return middle;
                }

                if (comparison > 0)
                {
                    high = middle - 1;
                }
                else
                {
                    low = middle + 1;
                }
            }

            return ~low;
        }

        internal static ImmutableArray<TDerived> CastDown<TOriginal, TDerived>(this ImmutableArray<TOriginal> array) where TDerived : class, TOriginal =>
            array.CastArray<TDerived>();
    }
}
