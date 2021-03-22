using System;
using System.Collections.Generic;
using System.Text;
using Loretta.CodeAnalysis.Lua.SymbolDisplay;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Syntax;
using InternalSyntax = Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax;

namespace Loretta.CodeAnalysis.Lua
{
    /// <summary>
    /// A class containing factory methods for constructing syntax nodes, tokens and trivia.
    /// </summary>
    public static partial class SyntaxFactory
    {
        /// <summary>
        /// A trivia with kind EndOfLineTrivia containing both the carriage return and line feed characters.
        /// </summary>
        public static SyntaxTrivia CarriageReturnLineFeed { get; } = InternalSyntax.SyntaxFactory.CarriageReturnLineFeed;

        /// <summary>
        /// A trivia with kind EndOfLineTrivia containing a single line feed character.
        /// </summary>
        public static SyntaxTrivia LineFeed { get; } = InternalSyntax.SyntaxFactory.LineFeed;

        /// <summary>
        /// A trivia with kind EndOfLineTrivia containing a single carriage return character.
        /// </summary>
        public static SyntaxTrivia CarriageReturn { get; } = InternalSyntax.SyntaxFactory.CarriageReturn;

        /// <summary>
        ///  A trivia with kind WhitespaceTrivia containing a single space character.
        /// </summary>
        public static SyntaxTrivia Space { get; } = InternalSyntax.SyntaxFactory.Space;

        /// <summary>
        /// A trivia with kind WhitespaceTrivia containing a single tab character.
        /// </summary>
        public static SyntaxTrivia Tab { get; } = InternalSyntax.SyntaxFactory.Tab;

        /// <summary>
        /// An elastic trivia with kind EndOfLineTrivia containing both the carriage return and line feed characters.
        /// Elastic trivia are used to denote trivia that was not produced by parsing source text, and are usually not
        /// preserved during formatting.
        /// </summary>
        public static SyntaxTrivia ElasticCarriageReturnLineFeed { get; } =
            InternalSyntax.SyntaxFactory.ElasticCarriageReturnLineFeed;

        /// <summary>
        /// An elastic trivia with kind EndOfLineTrivia containing a single line feed character. Elastic trivia are used
        /// to denote trivia that was not produced by parsing source text, and are usually not preserved during
        /// formatting.
        /// </summary>
        public static SyntaxTrivia ElasticLineFeed { get; } = InternalSyntax.SyntaxFactory.ElasticLineFeed;

        /// <summary>
        /// An elastic trivia with kind EndOfLineTrivia containing a single carriage return character. Elastic trivia
        /// are used to denote trivia that was not produced by parsing source text, and are usually not preserved during
        /// formatting.
        /// </summary>
        public static SyntaxTrivia ElasticCarriageReturn { get; } = InternalSyntax.SyntaxFactory.ElasticCarriageReturn;

        /// <summary>
        /// An elastic trivia with kind WhitespaceTrivia containing a single space character. Elastic trivia are used to
        /// denote trivia that was not produced by parsing source text, and are usually not preserved during formatting.
        /// </summary>
        public static SyntaxTrivia ElasticSpace { get; } = InternalSyntax.SyntaxFactory.ElasticSpace;

        /// <summary>
        /// An elastic trivia with kind WhitespaceTrivia containing a single tab character. Elastic trivia are used to
        /// denote trivia that was not produced by parsing source text, and are usually not preserved during formatting.
        /// </summary>
        public static SyntaxTrivia ElasticTab { get; } = InternalSyntax.SyntaxFactory.ElasticTab;

        /// <summary>
        /// An elastic trivia with kind WhitespaceTrivia containing no characters. Elastic marker trivia are included
        /// automatically by factory methods when trivia is not specified. Syntax formatting will replace elastic
        /// markers with appropriate trivia.
        /// </summary>
        public static SyntaxTrivia ElasticMarker { get; } = InternalSyntax.SyntaxFactory.ElasticZeroSpace;

