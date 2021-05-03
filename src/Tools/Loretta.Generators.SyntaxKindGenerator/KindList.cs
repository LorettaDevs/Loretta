using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Loretta.Generators.SyntaxKindGenerator
{
    internal class KindList : IReadOnlyList<KindInfo>
    {
        private readonly ImmutableArray<KindInfo> _kinds;
        private readonly IImmutableDictionary<string, KindInfo> _kindMap;
        private readonly Lazy<ImmutableArray<KindInfo>> _lazyUnaryOperators;
        private readonly Lazy<ImmutableArray<KindInfo>> _lazyBinaryOperators;
        private readonly Lazy<ImmutableArray<KindInfo>> _lazyTokens;
        private readonly Lazy<ImmutableArray<KindInfo>> _lazyKeywords;

        public KindList(ImmutableArray<KindInfo> kinds)
        {
            _kinds = kinds;
            _kindMap = kinds.ToImmutableDictionary(kind => kind.Field.Name, StringComparer.Ordinal);
            _lazyUnaryOperators = new Lazy<ImmutableArray<KindInfo>>(() => kinds.Where(kind => kind.UnaryOperatorInfo is not null).ToImmutableArray(), true);
            _lazyBinaryOperators = new Lazy<ImmutableArray<KindInfo>>(() => kinds.Where(kind => kind.BinaryOperatorInfo is not null).ToImmutableArray(), true);
            _lazyTokens = new Lazy<ImmutableArray<KindInfo>>(() => kinds.Where(kind => kind.TokenInfo is { IsKeyword: false, Text: not null and not "" }).ToImmutableArray(), true);
            _lazyKeywords = new Lazy<ImmutableArray<KindInfo>>(() => kinds.Where(kind => kind.TokenInfo is { IsKeyword: true, Text: not null and not "" }).ToImmutableArray(), true);
        }

        public IEnumerable<KindInfo> UnaryOperators => _lazyUnaryOperators.Value;
        public IEnumerable<KindInfo> BinaryOperators => _lazyBinaryOperators.Value;
        public IEnumerable<KindInfo> Tokens => _lazyTokens.Value;
        public IEnumerable<KindInfo> Keywords => _lazyKeywords.Value;

        #region IReadOnlyList<KindInfo>
        public KindInfo this[int index] => _kinds[index];
        public int Count => _kinds.Length;
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
