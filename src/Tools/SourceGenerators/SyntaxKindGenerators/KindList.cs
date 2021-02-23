using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;

namespace Loretta.Generators.SyntaxKindGenerators
{
    internal class KindList : IReadOnlyList<KindInfo>
    {
        private readonly ImmutableArray<KindInfo> _kinds;
        private readonly IImmutableDictionary<string, KindInfo> _kindMap;

        public KindList(ImmutableArray<KindInfo> kinds)
        {

            _kinds = kinds;
            _kindMap = kinds.ToImmutableDictionary(kind => kind.Field.Name, StringComparer.Ordinal);
        }

        #region IReadOnlyList<KindInfo>
        public KindInfo this[int index] => ((IReadOnlyList<KindInfo>) _kinds)[index];
        public int Count => ((IReadOnlyCollection<KindInfo>) _kinds).Count;
        public IEnumerator<KindInfo> GetEnumerator() => ((IEnumerable<KindInfo>) _kinds).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _kinds).GetEnumerator();
        #endregion IReadOnlyList<KindInfo>

        #region Dictionary
        public KindInfo this[string key] => _kindMap[key];
        public bool ContainsKind(string key) => _kindMap.ContainsKey(key);
        public bool TryGetKind(string key, out KindInfo value) => _kindMap.TryGetValue(key, out value);
        #endregion Dictionary

        public bool IsToken(string kindName) =>
            TryGetKind(kindName, out var kind) && kind.TokenInfo is not null;

        public bool IsAutoCreatableToken(string kindName) =>
            TryGetKind(kindName, out var kind)
            && !string.IsNullOrWhiteSpace(kind.TokenInfo?.Text);
    }
}