        /// <summary>
        /// Creates a trivia with kind EndOfLineTrivia containing the specified text. 
        /// </summary>
        /// <param name="text">The text of the end of line. Any text can be specified here, however only carriage return and
        /// line feed characters are recognized by the parser as end of line.</param>
        public static SyntaxTrivia EndOfLine(string text)
            => InternalSyntax.SyntaxFactory.EndOfLine(text, elastic: false);

        /// <summary>
        /// Creates a trivia with kind EndOfLineTrivia containing the specified text. Elastic trivia are used to
        /// denote trivia that was not produced by parsing source text, and are usually not preserved during formatting.
        /// </summary>
        /// <param name="text">The text of the end of line. Any text can be specified here, however only carriage return and
        /// line feed characters are recognized by the parser as end of line.</param>
        public static SyntaxTrivia ElasticEndOfLine(string text)
            => InternalSyntax.SyntaxFactory.EndOfLine(text, elastic: true);

        /// <summary>
        /// Creates a trivia with kind WhitespaceTrivia containing the specified text.
        /// </summary>
        /// <param name="text">The text of the whitespace. Any text can be specified here, however only specific
        /// whitespace characters are recognized by the parser.</param>
        public static SyntaxTrivia Whitespace(string text)
            => InternalSyntax.SyntaxFactory.Whitespace(text, elastic: false);

        /// <summary>
        /// Creates a trivia with kind WhitespaceTrivia containing the specified text. Elastic trivia are used to
        /// denote trivia that was not produced by parsing source text, and are usually not preserved during formatting.
        /// </summary>
        /// <param name="text">The text of the whitespace. Any text can be specified here, however only specific
        /// whitespace characters are recognized by the parser.</param>
        public static SyntaxTrivia ElasticWhitespace(string text)
            => InternalSyntax.SyntaxFactory.Whitespace(text, elastic: false);

        /// <summary>
        /// Creates a trivia with kind either SingleLineCommentTrivia or MultiLineCommentTrivia containing the specified
        /// text.
        /// </summary>
        /// <param name="text">The entire text of the comment including the leading '--' or '//' token for single line
        /// comments or stop or start tokens for multiline comments.</param>
        public static SyntaxTrivia Comment(string text) => InternalSyntax.SyntaxFactory.Comment(text);

        /// <summary>
        /// Trivia nodes represent parts of the program text that are not parts of the
        /// syntactic grammar, such as spaces, newlines, shebangs and comments.
        /// </summary>
        /// <param name="kind">
        /// A <see cref="SyntaxKind"/> representing the specific kind of <see cref="SyntaxTrivia"/>. One of
        /// <see cref="SyntaxKind.ShebangTrivia"/>, <see cref="SyntaxKind.EndOfLineTrivia"/>,
        /// <see cref="SyntaxKind.SingleLineCommentTrivia"/>, <see cref="SyntaxKind.MultiLineCommentTrivia"/>,
        /// <see cref="SyntaxKind.WhitespaceTrivia"/>
        /// </param>
        /// <param name="text">
        /// The actual text of this token.
        /// </param>
        public static SyntaxTrivia SyntaxTrivia(SyntaxKind kind, string text)
        {
            if (text is null)
                throw new ArgumentNullException(nameof(text));

            return kind switch
            {
                SyntaxKind.ShebangTrivia
                or SyntaxKind.EndOfLineTrivia
                or SyntaxKind.SingleLineCommentTrivia
                or SyntaxKind.MultiLineCommentTrivia
                or SyntaxKind.WhitespaceTrivia
                => new SyntaxTrivia(default, new InternalSyntax.SyntaxTrivia(kind, text), 0, 0),
                _ => throw new ArgumentException("The provided kind is not a trivia's.", nameof(kind)),
            };
        }

        /// <summary>
        /// Creates a token corresponding to a syntax kind. This method can be used for token syntax kinds whose text
        /// can be inferred by the kind alone.
        /// </summary>
        /// <param name="kind">A syntax kind value for a token. These have the suffix Token or Keyword.</param>
        /// <returns></returns>
        public static SyntaxToken Token(SyntaxKind kind)
            => new(InternalSyntax.SyntaxFactory.Token(ElasticMarker.UnderlyingNode, kind, ElasticMarker.UnderlyingNode));

