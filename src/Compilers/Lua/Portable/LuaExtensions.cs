using System.Diagnostics.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Syntax;

namespace Loretta.CodeAnalysis
{
    /// <summary>
    /// Extension methods for lua specific data.
    /// </summary>
    public static class LuaExtensions
    {
        /// <summary>
        /// Determines if <see cref="SyntaxToken"/> is of a specified kind.
        /// </summary>
        /// <param name="token">The source token.</param>
        /// <param name="kind">The syntax kind to test for.</param>
        /// <returns><see langword="true"/> if the token is of the specified kind; otherwise, <see langword="false"/>.</returns>
        public static bool IsKind(this SyntaxToken token, SyntaxKind kind) => token.RawKind == (int) kind;

        /// <summary>
        /// Determines if <see cref="SyntaxTrivia"/> is of a specified kind.
        /// </summary>
        /// <param name="trivia">The source trivia.</param>
        /// <param name="kind">The syntax kind to test for.</param>
        /// <returns><see langword="true"/> if the trivia is of the specified kind; otherwise, <see langword="false"/>.</returns>
        public static bool IsKind(this SyntaxTrivia trivia, SyntaxKind kind) => trivia.RawKind == (int) kind;

        /// <summary>
        /// Determines if <see cref="SyntaxNode"/> is of a specified kind.
        /// </summary>
        /// <param name="node">The source node.</param>
        /// <param name="kind">The syntax kind to test for.</param>
        /// <returns><see langword="true"/> if the node is of the specified kind; otherwise, <see langword="false"/>.</returns>
        public static bool IsKind([NotNullWhen(true)] this SyntaxNode? node, SyntaxKind kind) => node?.RawKind == (int) kind;

        /// <summary>
        /// Determines if <see cref="SyntaxNodeOrToken"/> is of a specified kind.
        /// </summary>
        /// <param name="nodeOrToken">The source node or token.</param>
        /// <param name="kind">The syntax kind to test for.</param>
        /// <returns><see langword="true"/> if the node or token is of the specified kind; otherwise, <see langword="false"/>.</returns>
        public static bool IsKind(this SyntaxNodeOrToken nodeOrToken, SyntaxKind kind) => nodeOrToken.RawKind == (int) kind;

        internal static SyntaxKind ContextualKind(this SyntaxToken token)
            => (object) token.Language == LanguageNames.Lua ? (SyntaxKind) token.RawContextualKind : SyntaxKind.None;

        /// <summary>
        /// Returns the index of the first node of a specified kind in the node list.
        /// </summary>
        /// <param name="list">Node list.</param>
        /// <param name="kind">The <see cref="SyntaxKind"/> to find.</param>
        /// <returns>Returns non-negative index if the list contains a node which matches <paramref name="kind"/>, -1 otherwise.</returns>
        public static int IndexOf<TNode>(this SyntaxList<TNode> list, SyntaxKind kind)
            where TNode : SyntaxNode => list.IndexOf((int) kind);

        /// <summary>
        /// True if the list has at least one node of the specified kind.
        /// </summary>
        public static bool Any<TNode>(this SyntaxList<TNode> list, SyntaxKind kind)
            where TNode : SyntaxNode => list.IndexOf(kind) >= 0;

        /// <summary>
        /// Returns the index of the first node of a specified kind in the node list.
        /// </summary>
        /// <param name="list">Node list.</param>
        /// <param name="kind">The <see cref="SyntaxKind"/> to find.</param>
        /// <returns>Returns non-negative index if the list contains a node which matches <paramref name="kind"/>, -1 otherwise.</returns>
        public static int IndexOf<TNode>(this SeparatedSyntaxList<TNode> list, SyntaxKind kind)
            where TNode : SyntaxNode => list.IndexOf((int) kind);

        /// <summary>
        /// True if the list has at least one node of the specified kind.
        /// </summary>
        public static bool Any<TNode>(this SeparatedSyntaxList<TNode> list, SyntaxKind kind)
            where TNode : SyntaxNode => list.IndexOf(kind) >= 0;

