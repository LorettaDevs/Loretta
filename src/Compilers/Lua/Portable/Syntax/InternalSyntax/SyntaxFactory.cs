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
    }
}
