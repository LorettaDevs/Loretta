using System.Collections;

namespace Loretta.Generators.SyntaxFactsGenerator
{
    internal class KindList : IReadOnlyList<KindInfo>
    {
        private readonly ImmutableArray<KindInfo> _kinds;

        public KindList(ImmutableArray<KindInfo> kinds)
        {
            _kinds = kinds;
        }

        public IEnumerable<KindInfo> UnaryOperators => _kinds.Where(kind => kind.UnaryOperatorInfo is not null);
        public IEnumerable<KindInfo> BinaryOperators => _kinds.Where(kind => kind.BinaryOperatorInfo is not null);
        public IEnumerable<KindInfo> Tokens => _kinds.Where(kind => kind.TokenInfo is { IsKeyword: false, Text: not null and not "" });
        public IEnumerable<KindInfo> Keywords => _kinds.Where(kind => kind.TokenInfo is { IsKeyword: true, Text: not null and not "" });

        #region IReadOnlyList<KindInfo>
        public KindInfo this[int index] => _kinds[index];
        public int Count => _kinds.Length;
        public IEnumerator<KindInfo> GetEnumerator() => ((IEnumerable<KindInfo>) _kinds).GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable) _kinds).GetEnumerator();
        #endregion IReadOnlyList<KindInfo>
    }
}
