﻿using System.Text;
using System.Numerics;
using Loretta.CodeAnalysis.Lua.SymbolDisplay;
using Loretta.CodeAnalysis.Lua.Syntax;
using Loretta.CodeAnalysis.Syntax;
using Loretta.CodeAnalysis.Text;
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
        public static SyntaxTrivia EndOfLine(string text) =>
            InternalSyntax.SyntaxFactory.EndOfLine(text, elastic: false);

        /// <summary>
        /// Creates a trivia with kind EndOfLineTrivia containing the specified text. Elastic trivia are used to
        /// denote trivia that was not produced by parsing source text, and are usually not preserved during formatting.
        /// </summary>
        /// <param name="text">The text of the end of line. Any text can be specified here, however only carriage return and
        /// line feed characters are recognized by the parser as end of line.</param>
        public static SyntaxTrivia ElasticEndOfLine(string text) =>
            InternalSyntax.SyntaxFactory.EndOfLine(text, elastic: true);

        /// <summary>
        /// Creates a trivia with kind WhitespaceTrivia containing the specified text.
        /// </summary>
        /// <param name="text">The text of the whitespace. Any text can be specified here, however only specific
        /// whitespace characters are recognized by the parser.</param>
        public static SyntaxTrivia Whitespace(string text) =>
            InternalSyntax.SyntaxFactory.Whitespace(text, elastic: false);

        /// <summary>
        /// Creates a trivia with kind WhitespaceTrivia containing the specified text. Elastic trivia are used to
        /// denote trivia that was not produced by parsing source text, and are usually not preserved during formatting.
        /// </summary>
        /// <param name="text">The text of the whitespace. Any text can be specified here, however only specific
        /// whitespace characters are recognized by the parser.</param>
        public static SyntaxTrivia ElasticWhitespace(string text) =>
            InternalSyntax.SyntaxFactory.Whitespace(text, elastic: false);

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
        public static SyntaxTrivia SyntaxTrivia(SyntaxKind kind, string text!!)
        {
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
        public static SyntaxToken Token(SyntaxKind kind) =>
            new(InternalSyntax.SyntaxFactory.Token(ElasticMarker.UnderlyingNode, kind, ElasticMarker.UnderlyingNode));

        /// <summary>
        /// Creates a token corresponding to syntax kind. This method can be used for token syntax kinds whose text can
        /// be inferred by the kind alone.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="kind">A syntax kind value for a token. These have the suffix Token or Keyword.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Token(SyntaxTriviaList leading, SyntaxKind kind, SyntaxTriviaList trailing) =>
            new(InternalSyntax.SyntaxFactory.Token(leading.Node, kind, trailing.Node));

        /// <summary>
        /// Creates a token corresponding to syntax kind. This method gives control over token Text and ValueText.
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
        public static SyntaxToken MissingToken(SyntaxKind kind) =>
            new(InternalSyntax.SyntaxFactory.MissingToken(ElasticMarker.UnderlyingNode, kind, ElasticMarker.UnderlyingNode));

        /// <summary>
        /// Creates a missing token corresponding to syntax kind. A missing token is produced by the parser when an
        /// expected token is not found. A missing token has no text and normally has associated diagnostics.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="kind">A syntax kind value for a token. These have the suffix Token or Keyword.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken MissingToken(SyntaxTriviaList leading, SyntaxKind kind, SyntaxTriviaList trailing) =>
            new(InternalSyntax.SyntaxFactory.MissingToken(leading.Node, kind, trailing.Node));

        /// <summary>
        /// Creates a token with kind IdentifierToken containing the specified text.
        /// <param name="text">The text of the identifier name.</param>
        /// </summary>
        public static SyntaxToken Identifier(string text) =>
            new(InternalSyntax.SyntaxFactory.Identifier(ElasticMarker.UnderlyingNode, text, ElasticMarker.UnderlyingNode));

        /// <summary>
        /// Creates a token with kind IdentifierToken containing the specified text.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The text of the identifier name.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Identifier(SyntaxTriviaList leading, string text, SyntaxTriviaList trailing) =>
            new(InternalSyntax.SyntaxFactory.Identifier(leading.Node, text, trailing.Node));

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
        public static SyntaxToken Identifier(SyntaxTriviaList leading, SyntaxKind contextualKind, string text, SyntaxTriviaList trailing) =>
            new(InternalSyntax.SyntaxFactory.Identifier(contextualKind, leading.Node, text, trailing.Node));

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from an 8-byte integer value.
        /// </summary>
        /// <param name="value">The 8-byte signed integer value to be represented by the returned token.</param>
        public static SyntaxToken Literal(long value) =>
            Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.None), value);

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from a complex value.
        /// </summary>
        /// <param name="value">The complex value to be represented by the returned token.</param>
        public static SyntaxToken Literal(Complex value)
        {
            if (value.Real != 0)
            {
                throw new ArgumentException("The value cannot have a real counterpart.", nameof(value));
            }

            return Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.None), value); 
        }


        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 8-byte signed integer value.
        /// </summary>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 8-byte signed integer value to be represented by the returned token.</param>
        public static SyntaxToken Literal(string text, long value) =>
            new(InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from an 8-byte integer value.
        /// </summary>
        /// <param name="value">The 8-byte unsigned integer value to be represented by the returned token.</param>
        public static SyntaxToken Literal(ulong value) =>
            Literal(ObjectDisplay.FormatLiteral(value, ObjectDisplayOptions.None), value);

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 8-byte signed integer value.
        /// </summary>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 8-byte unsigned integer value to be represented by the returned token.</param>
        public static SyntaxToken Literal(string text, ulong value) =>
            new(InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 8-byte signed integer value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 8-byte signed integer value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Literal(SyntaxTriviaList leading, string text, long value, SyntaxTriviaList trailing) =>
            new(InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));

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
            new(InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding complex value.
        /// </summary>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The complex value to be represented by the returned token.</param>
        public static SyntaxToken Literal(string text, Complex value) 
        {
            if (value.Real != 0)
            {
                throw new ArgumentException("The value cannot have a real counterpart.", nameof(value));
            }

            return new(InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));
        }
            

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 8-byte floating point value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 8-byte unsigned integer value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Literal(SyntaxTriviaList leading, string text, ulong value, SyntaxTriviaList trailing) =>
            new(InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding complex value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The complex value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Literal(SyntaxTriviaList leading, string text, Complex value, SyntaxTriviaList trailing)
        {
            if (value.Real != 0)
            {
                throw new ArgumentException("The value cannot have a real counterpart.", nameof(value));
            }

            return new(InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));
        }

        /// <summary>
        /// Creates a token with kind NumericLiteralToken from the text and corresponding 8-byte floating point value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal.</param>
        /// <param name="value">The 8-byte floating point value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Literal(SyntaxTriviaList leading, string text, double value, SyntaxTriviaList trailing) =>
            new(InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));

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
            new(InternalSyntax.SyntaxFactory.Literal(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));

        /// <summary>
        /// Creates a token with kind StringLiteralToken from the text and corresponding string value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal, including quotes and escape sequences.</param>
        /// <param name="value">The string value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken Literal(SyntaxTriviaList leading, string text, string value, SyntaxTriviaList trailing) =>
            new(InternalSyntax.SyntaxFactory.Literal(leading.Node, text, value, trailing.Node));

        /// <summary>
        /// Creates a FiveM hash string literal token with kind <see cref="SyntaxKind.HashStringLiteralToken"/> and
        /// formats the provided string as a Lua string and calculates the hash from it.
        /// </summary>
        /// <param name="stringValue">The actual value of the string without any escapes.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="stringValue"/> is null.</exception>
        public static SyntaxToken HashLiteral(string stringValue!!)
        {
            var raw = ObjectDisplay.FormatLiteral(stringValue, ObjectDisplayOptions.EscapeNonPrintableCharacters | ObjectDisplayOptions.EscapeWithUtf8);
            raw = '`' + raw + '`';
            var hash = Hash.GetJenkinsOneAtATimeHashCode(stringValue.AsSpan());
            return new(InternalSyntax.SyntaxFactory.HashLiteral(ElasticMarker.UnderlyingNode, raw, hash, ElasticMarker.UnderlyingNode));
        }

        /// <summary>
        /// Creates a FiveM hash string literal token with kind <see cref="SyntaxKind.HashStringLiteralToken"/> from the text
        /// and corresponding string value.
        /// </summary>
        /// <param name="text">The raw text of the literal, including quotes and escape sequences.</param>
        /// <param name="value">The hash value to be represented by the returned token.</param>
        /// <returns></returns>
        public static SyntaxToken HashLiteral(string text, uint value) =>
            new(InternalSyntax.SyntaxFactory.HashLiteral(ElasticMarker.UnderlyingNode, text, value, ElasticMarker.UnderlyingNode));

        /// <summary>
        /// Creates a FiveM hash string literal token with kind <see cref="SyntaxKind.HashStringLiteralToken"/> from the text
        /// and corresponding string value.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the literal, including quotes and escape sequences.</param>
        /// <param name="value">The hash value to be represented by the returned token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        /// <returns></returns>
        public static SyntaxToken HashLiteral(SyntaxTriviaList leading, string text, uint value, SyntaxTriviaList trailing) =>
            new(InternalSyntax.SyntaxFactory.HashLiteral(leading.Node, text, value, trailing.Node));

        /// <summary>
        /// Creates a token with kind BadToken.
        /// </summary>
        /// <param name="leading">A list of trivia immediately preceding the token.</param>
        /// <param name="text">The raw text of the bad token.</param>
        /// <param name="trailing">A list of trivia immediately following the token.</param>
        public static SyntaxToken BadToken(SyntaxTriviaList leading, string text, SyntaxTriviaList trailing) =>
            new(InternalSyntax.SyntaxFactory.BadToken(leading.Node, text, trailing.Node));

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
        public static SyntaxList<TNode> SingletonList<TNode>(TNode node) where TNode : SyntaxNode => new(node);

        /// <summary>
        /// Creates a list of syntax nodes.
        /// </summary>
        /// <typeparam name="TNode">The specific type of the element nodes.</typeparam>
        /// <param name="nodes">A sequence of element nodes.</param>
        public static SyntaxList<TNode> List<TNode>(IEnumerable<TNode> nodes) where TNode : SyntaxNode => new(nodes);

        /// <summary>
        /// Creates an empty list of tokens.
        /// </summary>
        public static SyntaxTokenList TokenList() => default;

        /// <summary>
        /// Creates a singleton list of tokens.
        /// </summary>
        /// <param name="token">The single token.</param>
        public static SyntaxTokenList TokenList(SyntaxToken token) => new(token);

        /// <summary>
        /// Creates a list of tokens.
        /// </summary>
        /// <param name="tokens">An array of tokens.</param>
        public static SyntaxTokenList TokenList(params SyntaxToken[] tokens) => new(tokens);

        /// <summary>
        /// Creates a list of tokens.
        /// </summary>
        /// <param name="tokens"></param>
        /// <returns></returns>
        public static SyntaxTokenList TokenList(IEnumerable<SyntaxToken> tokens) => new(tokens);

        /// <summary>
        /// Creates a trivia from a StructuredTriviaSyntax node.
        /// </summary>
        public static SyntaxTrivia Trivia(StructuredTriviaSyntax node) => new(default, node.Green, position: 0, index: 0);

        /// <summary>
        /// Creates an empty list of trivia.
        /// </summary>
        public static SyntaxTriviaList TriviaList() => default;

        /// <summary>
        /// Creates a singleton list of trivia.
        /// </summary>
        /// <param name="trivia">A single trivia.</param>
        public static SyntaxTriviaList TriviaList(SyntaxTrivia trivia) => new(trivia);

        /// <summary>
        /// Creates a list of trivia.
        /// </summary>
        /// <param name="trivias">An array of trivia.</param>
        public static SyntaxTriviaList TriviaList(params SyntaxTrivia[] trivias)
            => new(trivias);

        /// <summary>
        /// Creates a list of trivia.
        /// </summary>
        /// <param name="trivias">A sequence of trivia.</param>
        public static SyntaxTriviaList TriviaList(IEnumerable<SyntaxTrivia> trivias)
            => new(trivias);

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
        public static SeparatedSyntaxList<TNode> SingletonSeparatedList<TNode>(TNode node) where TNode : SyntaxNode => new(new SyntaxNodeOrTokenList(node, index: 0));

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

            using var enumerator = nodes.GetEnumerator();
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
                var builder = SeparatedSyntaxListBuilder<TNode>.Create();
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
                if (element.IsNode && element.AsNode() is not TNode)
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
        public static SyntaxNodeOrTokenList NodeOrTokenList(IEnumerable<SyntaxNodeOrToken> nodesAndTokens) => new(nodesAndTokens);

        /// <summary>
        /// Create a <see cref="SyntaxNodeOrTokenList"/> from one or more <see cref="SyntaxNodeOrToken"/>.
        /// </summary>
        /// <param name="nodesAndTokens">The nodes and tokens</param>
        public static SyntaxNodeOrTokenList NodeOrTokenList(params SyntaxNodeOrToken[] nodesAndTokens) => new(nodesAndTokens);

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

        /// <inheritdoc cref="LuaSyntaxTree.ParseText(string, LuaParseOptions?, string, Encoding?, CancellationToken)"/>
        public static SyntaxTree ParseSyntaxTree(
            string text,
            ParseOptions? options = null,
            string path = "",
            Encoding? encoding = null,
            CancellationToken cancellationToken = default) =>
            LuaSyntaxTree.ParseText(text, (LuaParseOptions?) options, path, encoding, cancellationToken);

        /// <inheritdoc cref="LuaSyntaxTree.ParseText(SourceText, LuaParseOptions?, string, CancellationToken)"/>
        public static SyntaxTree ParseSyntaxTree(
            SourceText text,
            ParseOptions? options = null,
            string path = "",
            CancellationToken cancellationToken = default) =>
            LuaSyntaxTree.ParseText(text, (LuaParseOptions?) options, path, cancellationToken);

        /// <summary>
        /// Parse a list of trivia rules for leading trivia.
        /// </summary>
        public static SyntaxTriviaList ParseLeadingTrivia(string text, LuaParseOptions? options, int offset = 0) =>
            ParseLeadingTrivia(MakeSourceText(text, offset), options);

        /// <summary>
        /// Parse a list of trivia rules for leading trivia.
        /// </summary>
        public static SyntaxTriviaList ParseLeadingTrivia(SourceText text, LuaParseOptions? options)
        {
            using var lexer = MakeLexer(text, options);
            return lexer.LexSyntaxLeadingTrivia();
        }

        /// <summary>
        /// Parse a list of trivia using the parsing rules for trailing trivia.
        /// </summary>
        public static SyntaxTriviaList ParseTrailingTrivia(string text, LuaParseOptions? options, int offset = 0) =>
            ParseTrailingTrivia(MakeSourceText(text, offset), options);

        /// <summary>
        /// Parse a list of trivia using the parsing rules for trailing trivia.
        /// </summary>
        public static SyntaxTriviaList ParseTrailingTrivia(SourceText text, LuaParseOptions? options)
        {
            using var lexer = MakeLexer(text, options);
            return lexer.LexSyntaxTrailingTrivia();
        }

        /// <summary>
        /// Parse a Lua language token.
        /// </summary>
        /// <param name="text">The text of the token including leading and trailing trivia.</param>
        /// <param name="offset">Optional offset into text.</param>
        /// <param name="options">Parse options.</param>
        public static SyntaxToken ParseToken(string text, int offset = 0, LuaParseOptions? options = null) =>
            ParseToken(MakeSourceText(text, offset), options);

        /// <summary>
        /// Parse a Lua language token.
        /// </summary>
        /// <param name="text">The text of the token including leading and trailing trivia.</param>
        /// <param name="options">Parse options.</param>
        public static SyntaxToken ParseToken(SourceText text, LuaParseOptions? options = null)
        {
            using var lexer = MakeLexer(text, options);
            return new SyntaxToken(lexer.Lex());
        }

        /// <summary>
        /// Parse a sequence of Lua language tokens.
        /// </summary>
        /// <param name="text">The text of all the tokens.</param>
        /// <param name="initialTokenPosition">An integer to use as the starting position of the first token.</param>
        /// <param name="offset">Optional offset into text.</param>
        /// <param name="options">Parse options.</param>
        public static IEnumerable<SyntaxToken> ParseTokens(string text, int offset = 0, int initialTokenPosition = 0, LuaParseOptions? options = null) =>
            ParseTokens(MakeSourceText(text, offset), initialTokenPosition, options);

        /// <summary>
        /// Parse a sequence of Lua language tokens.
        /// </summary>
        /// <param name="text">The text of all the tokens.</param>
        /// <param name="initialTokenPosition">An integer to use as the starting position of the first token.</param>
        /// <param name="options">Parse options.</param>
        public static IEnumerable<SyntaxToken> ParseTokens(SourceText text, int initialTokenPosition = 0, LuaParseOptions? options = null)
        {
            using var lexer = MakeLexer(text, options);
            var position = initialTokenPosition;
            while (true)
            {
                var token = lexer.Lex();
                yield return new SyntaxToken(parent: null, token: token, position: position, index: 0);

                position += token.FullWidth;

                if (token.Kind == SyntaxKind.EndOfFileToken)
                {
                    break;
                }
            }
        }

        /// <summary>
        /// Parse an ExpressionSyntax node using the lowest precedence grammar rule for expressions.
        /// </summary>
        /// <param name="text">The text of the expression.</param>
        /// <param name="offset">Optional offset into text.</param>
        /// <param name="options">The optional parse options to use. If no options are specified default options are
        /// used.</param>
        /// <param name="consumeFullText">True if extra tokens in the input should be treated as an error</param>
        public static ExpressionSyntax ParseExpression(string text, int offset = 0, LuaParseOptions? options = null, bool consumeFullText = true) =>
            ParseExpression(MakeSourceText(text, offset), options, consumeFullText);

        /// <summary>
        /// Parse an ExpressionSyntax node using the lowest precedence grammar rule for expressions.
        /// </summary>
        /// <param name="text">The text of the expression.</param>
        /// <param name="options">The optional parse options to use. If no options are specified default options are
        /// used.</param>
        /// <param name="consumeFullText">True if extra tokens in the input should be treated as an error</param>
        public static ExpressionSyntax ParseExpression(SourceText text, LuaParseOptions? options = null, bool consumeFullText = true)
        {
            using var lexer = MakeLexer(text, options);
            using var parser = MakeParser(lexer);

            var node = parser.ParseExpression();
            if (consumeFullText) node = parser.ConsumeUnexpectedTokens(node);
            return (ExpressionSyntax) node.CreateRed();
        }

        /// <summary>
        /// Parse a StatementSyntaxNode using grammar rule for statements.
        /// </summary>
        /// <param name="text">The text of the statement.</param>
        /// <param name="offset">Optional offset into text.</param>
        /// <param name="options">The optional parse options to use. If no options are specified default options are
        /// used.</param>
        /// <param name="consumeFullText">True if extra tokens in the input should be treated as an error</param>
        public static StatementSyntax ParseStatement(string text, int offset = 0, LuaParseOptions? options = null, bool consumeFullText = true) =>
            ParseStatement(MakeSourceText(text, offset), options, consumeFullText);

        /// <summary>
        /// Parse a StatementSyntaxNode using grammar rule for statements.
        /// </summary>
        /// <param name="text">The text of the statement.</param>
        /// <param name="options">The optional parse options to use. If no options are specified default options are
        /// used.</param>
        /// <param name="consumeFullText">True if extra tokens in the input should be treated as an error</param>
        public static StatementSyntax ParseStatement(SourceText text, LuaParseOptions? options = null, bool consumeFullText = true)
        {
            using var lexer = MakeLexer(text, options);
            using var parser = MakeParser(lexer);
            var node = parser.ParseStatement();
            if (consumeFullText) node = parser.ConsumeUnexpectedTokens(node);
            return (StatementSyntax) node.CreateRed();
        }

        /// <summary>
        /// Parse a <see cref="TypeSyntax"/> using the grammar rule for types.
        /// </summary>
        /// <param name="text">The text of the type.</param>
        /// <param name="offset">Optional offset into text.</param>
        /// <param name="options">
        /// The optional parse options to use.
        /// If no options are specified default options are used.
        /// </param>
        /// <param name="consumeFullText">
        /// Whether all text should be consumed and an error generated if it's not.
        /// </param>
        /// <returns></returns>
        public static TypeSyntax ParseType(
            string text,
            int offset = 0,
            LuaParseOptions? options = null,
            bool consumeFullText = true) =>
            ParseType(MakeSourceText(text, offset), options, consumeFullText);

        /// <summary>
        /// Parse a <see cref="TypeSyntax"/> using the grammar rule for types.
        /// </summary>
        /// <param name="text">The text of the type.</param>
        /// <param name="options">
        /// The optional parse options to use.
        /// If no options are specified default options are used.
        /// </param>
        /// <param name="consumeFullText">
        /// Whether all text should be consumed and an error generated if it's not.
        /// </param>
        /// <returns></returns>
        public static TypeSyntax ParseType(
            SourceText text,
            LuaParseOptions? options = null,
            bool consumeFullText = true)
        {
            var lexer = MakeLexer(text, options);
            var parser = MakeParser(lexer);
            var node = parser.ParseType();
            if (consumeFullText) node = parser.ConsumeUnexpectedTokens(node);
            return (TypeSyntax) node.CreateRed();
        }

        /// <summary>
        /// Parse a <see cref="CompilationUnitSyntax"/> using the grammar rule for an entire compilation unit (file). To produce a
        /// <see cref="CodeAnalysis.SyntaxTree"/> instance, use <see cref="LuaSyntaxTree.ParseText(string, LuaParseOptions?, string, Encoding?, CancellationToken)"/>
        /// instead.
        /// </summary>
        /// <param name="text">The text of the compilation unit.</param>
        /// <param name="offset">Optional offset into text.</param>
        /// <param name="options">The optional parse options to use. If no options are specified default options are
        /// used.</param>
        public static CompilationUnitSyntax ParseCompilationUnit(string text, int offset = 0, LuaParseOptions? options = null) =>
            ParseCompilationUnit(MakeSourceText(text, offset), options);

        /// <summary>
        /// Parse a <see cref="CompilationUnitSyntax"/> using the grammar rule for an entire compilation unit (file). To produce a
        /// <see cref="CodeAnalysis.SyntaxTree"/> instance, use <see cref="LuaSyntaxTree.ParseText(SourceText, LuaParseOptions?, string, CancellationToken)"/>
        /// instead.
        /// </summary>
        /// <param name="text">The text of the compilation unit.</param>
        /// <param name="options">The optional parse options to use. If no options are specified default options are
        /// used.</param>
        public static CompilationUnitSyntax ParseCompilationUnit(SourceText text, LuaParseOptions? options = null)
        {
            // note that we do not need a "consumeFullText" parameter, because parsing a compilation unit always must
            // consume input until the end-of-file
            using var lexer = MakeLexer(text, options);
            using var parser = MakeParser(lexer);
            var node = parser.ParseCompilationUnit();
            return (CompilationUnitSyntax) node.CreateRed();
        }

        /// <summary>
        /// Helper method for wrapping a string in a SourceText.
        /// </summary>
        private static SourceText MakeSourceText(string text, int offset) =>
            SourceText.From(text, Encoding.UTF8).GetSubText(offset);

        private static InternalSyntax.Lexer MakeLexer(SourceText text, LuaParseOptions? options = null) =>
            new(text: text, options: options ?? LuaParseOptions.Default);

        private static InternalSyntax.LanguageParser MakeParser(InternalSyntax.Lexer lexer) =>
            new(lexer, oldTree: null, changes: null);

        /// <summary>
        /// Determines if two trees are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldTree">The original tree.</param>
        /// <param name="newTree">The new tree.</param>
        /// <param name="topLevel"> 
        /// If true then the trees are equivalent if the contained nodes and tokens declaring
        /// metadata visible symbolic information are equivalent, ignoring any differences of nodes inside method bodies
        /// or initializer expressions, otherwise all nodes and tokens must be equivalent. 
        /// </param>
        public static bool AreEquivalent(SyntaxTree? oldTree, SyntaxTree? newTree, bool topLevel)
        {
            if (oldTree == null && newTree == null)
            {
                return true;
            }

            if (oldTree == null || newTree == null)
            {
                return false;
            }

            return SyntaxEquivalence.AreEquivalent(oldTree, newTree, ignoreChildNode: null, topLevel: topLevel);
        }

        /// <summary>
        /// Determines if two syntax nodes are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldNode">The old node.</param>
        /// <param name="newNode">The new node.</param>
        /// <param name="topLevel"> 
        /// If true then the nodes are equivalent if the contained nodes and tokens declaring
        /// metadata visible symbolic information are equivalent, ignoring any differences of nodes inside method bodies
        /// or initializer expressions, otherwise all nodes and tokens must be equivalent. 
        /// </param>
        public static bool AreEquivalent(SyntaxNode? oldNode, SyntaxNode? newNode, bool topLevel) =>
            SyntaxEquivalence.AreEquivalent(oldNode, newNode, ignoreChildNode: null, topLevel: topLevel);

        /// <summary>
        /// Determines if two syntax nodes are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldNode">The old node.</param>
        /// <param name="newNode">The new node.</param>
        /// <param name="ignoreChildNode">
        /// If specified called for every child syntax node (not token) that is visited during the comparison. 
        /// If it returns true the child is recursively visited, otherwise the child and its subtree is disregarded.
        /// </param>
        public static bool AreEquivalent(SyntaxNode? oldNode, SyntaxNode? newNode, Func<SyntaxKind, bool>? ignoreChildNode = null) =>
            SyntaxEquivalence.AreEquivalent(oldNode, newNode, ignoreChildNode: ignoreChildNode, topLevel: false);

        /// <summary>
        /// Determines if two syntax tokens are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldToken">The old token.</param>
        /// <param name="newToken">The new token.</param>
        public static bool AreEquivalent(SyntaxToken oldToken, SyntaxToken newToken) =>
            SyntaxEquivalence.AreEquivalent(oldToken, newToken);

        /// <summary>
        /// Determines if two lists of tokens are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldList">The old token list.</param>
        /// <param name="newList">The new token list.</param>
        public static bool AreEquivalent(SyntaxTokenList oldList, SyntaxTokenList newList) =>
            SyntaxEquivalence.AreEquivalent(oldList, newList);

        /// <summary>
        /// Determines if two lists of syntax nodes are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldList">The old list.</param>
        /// <param name="newList">The new list.</param>
        /// <param name="topLevel"> 
        /// If true then the nodes are equivalent if the contained nodes and tokens declaring
        /// metadata visible symbolic information are equivalent, ignoring any differences of nodes inside method bodies
        /// or initializer expressions, otherwise all nodes and tokens must be equivalent. 
        /// </param>
        public static bool AreEquivalent<TNode>(SyntaxList<TNode> oldList, SyntaxList<TNode> newList, bool topLevel)
            where TNode : LuaSyntaxNode =>
            SyntaxEquivalence.AreEquivalent(oldList.Node, newList.Node, null, topLevel);

        /// <summary>
        /// Determines if two lists of syntax nodes are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldList">The old list.</param>
        /// <param name="newList">The new list.</param>
        /// <param name="ignoreChildNode">
        /// If specified called for every child syntax node (not token) that is visited during the comparison. 
        /// If it returns true the child is recursively visited, otherwise the child and its subtree is disregarded.
        /// </param>
        public static bool AreEquivalent<TNode>(SyntaxList<TNode> oldList, SyntaxList<TNode> newList, Func<SyntaxKind, bool>? ignoreChildNode = null)
            where TNode : SyntaxNode =>
            SyntaxEquivalence.AreEquivalent(oldList.Node, newList.Node, ignoreChildNode, topLevel: false);

        /// <summary>
        /// Determines if two lists of syntax nodes are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldList">The old list.</param>
        /// <param name="newList">The new list.</param>
        /// <param name="topLevel"> 
        /// If true then the nodes are equivalent if the contained nodes and tokens declaring
        /// metadata visible symbolic information are equivalent, ignoring any differences of nodes inside method bodies
        /// or initializer expressions, otherwise all nodes and tokens must be equivalent. 
        /// </param>
        public static bool AreEquivalent<TNode>(SeparatedSyntaxList<TNode> oldList, SeparatedSyntaxList<TNode> newList, bool topLevel)
            where TNode : SyntaxNode =>
            SyntaxEquivalence.AreEquivalent(oldList.Node, newList.Node, null, topLevel);

        /// <summary>
        /// Determines if two lists of syntax nodes are the same, disregarding trivia differences.
        /// </summary>
        /// <param name="oldList">The old list.</param>
        /// <param name="newList">The new list.</param>
        /// <param name="ignoreChildNode">
        /// If specified called for every child syntax node (not token) that is visited during the comparison. 
        /// If it returns true the child is recursively visited, otherwise the child and its subtree is disregarded.
        /// </param>
        public static bool AreEquivalent<TNode>(SeparatedSyntaxList<TNode> oldList, SeparatedSyntaxList<TNode> newList, Func<SyntaxKind, bool>? ignoreChildNode = null)
            where TNode : SyntaxNode =>
            SyntaxEquivalence.AreEquivalent(oldList.Node, newList.Node, ignoreChildNode, topLevel: false);

        /// <summary>
        /// Creates a new <see cref="StatementListSyntax"/> instance.
        /// </summary>
        /// <param name="statements"></param>
        /// <returns></returns>
        public static StatementListSyntax StatementList(params StatementSyntax[] statements) =>
            StatementList(List(statements));

        /// <summary>
        /// Creates a new <see cref="StatementListSyntax"/> instance.
        /// </summary>
        /// <param name="statements"></param>
        /// <returns></returns>
        public static StatementListSyntax StatementList(IEnumerable<StatementSyntax> statements) =>
            StatementList(List(statements));

        /// <summary>
        /// Creates a new <see cref="AssignmentStatementSyntax"/> node.
        /// </summary>
        public static AssignmentStatementSyntax AssignmentStatement(SeparatedSyntaxList<PrefixExpressionSyntax> variables, SeparatedSyntaxList<ExpressionSyntax> values) =>
            AssignmentStatement(variables, EqualsValuesClause(values));

        /// <summary>
        /// Creates a new <see cref="LocalVariableDeclarationStatementSyntax"/> node.
        /// </summary>
        public static LocalVariableDeclarationStatementSyntax LocalVariableDeclarationStatement(
            SeparatedSyntaxList<LocalDeclarationNameSyntax> names,
            SeparatedSyntaxList<ExpressionSyntax> values) =>
            LocalVariableDeclarationStatement(names, EqualsValuesClause(values));
    }
}