        /// <summary>
        /// Creates a token corresponding to syntax kind. This method can be used for token syntax kinds whose text can
        /// be inferred by the kind alone.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="kind">A syntax kind value for a token. These have the suffix Token or Keyword.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Token(SyntaxTriviaList leading, SyntaxKind kind, SyntaxTriviaList trailing)
            => new(InternalSyntax.SyntaxFactory.Token(leading.Node, kind, trailing.Node));

        /// <summary>
        /// Creates a token corresponding to syntax kind. This method gives control over token Text and ValueText.
        /// 
        /// For example, consider the text '&lt;see cref="operator &amp;#43;"/&gt;'.  To create a token for the value of
        /// the operator symbol (&amp;#43;), one would call 
        /// Token(default(SyntaxTriviaList), SyntaxKind.PlusToken, "&amp;#43;", "+", default(SyntaxTriviaList)).
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="kind">A syntax kind value for a token. These have the suffix Token or Keyword.</param>
        /// <param name="text">The text from which this token was created (e.g. lexed).</param>
        /// <param name="valueText">How Lua should interpret the text of this token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Token(SyntaxTriviaList leading, SyntaxKind kind, string text, string valueText, SyntaxTriviaList trailing)
        {
            switch (kind)
            {
                case SyntaxKind.IdentifierToken:
                    // Have a different representation.
                    throw new ArgumentException(LuaResources.UseIdentifierToCreateIdentifiers, nameof(kind));
                case SyntaxKind.NumericLiteralToken:
                    // Value should not have type string.
                    throw new ArgumentException(LuaResources.UseLiteralForNumeric, nameof(kind));
            }

            if (!SyntaxFacts.IsToken(kind))
            {
                throw new ArgumentException(string.Format(LuaResources.ThisMethodCanOnlyBeUsedToCreateTokens, kind), nameof(kind));
            }

            return new SyntaxToken(InternalSyntax.SyntaxFactory.Token(leading.Node, kind, text, valueText, trailing.Node));
        }

        /// <summary>
        /// Creates a missing token corresponding to syntax kind. A missing token is produced by the parser when an
        /// expected token is not found. A missing token has no text and normally has associated diagnostics.
        /// </summary>
        /// <param name="kind">A syntax kind value for a token. These have the suffix Token or Keyword.</param>
        public static SyntaxToken MissingToken(SyntaxKind kind)
            => new(InternalSyntax.SyntaxFactory.MissingToken(ElasticMarker.UnderlyingNode, kind, ElasticMarker.UnderlyingNode));

        /// <summary>
        /// Creates a missing token corresponding to syntax kind. A missing token is produced by the parser when an
        /// expected token is not found. A missing token has no text and normally has associated diagnostics.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="kind">A syntax kind value for a token. These have the suffix Token or Keyword.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken MissingToken(SyntaxTriviaList leading, SyntaxKind kind, SyntaxTriviaList trailing)
            => new(InternalSyntax.SyntaxFactory.MissingToken(leading.Node, kind, trailing.Node));

        /// <summary>
        /// Creates a token with kind IdentifierToken containing the specified text.
        /// <param name="text">The text of the identifier name.</param>
        /// </summary>
        public static SyntaxToken Identifier(string text)
            => new SyntaxToken(InternalSyntax.SyntaxFactory.Identifier(ElasticMarker.UnderlyingNode, text, ElasticMarker.UnderlyingNode));

        /// <summary>
        /// Creates a token with kind IdentifierToken containing the specified text.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The text of the identifier name.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Identifier(SyntaxTriviaList leading, string text, SyntaxTriviaList trailing)
            => new SyntaxToken(InternalSyntax.SyntaxFactory.Identifier(leading.Node, text, trailing.Node));

