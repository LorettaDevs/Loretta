// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Loretta.CodeAnalysis.PooledObjects;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis
{
    // special case for a situation where the key is a reference type with object identity 
    // in this case:
    //      keyHash             is assumed to be RuntimeHelpers.GetHashCode
    //      keyValueEquality    is an object == for the new and old keys 
    //                          NOTE: we do store the key in this case 
    //                          reference comparison of keys is as cheap as comparing hash codes.
    internal class CachingIdentityFactory<TKey, TValue> : CachingBase<CachingIdentityFactory<TKey, TValue>.Entry>
        where TKey : class
    {
        private readonly Func<TKey, TValue> _valueFactory;
        private readonly ObjectPool<CachingIdentityFactory<TKey, TValue>>? _pool;

        internal struct Entry
        {
            internal TKey key;
            internal TValue value;
        }

        public CachingIdentityFactory(int size, Func<TKey, TValue> valueFactory)
            : base(size)
        {
            _valueFactory = valueFactory;
        }

        public CachingIdentityFactory(int size, Func<TKey, TValue> valueFactory, ObjectPool<CachingIdentityFactory<TKey, TValue>> pool)
            : this(size, valueFactory)
        {
            _pool = pool;
        }

        public void Add(TKey key, TValue value)
        {
            var hash = RuntimeHelpers.GetHashCode(key);
            var idx = hash & mask;

            entries[idx].key = key;
            entries[idx].value = value;
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(returnValue: false)] out TValue value)
        {
            int hash = RuntimeHelpers.GetHashCode(key);
            int idx = hash & mask;

            var entries = this.entries;
            if (entries[idx].key == key)
            {
                value = entries[idx].value;
                return true;
            }

            value = default!;
            return false;
        }

        public TValue GetOrMakeValue(TKey key)
        {
            int hash = RuntimeHelpers.GetHashCode(key);
            int idx = hash & mask;

            var entries = this.entries;
            if (entries[idx].key == key)
            {
                return entries[idx].value;
            }

            var value = _valueFactory(key);
            entries[idx].key = key;
            entries[idx].value = value;

            return value;
        }

        // if someone needs to create a pool;
        public static ObjectPool<CachingIdentityFactory<TKey, TValue>> CreatePool(int size, Func<TKey, TValue> valueFactory)
        {
            var pool = new ObjectPool<CachingIdentityFactory<TKey, TValue>>(
                pool => new CachingIdentityFactory<TKey, TValue>(size, valueFactory, pool),
                Environment.ProcessorCount * 2);

            return pool;
        }

        public void Free()
        {
            var pool = _pool;

            // Array.Clear(this.entries, 0, this.entries.Length);

            pool?.Free(this);
        }
    }

    // Just holds the data for the derived caches.
    internal abstract class CachingBase<TEntry>
    {
        // cache size is always ^2. 
        // items are placed at [hash ^ mask]
        // new item will displace previous one at the same location.
        protected readonly int mask;
        protected readonly TEntry[] entries;

        internal CachingBase(int size)
        {
            var alignedSize = AlignSize(size);
            mask = alignedSize - 1;
            entries = new TEntry[alignedSize];
        }

        private static int AlignSize(int size)
        {
            LorettaDebug.Assert(size > 0);

            size--;
            size |= size >> 1;
            size |= size >> 2;
            size |= size >> 4;
            size |= size >> 8;
            size |= size >> 16;
            return size + 1;
        }
    }
}