        /// <summary>
        /// Returns the index of the first trivia of a specified kind in the trivia list.
        /// </summary>
        /// <param name="list">Trivia list.</param>
        /// <param name="kind">The <see cref="SyntaxKind"/> to find.</param>
        /// <returns>Returns non-negative index if the list contains a trivia which matches <paramref name="kind"/>, -1 otherwise.</returns>
        public static int IndexOf(this SyntaxTriviaList list, SyntaxKind kind) => list.IndexOf((int) kind);

        /// <summary>
        /// True if the list has at least one trivia of the specified kind.
        /// </summary>
        public static bool Any(this SyntaxTriviaList list, SyntaxKind kind) => list.IndexOf(kind) >= 0;

        /// <summary>
        /// Returns the index of the first token of a specified kind in the token list.
        /// </summary>
        /// <param name="list">Token list.</param>
        /// <param name="kind">The <see cref="SyntaxKind"/> to find.</param>
        /// <returns>Returns non-negative index if the list contains a token which matches <paramref name="kind"/>, -1 otherwise.</returns>
        public static int IndexOf(this SyntaxTokenList list, SyntaxKind kind) => list.IndexOf((int) kind);

        /// <summary>
        /// Tests whether a list contains a token of a particular kind.
        /// </summary>
        /// <param name="list"></param>
        /// <param name="kind">The <see cref="SyntaxKind"/> to test for.</param>
        /// <returns>Returns true if the list contains a token which matches <paramref name="kind"/></returns>
        public static bool Any(this SyntaxTokenList list, SyntaxKind kind) => list.IndexOf(kind) >= 0;

