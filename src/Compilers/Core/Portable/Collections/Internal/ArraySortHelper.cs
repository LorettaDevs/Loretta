// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// NOTE: This code is derived from an implementation originally in dotnet/runtime:
// https://github.com/dotnet/runtime/blob/v5.0.2/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/ArraySortHelper.cs
//
// See the commentary in https://github.com/dotnet/roslyn/pull/50156 for notes on incorporating changes made to the
// reference implementation.

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Loretta.Utilities;

#if NETCOREAPP
using System.Numerics;
#else
using System.Runtime.InteropServices;
#endif

#if !NET5_0 && !NET5_0_OR_GREATER
using Half = System.Single;
#endif

namespace Loretta.CodeAnalysis.Collections.Internal
{
    #region ArraySortHelper for single arrays

    internal static class SegmentedArraySortHelper<T>
    {
        public static void Sort(SegmentedArraySegment<T> keys, IComparer<T>? comparer)
        {
            // Add a try block here to detect IComparers (or their
            // underlying IComparables, etc) that are bogus.
            try
            {
                comparer ??= Comparer<T>.Default;
                IntrospectiveSort(keys, comparer.Compare);
            }
            catch (IndexOutOfRangeException)
            {
                ThrowHelper.ThrowArgumentException_BadComparer(comparer);
            }
            catch (Exception e)
            {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
            }
        }

        public static int BinarySearch(SegmentedArray<T> array, int index, int length, T value, IComparer<T>? comparer)
        {
            try
            {
                comparer ??= Comparer<T>.Default;
                return InternalBinarySearch(array, index, length, value, comparer);
            }
            catch (Exception e)
            {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
                return 0;
            }
        }

        internal static void Sort(SegmentedArraySegment<T> keys, Comparison<T> comparer)
        {
            LorettaDebug.Assert(comparer != null, "Check the arguments in the caller!");

            // Add a try block here to detect bogus comparisons
            try
            {
                IntrospectiveSort(keys, comparer);
            }
            catch (IndexOutOfRangeException)
            {
                ThrowHelper.ThrowArgumentException_BadComparer(comparer);
            }
            catch (Exception e)
            {
                ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_IComparerFailed, e);
            }
        }

        internal static int InternalBinarySearch(SegmentedArray<T> array, int index, int length, T value, IComparer<T> comparer)
        {
            LorettaDebug.Assert(index >= 0 && length >= 0 && (array.Length - index >= length), "Check the arguments in the caller!");

            int lo = index;
            int hi = index + length - 1;
            while (lo <= hi)
            {
                int i = lo + ((hi - lo) >> 1);
                int order = comparer.Compare(array[i], value);

                if (order == 0)
                    return i;
                if (order < 0)
                {
                    lo = i + 1;
                }
                else
                {
                    hi = i - 1;
                }
            }

            return ~lo;
        }