        /// <summary>
        /// Creates a token with kind IdentifierToken containing the specified text.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="contextualKind">An alternative SyntaxKind that can be inferred for this token in special
        /// contexts. These are usually keywords.</param>
        /// <param name="text">The raw text of the identifier name, including any escapes or leading '@'
        /// character.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        /// 
        /// <returns></returns>
        public static SyntaxToken Identifier(SyntaxTriviaList leading, SyntaxKind contextualKind, string text, SyntaxTriviaList trailing)
            => new SyntaxToken(InternalSyntax.SyntaxFactory.Identifier(contextualKind, leading.Node, text, trailing.Node));

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from an 8-byte floating point value.
        /// </summary>
        /// <param name="value">The 8-byte floating point value to be represented by the returned token.</param>
        public static SyntaxToken Literal(double value) =>
            Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.None), value);

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 8-byte floating point value.
        /// </summary>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 8-byte floating point value to be represented by the returned token.</param>
        public static SyntaxToken Literal(string text, double value) =>
            new SyntaxToken(InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 8-byte floating point value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 8-byte floating point value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Literal(SyntaxTriviaList leading, string text, double value, SyntaxTriviaList trailing) =>
            new SyntaxToken(InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));

        /// <summary>
        /// Creates a token with kind StringLiteralToken from a string value.
        /// </summary>
        /// <param name="value">The string value to be represented by the returned token.</param>
        public static SyntaxToken Literal(string value) =>
            Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.EscapeNonPrintableCharacters | ObjectDisplayOptions.UseQuotes), value);

        /// <summary>
        /// Creates a token with kind StringLiteralToken from the text and corresponding string value.
        /// </summary>
        /// <param name="text">The raw text of the literal, including quotes and escape sequences.</param>
        /// <param name="value">The string value to be represented by the returned token.</param>
        public static SyntaxToken Literal(string text, string value) =>
            new SyntaxToken(InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));

        /// <summary>
        /// Creates a token with kind StringLiteralToken from the text and corresponding string value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal, including quotes and escape sequences.</param>
        /// <param name="value">The string value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Literal(SyntaxTriviaList leading, string text, string value, SyntaxTriviaList trailing) =>
            new SyntaxToken(InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));

        /// <summary>
        /// Creates a token with kind BadToken.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the bad token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken BadToken(SyntaxTriviaList leading, string text, SyntaxTriviaList trailing) =>
            new SyntaxToken(Syntax.InternalSyntax.SyntaxFactory.BadToken(leading.Node, text, trailing.Node));

        /// <summary>
        /// Creates an empty list of syntax nodes.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        public static SyntaxList<TNode> List<TNode>() where TNode : SyntaxNode => default;

        /// <summary>
        /// Creates a singleton list of syntax nodes.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        /// <param name="node">The single element node.</param>
        /// <returns></returns>
        public static SyntaxList<TNode> SingletonList<TNode>(TNode node) where TNode : SyntaxNode => new SyntaxList<TNode>(node);

        /// <summary>
        /// Creates a list of syntax nodes.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        /// <param name="nodes">A sequence of element nodes.</param>
        public static SyntaxList<TNode> List<TNode>(IEnumerable<TNode> nodes) where TNode : SyntaxNode => new SyntaxList<TNode>(nodes);

        /// <summary>
        /// Creates an empty list of tokens.
        /// </summary>
        public static SyntaxTokenList TokenList() => default;

        /// <summary>
        /// Creates a singleton list of tokens.
        /// </summary>
        /// <param name="token">The single token.</param>
        public static SyntaxTokenList TokenList(SyntaxToken token) => new SyntaxTokenList(token);

        /// <summary>
        /// Creates a list of tokens.
        /// </summary>
        /// <param name="tokens">An array of tokens.</param>
        public static SyntaxTokenList TokenList(params SyntaxToken[] tokens) => new SyntaxTokenList(tokens);

        /// <summary>
        /// Creates a list of tokens.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static SyntaxTokenList TokenList(IEnumerable<SyntaxToken> tokens) => new SyntaxTokenList(tokens);

        /// <summary>
        /// Creates a trivia from a StructuredTriviaSyntax node.
        /// </summary>
        public static SyntaxTrivia Trivia(StructuredTriviaSyntax node) => new SyntaxTrivia(default, node.Green, position: 0, index: 0);

        /// <summary>
        /// Creates an empty list of trivia.
        /// </summary>
        public static SyntaxTriviaList TriviaList() => default;

        /// <summary>
        /// Creates a singleton list of trivia.
        /// </summary>
        /// <param name="trivia">A single trivia.</param>
        public static SyntaxTriviaList TriviaList(SyntaxTrivia trivia) => new SyntaxTriviaList(trivia);

        /// <summary>
        /// Creates a list of trivia.
        /// </summary>
        /// <param name="trivias">An array of trivia.</param>
        public static SyntaxTriviaList TriviaList(params SyntaxTrivia[] trivias)
            => new SyntaxTriviaList(trivias);

        /// <summary>
        /// Creates a list of trivia.
        /// </summary>
        /// <param name="trivias">A sequence of trivia.</param>
        public static SyntaxTriviaList TriviaList(IEnumerable<SyntaxTrivia> trivias)
            => new SyntaxTriviaList(trivias);

        /// <summary>
        /// Creates an empty separated list.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        public static SeparatedSyntaxList<TNode> SeparatedList<TNode>() where TNode : SyntaxNode => default;

        /// <summary>
        /// Creates a singleton separated list.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        /// <param name="node">A single node.</param>
        public static SeparatedSyntaxList<TNode> SingletonSeparatedList<TNode>(TNode node) where TNode : SyntaxNode => new SeparatedSyntaxList<TNode>(new SyntaxNodeOrTokenList(node, index: 0));

        /// <summary>
        /// Creates a separated list of nodes from a sequence of nodes, synthesizing comma separators in between.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        /// <param name="nodes">A sequence of syntax nodes.</param>
        public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(IEnumerable<TNode>? nodes) where TNode : SyntaxNode
        {
            if (nodes == null)
            {
                return default;
            }

            var collection = nodes as ICollection<TNode>;

            if (collection != null && collection.Count == 0)
            {
                return default;
            }

            using (var enumerator = nodes.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    return default;
                }

                var firstNode = enumerator.Current;

                if (!enumerator.MoveNext())
                {
                    return SingletonSeparatedList(firstNode);
                }

                var builder = new SeparatedSyntaxListBuilder<TNode>(collection != null ? collection.Count : 3);

                builder.Add(firstNode);

                var commaToken = Token(SyntaxKind.CommaToken);

                do
                {
                    builder.AddSeparator(commaToken);
                    builder.Add(enumerator.Current);
                }
                while (enumerator.MoveNext());

                return builder.ToList();
            }
        }

        /// <summary>
        /// Creates a separated list of nodes from a sequence of nodes and a sequence of separator tokens.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        /// <param name="nodes">A sequence of syntax nodes.</param>
        /// <param name="separators">A sequence of token to be interleaved between the nodes. The number of tokens must
        /// be one less than the number of nodes.</param>
        public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(IEnumerable<TNode> nodes, IEnumerable<SyntaxToken> separators) where TNode : SyntaxNode
        {
            // Interleave the nodes and the separators.  The number of separators must be equal to or 1 less than the number of nodes or
            // an argument exception is thrown.

            if (nodes != null)
            {
                var enumerator = nodes.GetEnumerator();
                SeparatedSyntaxListBuilder<TNode> builder = SeparatedSyntaxListBuilder<TNode>.Create();
                if (separators != null)
                {
                    foreach (var token in separators)
                    {
                        if (!enumerator.MoveNext())
                        {
                            throw new ArgumentException($"{nameof(nodes)} must not be empty.", nameof(nodes));
                        }

                        builder.Add(enumerator.Current);
                        builder.AddSeparator(token);
                    }
                }

                if (enumerator.MoveNext())
                {
                    builder.Add(enumerator.Current);
                    if (enumerator.MoveNext())
                    {
                        throw new ArgumentException($"{nameof(separators)} must have 1 fewer element than {nameof(nodes)}", nameof(separators));
                    }
                }

                return builder.ToList();
            }

            if (separators != null)
            {
                throw new ArgumentException($"When {nameof(nodes)} is null, {nameof(separators)} must also be null.", nameof(separators));
            }

            return default;
        }

        /// <summary>
        /// Creates a separated list from a sequence of nodes and tokens, starting with a node and alternating between additional nodes and separator tokens.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        /// <param name="nodesAndTokens">A sequence of nodes or tokens, alternating between nodes and separator tokens.</param>
        public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(IEnumerable<SyntaxNodeOrToken> nodesAndTokens) where TNode : SyntaxNode => SeparatedList<TNode>(NodeOrTokenList(nodesAndTokens));

        /// <summary>
        /// Creates a separated list from a <see cref="SyntaxNodeOrTokenList"/>, where the list elements start with a node and then alternate between
        /// additional nodes and separator tokens.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        /// <param name="nodesAndTokens">The list of nodes and tokens.</param>
        public static SeparatedSyntaxList<TNode> SeparatedList<TNode>(SyntaxNodeOrTokenList nodesAndTokens) where TNode : SyntaxNode
        {
            if (!HasSeparatedNodeTokenPattern(nodesAndTokens))
            {
                throw new ArgumentException(CodeAnalysisResources.NodeOrTokenOutOfSequence);
            }

            if (!NodesAreCorrectType<TNode>(nodesAndTokens))
            {
                throw new ArgumentException(CodeAnalysisResources.UnexpectedTypeOfNodeInList);
            }

            return new SeparatedSyntaxList<TNode>(nodesAndTokens);
        }

        private static bool NodesAreCorrectType<TNode>(SyntaxNodeOrTokenList list)
        {
            for (int i = 0, n = list.Count; i < n; i++)
            {
                var element = list[i];
                if (element.IsNode && !(element.AsNode() is TNode))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool HasSeparatedNodeTokenPattern(SyntaxNodeOrTokenList list)
        {
            for (int i = 0, n = list.Count; i < n; i++)
            {
                var element = list[i];
                if (element.IsToken == ((i & 1) == 0))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Creates an empty <see cref="SyntaxNodeOrTokenList"/>.
        /// </summary>
        public static SyntaxNodeOrTokenList NodeOrTokenList() => default;

        /// <summary>
        /// Create a <see cref="SyntaxNodeOrTokenList"/> from a sequence of <see cref="SyntaxNodeOrToken"/>.
        /// </summary>
        /// <param name="nodesAndTokens">The sequence of nodes and tokens</param>
        public static SyntaxNodeOrTokenList NodeOrTokenList(IEnumerable<SyntaxNodeOrToken> nodesAndTokens) => new SyntaxNodeOrTokenList(nodesAndTokens);

        /// <summary>
        /// Create a <see cref="SyntaxNodeOrTokenList"/> from one or more <see cref="SyntaxNodeOrToken"/>.
        /// </summary>
        /// <param name="nodesAndTokens">The nodes and tokens</param>
        public static SyntaxNodeOrTokenList NodeOrTokenList(params SyntaxNodeOrToken[] nodesAndTokens) => new SyntaxNodeOrTokenList(nodesAndTokens);

        /// <summary>
        /// Creates an <see cref="IdentifierNameSyntax"/> node.
        /// </summary>
        /// <param name="name">The identifier name.</param>
        public static IdentifierNameSyntax IdentifierName(string name) => IdentifierName(Identifier(name));

        // direct access to parsing for common grammar areas

        /// <summary>
        /// Create a new syntax tree from a syntax node.
        /// </summary>
        public static SyntaxTree SyntaxTree(SyntaxNode root, ParseOptions? options = null, string path = "", Encoding? encoding = null) =>
            LuaSyntaxTree.Create((LuaSyntaxNode) root, (LuaParseOptions?) options, path, encoding);
    }
}
