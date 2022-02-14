// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Loretta.CodeAnalysis.PooledObjects
{
    // Dictionary that can be recycled via an object pool
    // NOTE: these dictionaries always have the default comparer.
    internal sealed partial class PooledDictionary<K, V> : Dictionary<K, V>
        where K : notnull
    {
        private readonly ObjectPool<PooledDictionary<K, V>> _pool;

        private PooledDictionary(ObjectPool<PooledDictionary<K, V>> pool, IEqualityComparer<K> keyComparer)
            : base(keyComparer)
        {
            _pool = pool;
        }

        public ImmutableDictionary<K, V> ToImmutableDictionaryAndFree()
        {
            var result = this.ToImmutableDictionary(Comparer);
            Free();
            return result;
        }

        public ImmutableDictionary<K, V> ToImmutableDictionary() => this.ToImmutableDictionary(Comparer);

        public void Free()
        {
            Clear();
            _pool?.Free(this);
        }

        // global pool
        private static readonly ObjectPool<PooledDictionary<K, V>> s_poolInstance = CreatePool(EqualityComparer<K>.Default);

        // if someone needs to create a pool;
        public static ObjectPool<PooledDictionary<K, V>> CreatePool(IEqualityComparer<K> keyComparer)
        {
            ObjectPool<PooledDictionary<K, V>>? pool = null;
            pool = new ObjectPool<PooledDictionary<K, V>>(() => new PooledDictionary<K, V>(pool!, keyComparer), 128);
            return pool;
        }

        public static PooledDictionary<K, V> GetInstance()
        {
            var instance = s_poolInstance.Allocate();
            LorettaDebug.Assert(instance.Count == 0);
            return instance;
        }
    }
}