        private static void SwapIfGreater(SegmentedArraySegment<T> keys, Comparison<T> comparer, int i, int j)
        {
            LorettaDebug.Assert(i != j);

            if (comparer(keys[i], keys[j]) > 0)
            {
                T key = keys[i];
                keys[i] = keys[j];
                keys[j] = key;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void Swap(SegmentedArraySegment<T> a, int i, int j)
        {
            LorettaDebug.Assert(i != j);

            T t = a[i];
            a[i] = a[j];
            a[j] = t;
        }

        internal static void IntrospectiveSort(SegmentedArraySegment<T> keys, Comparison<T> comparer)
        {
            LorettaDebug.Assert(comparer != null);

            if (keys.Length > 1)
            {
                IntroSort(keys, 2 * (SegmentedArraySortUtils.Log2((uint) keys.Length) + 1), comparer);
            }
        }

        private static void IntroSort(SegmentedArraySegment<T> keys, int depthLimit, Comparison<T> comparer)
        {
            LorettaDebug.Assert(keys.Length > 0);
            LorettaDebug.Assert(depthLimit >= 0);
            LorettaDebug.Assert(comparer != null);

            int partitionSize = keys.Length;
            while (partitionSize > 1)
            {
                if (partitionSize <= SegmentedArrayHelper.IntrosortSizeThreshold)
                {

                    if (partitionSize == 2)
                    {
                        SwapIfGreater(keys, comparer, 0, 1);
                        return;
                    }

                    if (partitionSize == 3)
                    {
                        SwapIfGreater(keys, comparer, 0, 1);
                        SwapIfGreater(keys, comparer, 0, 2);
                        SwapIfGreater(keys, comparer, 1, 2);
                        return;
                    }

                    InsertionSort(keys.Slice(0, partitionSize), comparer);
                    return;
                }

                if (depthLimit == 0)
                {
                    HeapSort(keys.Slice(0, partitionSize), comparer);
                    return;
                }
                depthLimit--;

                int p = PickPivotAndPartition(keys.Slice(0, partitionSize), comparer);

                // Note we've already partitioned around the pivot and do not have to move the pivot again.
                IntroSort(keys.Slice(p + 1, partitionSize - (p + 1)), depthLimit, comparer);
                partitionSize = p;
            }
        }

        private static int PickPivotAndPartition(SegmentedArraySegment<T> keys, Comparison<T> comparer)
        {
            LorettaDebug.Assert(keys.Length >= SegmentedArrayHelper.IntrosortSizeThreshold);
            LorettaDebug.Assert(comparer != null);

            int hi = keys.Length - 1;

            // Compute median-of-three.  But also partition them, since we've done the comparison.
            int middle = hi >> 1;

            // Sort lo, mid and hi appropriately, then pick mid as the pivot.
            SwapIfGreater(keys, comparer, 0, middle);  // swap the low with the mid point
            SwapIfGreater(keys, comparer, 0, hi);   // swap the low with the high
            SwapIfGreater(keys, comparer, middle, hi); // swap the middle with the high

            T pivot = keys[middle];
            Swap(keys, middle, hi - 1);
            int left = 0, right = hi - 1;  // We already partitioned lo and hi and put the pivot in hi - 1.  And we pre-increment & decrement below.

            while (left < right)
            {
                while (comparer(keys[++left], pivot) < 0)
                {
                    // Intentionally empty
                }

                while (comparer(pivot, keys[--right]) < 0)
                {
                    // Intentionally empty
                }

                if (left >= right)
                    break;

                Swap(keys, left, right);
            }

            // Put pivot in the right location.
            if (left != hi - 1)
            {
                Swap(keys, left, hi - 1);
            }
            return left;
        }

        private static void HeapSort(SegmentedArraySegment<T> keys, Comparison<T> comparer)
        {
            LorettaDebug.Assert(comparer != null);
            LorettaDebug.Assert(keys.Length > 0);

            int n = keys.Length;
            for (int i = n >> 1; i >= 1; i--)
            {
                DownHeap(keys, i, n, 0, comparer);
            }

            for (int i = n; i > 1; i--)
            {
                Swap(keys, 0, i - 1);
                DownHeap(keys, 1, i - 1, 0, comparer);
            }
        }

        private static void DownHeap(SegmentedArraySegment<T> keys, int i, int n, int lo, Comparison<T> comparer)
        {
            LorettaDebug.Assert(comparer != null);
            LorettaDebug.Assert(lo >= 0);
            LorettaDebug.Assert(lo < keys.Length);

            T d = keys[lo + i - 1];
            while (i <= n >> 1)
            {
                int child = 2 * i;
                if (child < n && comparer(keys[lo + child - 1], keys[lo + child]) < 0)
                {
                    child++;
                }

                if (!(comparer(d, keys[lo + child - 1]) < 0))
                    break;

                keys[lo + i - 1] = keys[lo + child - 1];
                i = child;
            }

            keys[lo + i - 1] = d;
        }

        private static void InsertionSort(SegmentedArraySegment<T> keys, Comparison<T> comparer)
        {
            for (int i = 0; i < keys.Length - 1; i++)
            {
                T t = keys[i + 1];

                int j = i;
                while (j >= 0 && comparer(t, keys[j]) < 0)
                {
                    keys[j + 1] = keys[j];
                    j--;
                }

                keys[j + 1] = t;
            }
        }
    }

    #endregion

    /// <summary>Helper methods for use in array/span sorting routines.</summary>
    internal static class SegmentedArraySortUtils
    {
#if !NETCOREAPP
        private static ReadOnlySpan<byte> Log2DeBruijn => new byte[32]
        {
            00, 09, 01, 10, 13, 21, 02, 29,
            11, 14, 16, 18, 22, 25, 03, 30,
            08, 12, 20, 28, 15, 17, 24, 07,
            19, 27, 23, 06, 26, 05, 04, 31,
        };
#endif

        public static int MoveNansToFront<TKey, TValue>(SegmentedArraySegment<TKey> keys, Span<TValue> values) where TKey : notnull
        {
            LorettaDebug.Assert(typeof(TKey) == typeof(double) || typeof(TKey) == typeof(float));

            int left = 0;

            for (int i = 0; i < keys.Length; i++)
            {
                if ((typeof(TKey) == typeof(double) && double.IsNaN((double) (object) keys[i])) ||
                    (typeof(TKey) == typeof(float) && float.IsNaN((float) (object) keys[i])) ||
                    (typeof(TKey) == typeof(Half) && Half.IsNaN((Half) (object) keys[i])))
                {
                    TKey temp = keys[left];
                    keys[left] = keys[i];
                    keys[i] = temp;

                    if ((uint) i < (uint) values.Length) // check to see if we have values
                    {
                        TValue tempValue = values[left];
                        values[left] = values[i];
                        values[i] = tempValue;
                    }

                    left++;
                }
            }

            return left;
        }

        public static int Log2(uint value) =>
#if NETCOREAPP
            BitOperations.Log2(value);
#else
            // Fallback contract is 0->0
            Log2SoftwareFallback(value);
#endif


#if !NETCOREAPP
        /// <summary>
        /// Returns the integer (floor) log of the specified value, base 2.
        /// Note that by convention, input value 0 returns 0 since Log(0) is undefined.
        /// Does not directly use any hardware intrinsics, nor does it incur branching.
        /// </summary>
        /// <param name="value">The value.</param>
        private static int Log2SoftwareFallback(uint value)
        {
            // No AggressiveInlining due to large method size
            // Has conventional contract 0->0 (Log(0) is undefined)

            // Fill trailing zeros with ones, eg 00010010 becomes 00011111
            value |= value >> 01;
            value |= value >> 02;
            value |= value >> 04;
            value |= value >> 08;
            value |= value >> 16;

            // uint.MaxValue >> 27 is always in range [0 - 31] so we use Unsafe.AddByteOffset to avoid bounds check
            return Unsafe.AddByteOffset(
                // Using deBruijn sequence, k=2, n=5 (2^5=32) : 0b_0000_0111_1100_0100_1010_1100_1101_1101u
                ref MemoryMarshal.GetReference(Log2DeBruijn),
                // uint|long -> IntPtr cast on 32-bit platforms does expensive overflow checks not needed here
                (IntPtr)(int)((value * 0x07C4ACDDu) >> 27));
        }
#endif
    }
}
