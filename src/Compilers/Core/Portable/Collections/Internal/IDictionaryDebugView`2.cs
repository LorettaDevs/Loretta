// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// NOTE: This code is derived from an implementation originally in dotnet/runtime:
// https://github.com/dotnet/runtime/blob/v5.0.2/src/libraries/System.Private.CoreLib/src/System/Collections/Generic/IDictionaryDebugView.cs
//
// See the commentary in https://github.com/dotnet/roslyn/pull/50156 for notes on incorporating changes made to the
// reference implementation.

using System.Diagnostics;

namespace Loretta.CodeAnalysis.Collections.Internal
{
    internal sealed class DictionaryDebugView<TKey, TValue>(IDictionary<TKey, TValue> dictionary) where TKey : notnull
    {
        private readonly IDictionary<TKey, TValue> _dict = dictionary
                                                           ?? throw new ArgumentNullException(nameof(dictionary));

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<TKey, TValue>[] Items
        {
            get
            {
                var items = new KeyValuePair<TKey, TValue>[_dict.Count];
                _dict.CopyTo(items, 0);
                return items;
            }
        }
    }

    // ReSharper disable once UnusedTypeParameter // Needed for debug type proxying
    internal sealed class DictionaryKeyCollectionDebugView<TKey, TValue>(ICollection<TKey> collection)
    {
        private readonly ICollection<TKey> _collection = collection
                                                         ?? throw new ArgumentNullException(nameof(collection));

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TKey[] Items
        {
            get
            {
                var items = new TKey[_collection.Count];
                _collection.CopyTo(items, 0);
                return items;
            }
        }
    }

    // ReSharper disable once UnusedTypeParameter // Needed for debug type proxying
    internal sealed class DictionaryValueCollectionDebugView<TKey, TValue>(ICollection<TValue> collection)
    {
        private readonly ICollection<TValue> _collection =
            collection ?? throw new ArgumentNullException(nameof(collection));

        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TValue[] Items
        {
            get
            {
                var items = new TValue[_collection.Count];
                _collection.CopyTo(items, 0);
                return items;
            }
        }
    }
}
