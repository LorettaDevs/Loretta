using System.Collections;

namespace Loretta.Generators.SyntaxFactsGenerator
{
    internal class KindList(ImmutableArray<KindInfo> kinds) : IReadOnlyList<KindInfo>
    {
        public IEnumerable<KindInfo> UnaryOperators => kinds.Where(static kind => kind.UnaryOperatorInfo is not null);

        public IEnumerable<KindInfo> BinaryOperators => kinds.Where(static kind => kind.BinaryOperatorInfo is not null);

        public IEnumerable<KindInfo> Tokens
            => kinds.Where(static kind => kind.TokenInfo is { IsKeyword: false, Text: not null and not "" });

        public IEnumerable<KindInfo> Keywords
            => kinds.Where(static kind => kind.TokenInfo is { IsKeyword: true, Text: not null and not "" });

        #region IReadOnlyList<KindInfo>

        public KindInfo this[int index] => kinds[index];

        public int Count => kinds.Length;

        public IEnumerator<KindInfo> GetEnumerator() => ((IEnumerable<KindInfo>) kinds).GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) kinds).GetEnumerator();

        #endregion IReadOnlyList<KindInfo>
    }
}
