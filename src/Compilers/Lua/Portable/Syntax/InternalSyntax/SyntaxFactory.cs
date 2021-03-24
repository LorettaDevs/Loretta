using System;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal static partial class SyntaxFactory
    {
        private const string Cr = "\r";
        private const string Lf = "\n";
        private const string CrLf = Cr + Lf;
        internal static readonly SyntaxTrivia CarriageReturnLineFeed = EndOfLine(CrLf);
        internal static readonly SyntaxTrivia LineFeed = EndOfLine(Lf);
        internal static readonly SyntaxTrivia CarriageReturn = EndOfLine(Cr);
        internal static readonly SyntaxTrivia Space = Whitespace(" ");
        internal static readonly SyntaxTrivia Tab = Whitespace("\t");

        internal static readonly SyntaxTrivia ElasticCarriageReturnLineFeed = EndOfLine(CrLf, elastic: true);
        internal static readonly SyntaxTrivia ElasticLineFeed = EndOfLine(Lf, elastic: true);
        internal static readonly SyntaxTrivia ElasticCarriageReturn = EndOfLine(Cr, elastic: true);
        internal static readonly SyntaxTrivia ElasticSpace = Whitespace(" ", elastic: true);
        internal static readonly SyntaxTrivia ElasticTab = Whitespace("\t", elastic: true);

        internal static readonly SyntaxTrivia ElasticZeroSpace = Whitespace(string.Empty, elastic: true);

        internal static SyntaxTrivia EndOfLine(string text, bool elastic = false)
        {
            SyntaxTrivia? trivia = null;

            // use predefined trivia
            switch (text)
            {
                case "\r":
                    trivia = elastic ? ElasticCarriageReturn : CarriageReturn;
                    break;
                case "\n":
                    trivia = elastic ? ElasticLineFeed : LineFeed;
                    break;
                case "\r\n":
                    trivia = elastic ? ElasticCarriageReturnLineFeed : CarriageReturnLineFeed;
                    break;
            }

            // note: predefined trivia might not yet be defined during initialization
            if (trivia != null)
            {
                return trivia;
            }

            trivia = SyntaxTrivia.Create(SyntaxKind.EndOfLineTrivia, text);
            if (!elastic)
            {
                return trivia;
            }

            return trivia.WithAnnotationsGreen(new[] { SyntaxAnnotation.ElasticAnnotation });
        }

        internal static SyntaxTrivia Whitespace(string text, bool elastic = false)
        {
            var trivia = SyntaxTrivia.Create(SyntaxKind.WhitespaceTrivia, text);
            if (!elastic)
            {
                return trivia;
            }

            return trivia.WithAnnotationsGreen(new[] { SyntaxAnnotation.ElasticAnnotation });
        }

        internal static SyntaxTrivia Comment(string text)
        {
            return isLongComment(text)
                ? SyntaxTrivia.Create(SyntaxKind.MultiLineCommentTrivia, text)
                : SyntaxTrivia.Create(SyntaxKind.SingleLineCommentTrivia, text);

            static bool isLongComment(string text)
            {
                if (text.StartsWith("/*", StringComparison.Ordinal))
                    return true;
                if (text.StartsWith("--[", StringComparison.Ordinal))
                {
                    var offset = 3;
                    while (offset < text.Length && text[offset] == '=') offset++;
                    return offset < text.Length && text[offset] == '[';
                }
                return false;
            }
        }

        internal static SyntaxTrivia Shebang(string text) => SyntaxTrivia.Create(SyntaxKind.ShebangTrivia, text);

        public static SyntaxToken Token(SyntaxKind kind) => SyntaxToken.Create(kind);

        internal static SyntaxToken Token(GreenNode? leading, SyntaxKind kind, GreenNode? trailing) => SyntaxToken.Create(kind, leading, trailing);

        internal static SyntaxToken Token(GreenNode? leading, SyntaxKind kind, string text, string valueText, GreenNode? trailing)
        {
            RoslynDebug.Assert(SyntaxFacts.IsToken(kind));
            RoslynDebug.Assert(kind != SyntaxKind.IdentifierToken);
            RoslynDebug.Assert(kind != SyntaxKind.NumericLiteralToken);

            var defaultText = SyntaxFacts.GetText(kind);
            return kind >= SyntaxToken.FirstTokenWithWellKnownText && kind <= SyntaxToken.LastTokenWithWellKnownText && text == defaultText && valueText == defaultText
                   ? Token(leading, kind, trailing)
                   : SyntaxToken.WithValue(kind, leading, text, valueText, trailing);
        }

        internal static SyntaxToken MissingToken(SyntaxKind kind) =>
            SyntaxToken.CreateMissing(kind, null, null);

        internal static SyntaxToken MissingToken(GreenNode? leading, SyntaxKind kind, GreenNode? trailing) =>
            SyntaxToken.CreateMissing(kind, leading, trailing);

        internal static SyntaxToken Identifier(string text) =>
            Identifier(SyntaxKind.IdentifierToken, null, text, null);

        internal static SyntaxToken Identifier(GreenNode? leading, string text, GreenNode? trailing) =>
            Identifier(SyntaxKind.IdentifierToken, leading, text, trailing);

        internal static SyntaxToken Identifier(SyntaxKind contextualKind, GreenNode? leading, string text, GreenNode? trailing) =>
            SyntaxToken.Identifier(contextualKind, leading, text, trailing);

        internal static SyntaxToken Literal(GreenNode? leading, string text, double value, GreenNode? trailing) =>
            SyntaxToken.WithValue(SyntaxKind.NumericLiteralToken, leading, text, value, trailing);

        internal static SyntaxToken Literal(GreenNode? leading, string text, string value, GreenNode? trailing) =>
            SyntaxToken.WithValue(SyntaxKind.StringLiteralToken, leading, text, value, trailing);

        internal static SyntaxToken Literal(GreenNode? leading, string text, SyntaxKind kind, string value, GreenNode? trailing) =>
            SyntaxToken.WithValue(kind, leading, text, value, trailing);

        internal static SyntaxToken BadToken(GreenNode? leading, string text, GreenNode? trailing) =>
            SyntaxToken.WithValue(SyntaxKind.BadToken, leading, text, text, trailing);

        internal static StatementListSyntax StatementList() =>
            StatementList(default);
    }
}