        internal static SyntaxToken FirstOrDefault(this SyntaxTokenList list, SyntaxKind kind)
        {
            var index = list.IndexOf(kind);
            return (index >= 0) ? list[index] : default;
        }
    }
}

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// Extension methods for lua specific data.
    /// </summary>
    public static class LuaExtensions
    {
        /// <summary>
        /// Determines if the given raw kind value belongs to the C# <see cref="SyntaxKind"/> enumeration.
        /// </summary>
        /// <param name="rawKind">The raw value to test.</param>
        /// <returns><see langword="true"/> when the raw value belongs to the C# syntax kind; otherwise, <see langword="false"/>.</returns>
        internal static bool IsLuaKind(int rawKind)
        {
            const int FirstLuaKind = (int) SyntaxKind.ShebangTrivia;
            const int LastLuaKind = (int) SyntaxKind.CompilationUnit;

            // in the range [FirstLuaKind, LastLuaKind]
            return unchecked((uint) (rawKind - FirstLuaKind)) <= (LastLuaKind - FirstLuaKind);
        }

        /// <summary>
        /// Returns <see cref="SyntaxKind"/> for <see cref="SyntaxToken"/> from <see cref="SyntaxToken.RawKind"/> property.
        /// </summary>
        public static SyntaxKind Kind(this SyntaxToken token)
        {
            var rawKind = token.RawKind;
            return IsLuaKind(rawKind) ? (SyntaxKind) rawKind : SyntaxKind.None;
        }

        /// <summary>
        /// Returns <see cref="SyntaxKind"/> for <see cref="SyntaxTrivia"/> from <see cref="SyntaxTrivia.RawKind"/> property.
        /// </summary>
        public static SyntaxKind Kind(this SyntaxTrivia trivia)
        {
            var rawKind = trivia.RawKind;
            return IsLuaKind(rawKind) ? (SyntaxKind) rawKind : SyntaxKind.None;
        }

        /// <summary>
        /// Returns <see cref="SyntaxKind"/> for <see cref="SyntaxNode"/> from <see cref="SyntaxNode.RawKind"/> property.
        /// </summary>
        public static SyntaxKind Kind(this SyntaxNode node)
        {
            var rawKind = node.RawKind;
            return IsLuaKind(rawKind) ? (SyntaxKind) rawKind : SyntaxKind.None;
        }

        /// <summary>
        /// Returns <see cref="SyntaxKind"/> for <see cref="SyntaxNode"/> from <see cref="SyntaxNodeOrToken.RawKind"/> property.
        /// </summary>
        public static SyntaxKind Kind(this SyntaxNodeOrToken nodeOrToken)
        {
            var rawKind = nodeOrToken.RawKind;
            return IsLuaKind(rawKind) ? (SyntaxKind) rawKind : SyntaxKind.None;
        }

        /// <summary>
        /// Returns whether the provided <see cref="SyntaxToken"/> is a Lua keyword.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsKeyword(this SyntaxToken token) => SyntaxFacts.IsKeyword(token.Kind());

        /// <summary>
        /// Returns whether a <see cref="SyntaxToken"/> is a verbatim string literal.
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public static bool IsVerbatimStringLiteral(this SyntaxToken token)
            => token.IsKind(SyntaxKind.StringLiteralToken) && token.Text.StartsWith("[", StringComparison.Ordinal);

        /// <summary>
        /// Insert one or more tokens in the list at the specified index.
        /// </summary>
        /// <returns>A new list with the tokens inserted.</returns>
        public static SyntaxTokenList Insert(this SyntaxTokenList list, int index, params SyntaxToken[] items!!)
        {
            if (index < 0 || index > list.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (list.Count == 0)
            {
                return SyntaxFactory.TokenList(items);
            }
            else
            {
                var builder = new SyntaxTokenListBuilder(list.Count + items.Length);
                if (index > 0)
                {
                    builder.Add(list, 0, index);
                }

                builder.Add(items);

                if (index < list.Count)
                {
                    builder.Add(list, index, list.Count - index);
                }

                return builder.ToList();
            }
        }

        /// <summary>
        /// Creates a new token with the specified old trivia replaced with computed new trivia.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="trivia">The trivia to be replaced; descendants of the root token.</param>
        /// <param name="computeReplacementTrivia">A function that computes a replacement trivia for
        /// the argument trivia. The first argument is the original trivia. The second argument is
        /// the same trivia rewritten with replaced structure.</param>
        public static SyntaxToken ReplaceTrivia(this SyntaxToken token, IEnumerable<SyntaxTrivia> trivia, Func<SyntaxTrivia, SyntaxTrivia, SyntaxTrivia> computeReplacementTrivia)
            => Syntax.SyntaxReplacer.Replace(token, trivia: trivia, computeReplacementTrivia: computeReplacementTrivia);

        /// <summary>
        /// Creates a new token with the specified old trivia replaced with a new trivia. The old trivia may appear in
        /// the token's leading or trailing trivia.
        /// </summary>
        /// <param name="token"></param>
        /// <param name="oldTrivia">The trivia to be replaced.</param>
        /// <param name="newTrivia">The new trivia to use in the new tree in place of the old
        /// trivia.</param>
        public static SyntaxToken ReplaceTrivia(this SyntaxToken token, SyntaxTrivia oldTrivia, SyntaxTrivia newTrivia)
            => Syntax.SyntaxReplacer.Replace(token, trivia: new[] { oldTrivia }, computeReplacementTrivia: (o, r) => newTrivia);

        /// <summary>
        /// Returns this list as a <see cref="Loretta.CodeAnalysis.SeparatedSyntaxList&lt;TNode&gt;"/>.
        /// </summary>
        /// <typeparam name="TOther">The type of the list elements in the separated list.</typeparam>
        /// <returns></returns>
        internal static SeparatedSyntaxList<TOther> AsSeparatedList<TOther>(this SyntaxNodeOrTokenList list) where TOther : SyntaxNode
        {
            var builder = SeparatedSyntaxListBuilder<TOther>.Create();
            foreach (var i in list)
            {
                var node = i.AsNode();
                if (node != null)
                {
                    builder.Add((TOther) node);
                }
                else
                {
                    builder.AddSeparator(i.AsToken());
                }
            }

            return builder.ToList();
        }

        #region SyntaxTree

        /// <summary>
        /// Obtains the root as a <see cref="Syntax.CompilationUnitSyntax"/>.
        /// </summary>
        /// <param name="tree"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public static Syntax.CompilationUnitSyntax GetCompilationUnitRoot(this SyntaxTree tree, CancellationToken cancellationToken = default)
            => (Syntax.CompilationUnitSyntax) tree.GetRoot(cancellationToken);

        #endregion SyntaxTree
    }
}
