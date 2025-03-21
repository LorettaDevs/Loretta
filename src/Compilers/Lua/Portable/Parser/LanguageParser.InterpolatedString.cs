using System.Diagnostics;
using Loretta.CodeAnalysis.PooledObjects;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal sealed partial class LanguageParser
    {
        private ExpressionSyntax ParseInterpolatedStringToken()
        {
            // We don't want to make the scanner stateful (between tokens) if we can possibly avoid it.
            // The approach implemented here is
            //
            // (1) Scan the whole interpolated string literal as a single token. Now the statefulness of
            // the scanner (to match { }'s) is limited to its behavior while scanning a single token.
            //
            // (2) When the parser gets such a token, here, it spins up another scanner / parser on each of
            // the holes and builds a tree for the whole thing (resulting in an InterpolatedStringExpressionSyntax).
            //
            // (3) The parser discards the original token and replaces it with this tree. (In other words,
            // it replaces one token with a different set of tokens that have already been parsed)
            //
            // (4) On an incremental change, we widen the invalidated region to include any enclosing interpolated
            // string non-terminal so that we never reuse tokens inside a changed interpolated string.
            //
            // This has the secondary advantage that it can reasonably be specified.
            // 
            // The substitution will end up being invisible to external APIs and clients such as the IDE, as
            // they have no way to ask for the stream of tokens before parsing.

            LorettaDebug.Assert(CurrentToken.Kind == SyntaxKind.InterpolatedStringToken);
            var originalToken = EatToken();

            var originalText = originalToken.ValueText; // this is actually the source text
            LorettaDebug.AssertNotNull(originalText);
            var originalTextSpan = originalText.AsSpan();
            LorettaDebug.Assert(originalText[0] == '`');

            // compute the positions of the interpolations in the original string literal, if there was an error or not,
            // and where the open and close quotes can be found.
            var interpolations = ArrayBuilder<Lexer.Interpolation>.GetInstance();

            RescanInterpolation(out var error, out var openQuoteRange, interpolations, out var closeQuoteRange);

            var result = SyntaxFactory.InterpolatedStringExpression(
                GetOpenQuote(),
                GetContent(originalTextSpan),
                GetCloseQuote());

            interpolations.Free();
            if (error != null)
            {
                // Errors are positioned relative to the start of the token that was lexed.  Specifically relative to
                // the starting '`'.  However, when placed on a node like this, it will be relative to the node's full
                // start.  So we have to adjust the diagnostics taking that into account.
                result = result.WithDiagnosticsGreen(
                    MoveDiagnostics([error], originalToken.GetLeadingTrivia()?.FullWidth ?? 0));
            }
            if (Options.SyntaxOptions.BacktickStringType != BacktickStringType.InterpolatedStringLiteral)
                result = AddError(result, ErrorCode.ERR_InterpolatedStringsNotSupportedInVersion);

            Debug.Assert(
                originalToken.ToFullString() == result.ToFullString()); // yield from text equals yield from node
            return result;

            void RescanInterpolation(
                // ReSharper disable VariableHidesOuterVariable
                out SyntaxDiagnosticInfo?         error,
                out Range                         openQuoteRange,
                ArrayBuilder<Lexer.Interpolation> interpolations,
                out Range                         closeQuoteRange
                // ReSharper restore VariableHidesOuterVariable
            )
            {
                using var tempLexer = new Lexer(SourceText.From(originalText), Options);
                var       info      = default(Lexer.TokenInfo);
                tempLexer.ScanInterpolatedStringLiteralTop(
                    ref info,
                    out error,
                    out openQuoteRange,
                    interpolations,
                    out closeQuoteRange);
            }

            SyntaxToken GetOpenQuote()
            {
                return SyntaxFactory.Token(
                    originalToken.GetLeadingTrivia(),
                    SyntaxKind.BacktickToken,
                    originalText[openQuoteRange],
                    trailing: null);
            }

            CodeAnalysis.Syntax.InternalSyntax.SyntaxList<InterpolatedStringContentSyntax> GetContent(
                ReadOnlySpan<char> originalTextSpan)
            {
                var builder = _pool.Allocate<InterpolatedStringContentSyntax>();

                var currentContentStart = openQuoteRange.End;
                foreach (Lexer.Interpolation interpolation in interpolations)
                {
                    // Add a token for text preceding the interpolation
                    builder.Add(MakeContent(originalTextSpan[currentContentStart..interpolation.OpenBraceRange.Start]));

                    // Now parse the interpolation itself.
                    var interpolationNode = ParseInterpolation(Options, originalText, interpolation);

                    builder.Add(interpolationNode);
                    currentContentStart = interpolation.CloseBraceRange.End;
                }

                // Add a token for text following the last interpolation
                builder.Add(MakeContent(originalTextSpan[currentContentStart..closeQuoteRange.Start]));

                // ReSharper disable once VariableHidesOuterVariable
                CodeAnalysis.Syntax.InternalSyntax.SyntaxList<InterpolatedStringContentSyntax> result = builder;
                _pool.Free(builder);
                return result;
            }

            InterpolatedStringContentSyntax? MakeContent(ReadOnlySpan<char> text)
            {
                return text.Length == 0
                           ? null
                           : SyntaxFactory.InterpolatedStringText(MakeInterpolatedStringTextToken(text.ToString()));
            }

            SyntaxToken GetCloseQuote()
            {
                // Make a token for the close quote ` (even if it was missing)
                return TokenOrMissingToken(
                    leading: null,
                    SyntaxKind.BacktickToken,
                    originalText[closeQuoteRange],
                    originalToken.GetTrailingTrivia());
            }
        }

        private static SyntaxToken TokenOrMissingToken(
            GreenNode? leading,
            SyntaxKind kind,
            string     text,
            GreenNode? trailing)
            => text == ""
                   ? SyntaxFactory.MissingToken(leading, kind, trailing)
                   : SyntaxFactory.Token(leading, kind, text, trailing);

        private static InterpolationSyntax ParseInterpolation(
            LuaParseOptions     options,
            string              text,
            Lexer.Interpolation interpolation)
        {
            // Grab the text from after the { all the way to the start of the } (or the start of the : if present). This
            // will be used to parse out the expression of the interpolation.
            //
            // The parsing of the open brace, close brace and colon is specially handled in ParseInterpolation below.
            var expressionText = text[interpolation.OpenBraceRange.End..interpolation.CloseBraceRange.Start];

            using var tempLexer = new Lexer(SourceText.From(expressionText), options);

            // First grab any trivia right after the {, it will be trailing trivia for the { token.
            var openTokenTrailingTrivia = tempLexer.LexSyntaxTrailingTrivia().Node;

            // Now create a parser to actually handle the expression portion of the interpolation
            using var tempParser = new LanguageParser(tempLexer, oldTree: null, changes: null);

            var result = tempParser.ParseInterpolation(
                text,
                interpolation,
                SyntaxFactory.Token(
                    leading: null,
                    SyntaxKind.OpenBraceToken,
                    text[interpolation.OpenBraceRange],
                    openTokenTrailingTrivia));

            Debug.Assert(
                text[interpolation.OpenBraceRange.Start..interpolation.CloseBraceRange.End]
                == result.ToFullString()); // yield from text equals yield from node
            return result;
        }

        private InterpolationSyntax ParseInterpolation(
            string              text,
            Lexer.Interpolation interpolation,
            SyntaxToken         openBraceToken)
        {
            var expression = ParseExpressionCore();
            expression = ConsumeUnexpectedTokens(expression);
            var closeBraceToken = TokenOrMissingToken(
                CurrentToken.GetLeadingTrivia(),
                SyntaxKind.CloseBraceToken,
                text[interpolation.CloseBraceRange],
                trailing: null);

            var result = SyntaxFactory.Interpolation(openBraceToken, expression, closeBraceToken);
#if DEBUG
            Debug.Assert(
                text[interpolation.OpenBraceRange.Start..interpolation.CloseBraceRange.End]
                == result.ToFullString()); // yield from text equals yield from node
#endif
            return result;
        }

        /// <summary>
        /// Interpret the given raw text from source as an InterpolatedStringTextToken.
        /// </summary>
        /// <param name="text">The text for the full string literal, including the quotes and contents</param>
        private SyntaxToken MakeInterpolatedStringTextToken(string text)
        {
            const string prefix = "\"";

            // For a normal/verbatim piece of content, process the inner content as if it was in a corresponding
            // *non*-interpolated string to get the correct meaning of all the escapes/diagnostics within.
            var       fakeString = prefix + text.Replace("\"", "\\\"") + "\"";
            using var tempLexer  = new Lexer(SourceText.From(fakeString), Options);
            var       token      = tempLexer.Lex();
            Debug.Assert(token.Kind == SyntaxKind.StringLiteralToken);
            var result = SyntaxFactory.Literal(
                leading: null,
                text,
                SyntaxKind.InterpolatedStringTextToken,
                token.ValueText!,
                trailing: null);
            if (token.ContainsDiagnostics)
                result = result.WithDiagnosticsGreen(MoveDiagnostics(token.GetDiagnostics(), -prefix.Length));

            return result;
        }

        private static DiagnosticInfo[] MoveDiagnostics(DiagnosticInfo[] infos, int offset)
        {
            Debug.Assert(infos.Length > 0);
            var builder = ArrayBuilder<DiagnosticInfo>.GetInstance(infos.Length);
            foreach (var info in infos)
            {
                // This cast should always be safe.  We are only moving diagnostics produced on syntax nodes and tokens.
                var sd = (SyntaxDiagnosticInfo) info;
                builder.Add(sd.WithOffset(sd.Offset + offset));
            }

            return builder.ToArrayAndFree();
        }
    }
}
