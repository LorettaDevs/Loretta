using Loretta.CodeAnalysis.Lua.Utilities;
using Loretta.CodeAnalysis.PooledObjects;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal sealed partial class Lexer
    {
        private void ScanStringLiteral(ref TokenInfo info)
        {
            _builder.Clear();
            var quote = TextWindow.NextChar();
            LorettaDebug.Assert(quote is '"' or '\'' or '`');

            char ch;
            while (!IsAtEnd(ch = TextWindow.PeekChar()) && ch != quote)
            {
                var charStart = TextWindow.Position;
                switch (ch)
                {
                    case '\\':
                    {
                        var high = ScanEscapeSequence(out var low);
                        if (high != SlidingTextWindow.InvalidCharacter)
                        {
                            _builder.Append(high);
                            if (low != SlidingTextWindow.InvalidCharacter) _builder.Append(low);
                        }
                    }
                        break;

                    case '\n':
                    case '\r':
                    {
                        _builder.Append(TextWindow.NextChar());
                        char ch2;
                        if (CharUtils.IsNewLine(ch2 = TextWindow.PeekChar()) && ch != ch2)
                            _builder.Append(TextWindow.NextChar());

                        AddError(charStart, TextWindow.Position - charStart, ErrorCode.ERR_UnescapedLineBreakInString);
                    }
                        break;

                    default: _builder.Append(TextWindow.NextChar()); break;
                }
            }

            if (TextWindow.PeekChar() == quote)
                TextWindow.AdvanceChar();
            else
                AddError(ErrorCode.ERR_UnfinishedString);

            var stringValue = TextWindow.Intern(_builder);
            if (quote == '`')
            {
                // Jenkins' one-at-a-time hash doesn't do this but FiveM does.
                stringValue = stringValue.ToLowerInvariant();

                info.Kind      = SyntaxKind.HashStringLiteralToken;
                info.ValueKind = ValueKind.UInt;
                info.UIntValue = Hash.GetJenkinsOneAtATimeHashCode(stringValue.AsSpan());
            }
            else
            {
                info.Kind        = SyntaxKind.StringLiteralToken;
                info.ValueKind   = ValueKind.String;
                info.StringValue = stringValue;
            }
            info.Text = TextWindow.GetText(intern: true);
        }

        private void ScanInterpolatedStringLiteral(ref TokenInfo info)
        {
            // We have a string of the following form
            //                ` ... `
            // Where the contents contains zero or more sequences
            //                { STUFF }
            // where these curly braces delimit STUFF in expression "holes".
            // In order to properly find the closing quote of the whole string,
            // we need to locate the closing brace of each hole, as strings
            // may appear in expressions in the holes. So we
            // need to match up any braces that appear between them.
            // But in order to do that, we also need to match up any
            // /**/ comments, ' characters quotes, () parens
            // [] brackets, and "" strings, including interpolated holes in the latter.

            ScanInterpolatedStringLiteralTop(ref info, out var error, out _, interpolations: null, out _);
            AddError(error);
            if (_options.SyntaxOptions.BacktickStringType != BacktickStringType.InterpolatedStringLiteral)
                AddError(ErrorCode.ERR_InterpolatedStringsNotSupportedInVersion);
        }

        internal void ScanInterpolatedStringLiteralTop(
            ref TokenInfo                info,
            out SyntaxDiagnosticInfo?    error,
            out Range                    openQuoteRange,
            ArrayBuilder<Interpolation>? interpolations,
            out Range                    closeQuoteRange)
        {
            var subScanner = new InterpolatedStringScanner(this);
            subScanner.ScanInterpolatedStringLiteralTop(out openQuoteRange, interpolations, out closeQuoteRange);
            error     = subScanner.Error;
            info.Kind = SyntaxKind.InterpolatedStringToken;
            info.Text = TextWindow.GetText(intern: false);
        }

        /// <summary>Turn a (parsed) interpolated string non-terminal into an interpolated string token.</summary>
        /// <param name="interpolatedString"></param>
        internal static SyntaxToken RescanInterpolatedString(InterpolatedStringExpressionSyntax interpolatedString)
        {
            var text = interpolatedString.ToString();
            // TODO: scan the contents (perhaps using ScanInterpolatedStringLiteralContents) to reconstruct any lexical
            // errors such as // inside an expression hole
            return SyntaxFactory.Literal(
                interpolatedString.GetFirstToken()!.GetLeadingTrivia(),
                text,
                SyntaxKind.InterpolatedStringToken,
                text,
                interpolatedString.GetLastToken()!.GetTrailingTrivia());
        }

        private char ScanEscapeSequence(out char lowSurrogateCharacter)
        {
            var start = TextWindow.Position;
            lowSurrogateCharacter = SlidingTextWindow.InvalidCharacter;

            var ch = TextWindow.NextChar();
            LorettaDebug.Assert(ch == '\\');

            ch = TextWindow.NextChar();
            switch (ch)
            {
                case '\\':
                case '\'':
                case '"':
                    break;

                case 'a': ch = '\a'; break;
                case 'b': ch = '\b'; break;
                case 'f': ch = '\f'; break;
                case 'n': ch = '\n'; break;
                case 'r': ch = '\r'; break;
                case 't': ch = '\t'; break;
                case 'v': ch = '\v'; break;

                case '\n':
                case '\r':
                {
                    char ch2;
                    if (CharUtils.IsNewLine(ch2 = TextWindow.PeekChar()) && ch != ch2)
                        lowSurrogateCharacter = TextWindow.NextChar();
                    break;
                }

                case 'z':
                    if (_options.SyntaxOptions.AcceptInvalidEscapes && !_options.SyntaxOptions.AcceptWhitespaceEscape)
                        goto default;

                    while (CharUtils.IsWhitespace(TextWindow.PeekChar())) TextWindow.AdvanceChar();

                    if (!_options.SyntaxOptions.AcceptWhitespaceEscape)
                    {
                        AddError(
                            start,
                            TextWindow.Position - start,
                            ErrorCode.ERR_WhitespaceEscapeNotSupportedInVersion);
                    }
                    ch = SlidingTextWindow.InvalidCharacter;
                    break;

                case '0':
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    ch = ParseDecimalInteger(start, ch);
                    break;

                case 'x':
                    if (_options.SyntaxOptions.AcceptInvalidEscapes
                        && !_options.SyntaxOptions.AcceptHexEscapesInStrings)
                    {
                        goto default;
                    }

                    ch = ParseHexadecimalEscapeInteger(start);

                    if (!_options.SyntaxOptions.AcceptHexEscapesInStrings)
                    {
                        AddError(
                            start,
                            TextWindow.Position - start,
                            ErrorCode.ERR_HexStringEscapesNotSupportedInVersion);
                    }
                    break;

                case 'u':
                    if (_options.SyntaxOptions.AcceptInvalidEscapes && !_options.SyntaxOptions.AcceptUnicodeEscape)
                        goto default;

                    ch = ParseUnicodeEscape(start, out lowSurrogateCharacter);

                    if (!_options.SyntaxOptions.AcceptUnicodeEscape)
                    {
                        AddError(
                            start,
                            TextWindow.Position - start,
                            ErrorCode.ERR_UnicodeEscapesNotSupportedLuaInVersion);
                    }
                    break;

                default:
                    if (!_options.SyntaxOptions.AcceptInvalidEscapes)
                    {
                        AddError(start, TextWindow.Position - start, ErrorCode.ERR_InvalidStringEscape);
                        ch = SlidingTextWindow.InvalidCharacter;
                    }
                    break;
            }
            return ch;

            // ReSharper disable once VariableHidesOuterVariable
            char ParseDecimalInteger(int start, char initial)
            {
                var readChars = 0;
                var num       = initial - '0';
                // ReSharper disable once VariableHidesOuterVariable
                char ch;
                while (readChars < 3 && CharUtils.IsDecimal(ch = TextWindow.PeekChar()))
                {
                    TextWindow.AdvanceChar();
                    num = (num * 10) + (ch - '0');
                    readChars++;
                }

                if (readChars >= 1 && num <= 255) return (char) num;

                AddError(start, TextWindow.Position - start, ErrorCode.ERR_InvalidStringEscape);
                return SlidingTextWindow.InvalidCharacter;
            }

            // ReSharper disable once VariableHidesOuterVariable LocalFunctionHidesMethod
            ulong ParseHexadecimalNumber(int start, int maxDigits, ErrorCode lessThanZeroErrorCode)
            {
                var readChars = 0;
                var num       = 0L;
                while (readChars < maxDigits)
                {
                    var peek = TextWindow.PeekChar();
                    if (CharUtils.IsDecimal(peek))
                    {
                        TextWindow.AdvanceChar();
                        num = (num << 4) | (uint) (peek - '0');
                    }
                    else if (CharUtils.IsHexadecimal(peek))
                    {
                        TextWindow.AdvanceChar();
                        num = (num << 4) | (uint) (10 + CharUtils.AsciiLowerCase(peek) - 'a');
                    }
                    else
                    {
                        break;
                    }
                    readChars++;
                }

                if (readChars >= 1) return (ulong) num;

                AddError(start, TextWindow.Position - start, lessThanZeroErrorCode);
                return SlidingTextWindow.InvalidCharacter;
            }

            // ReSharper disable once VariableHidesOuterVariable
            char ParseHexadecimalEscapeInteger(int start)
                => (char) ParseHexadecimalNumber(start, maxDigits: 2, ErrorCode.ERR_InvalidStringEscape);

            // ReSharper disable once VariableHidesOuterVariable
            char ParseUnicodeEscape(int start, out char lowSurrogate)
            {
                var missingOpeningBrace = TextWindow.PeekChar() is not '{';
                if (!missingOpeningBrace) TextWindow.AdvanceChar();

                var codepoint = ParseHexadecimalNumber(start, maxDigits: 16, ErrorCode.ERR_HexDigitExpected);

                var missingClosingBrace = TextWindow.PeekChar() is not '}';
                if (!missingClosingBrace) TextWindow.AdvanceChar();

                if (missingOpeningBrace)
                    AddError(start, TextWindow.Position - start, ErrorCode.ERR_UnicodeEscapeMissingOpenBrace);
                if (missingClosingBrace)
                    AddError(start, TextWindow.Position - start, ErrorCode.ERR_UnicodeEscapeMissingCloseBrace);
                if (codepoint > 0x10FFFF)
                {
                    AddError(start, TextWindow.Position - start, ErrorCode.ERR_EscapeTooLarge, "10FFFF");
                    codepoint = SlidingTextWindow.InvalidCharacter;
                }

                if (codepoint < 0x00010000)
                {
                    // Return the codepoint itself if it's in the BMP.
                    // NOTE: It *is* technically incorrect to consider a surrogate
                    // a Unicode codepoint but Lua accepts it so we do it as well.
                    lowSurrogate = SlidingTextWindow.InvalidCharacter;
                    return (char) codepoint;
                }

                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                LorettaDebug.Assert(codepoint is > 0x0000FFFF and <= 0x0010FFFF);
                lowSurrogate = (char) (((codepoint - 0x00010000) % 0x0400) + 0xDC00);
                return (char) (((codepoint - 0x00010000) / 0x0400) + 0xD800);
            }
        }

        /// <summary>
        ///     Non-copyable ref-struct so that this will only live on the stack for the lifetime of the lexer/parser
        ///     recursing to process interpolated strings.
        /// </summary>
        [NonCopyable]
        private ref struct InterpolatedStringScanner(Lexer lexer)
        {
            /// <summary>
            ///     Error encountered while scanning.  If we run into an error, then we'll attempt to stop parsing at the next
            ///     potential ending location to prevent compounding the issue.
            /// </summary>
            public SyntaxDiagnosticInfo? Error = null;

            private readonly bool IsAtEnd(bool allowNewline)
            {
                var ch = lexer.TextWindow.PeekChar();
                return (!allowNewline && CharUtils.IsNewLine(ch))
                       || (ch == SlidingTextWindow.InvalidCharacter && lexer.TextWindow.IsReallyAtEnd());
            }

            private void TrySetError(SyntaxDiagnosticInfo error)
                => // only need to record the first error we hit
                    Error ??= error;

            internal void ScanInterpolatedStringLiteralTop(
                out Range                    openQuoteRange,
                ArrayBuilder<Interpolation>? interpolations,
                out Range                    closeQuoteRange)
            {
                // Scan through the open-quote portion of this literal, determining important information the rest of
                // the scanning needs.
                var start     = lexer.TextWindow.Position;
                var succeeded = ScanOpenQuote();
                LorettaDebug.Assert(lexer.TextWindow.Position != start);

                openQuoteRange = start..lexer.TextWindow.Position;

                if (!succeeded)
                {
                    // Processing the start of this literal didn't give us enough information to proceed.  Stop now,
                    // terminating the string to the furthest point we reached.
                    closeQuoteRange = lexer.TextWindow.Position..lexer.TextWindow.Position;
                    return;
                }

                ScanInterpolatedStringLiteralContents(interpolations);
                ScanInterpolatedStringLiteralEnd(out closeQuoteRange);
            }

            /// <returns>
            ///     <see langword="true" /> if we successfully processed the open quote range and can proceed to the rest of the
            ///     literal. <see langword="false" /> if we were not successful and should stop processing.
            /// </returns>
            private bool ScanOpenQuote()
            {
                // Handles reading the start of the interpolated string literal (up to where the content begins)
                var window = lexer.TextWindow;
                var start  = window.Position;

                if (window.PeekChar() is '`')
                {
                    window.AdvanceChar(n: 1);
                    return true;
                }

                // We have no quotes at all.  We cannot continue on as we have no quotes, and thus can't even find
                // where the string starts or ends.
                TrySetError(
                    lexer.MakeError(
                        start,
                        window.Position - start,
                        ErrorCode.ERR_InterpolatedStringMustStartWithBacktickCharacter));
                return false;
            }

            private void ScanInterpolatedStringLiteralEnd(out Range closeQuoteRange)
            {
                // Handles reading the end of the interpolated string literal (after where the content ends)

                var closeQuotePosition = lexer.TextWindow.Position;

                if (lexer.TextWindow.PeekChar() != '`')
                {
                    // Didn't find a closing quote.  We hit the end of a line (in the normal case) or the end of the
                    // file in the normal/verbatim case.
                    LorettaDebug.Assert(IsAtEnd(allowNewline: false));

                    TrySetError(
                        lexer.MakeError(
                            IsAtEnd(allowNewline: true) ? lexer.TextWindow.Position - 1 : lexer.TextWindow.Position,
                            width: 1,
                            ErrorCode.ERR_UnfinishedString));
                }
                else
                {
                    // found the closing quote
                    lexer.TextWindow.AdvanceChar(); // `
                }

                // Note: this range may be empty.  For example, if we hit the end of a line for a single-line construct,
                // or we hit the end of a file for a multi-line construct.
                closeQuoteRange = closeQuotePosition..lexer.TextWindow.Position;
            }

            private void ScanInterpolatedStringLiteralContents(ArrayBuilder<Interpolation>? interpolations)
            {
                while (true)
                {
                    if (IsAtEnd(allowNewline: true))
                    {
                        // error: end of line/file before end of string pop out. Error will be reported in
                        // ScanInterpolatedStringLiteralEnd
                        return;
                    }

                    switch (lexer.TextWindow.PeekChar())
                    {
                        case '`': return; // End of string.

                        case '{':
                            HandleOpenBraceInContent(interpolations);
                            continue;

                        default:
                            // found some other character in the string portion.  Just consume it as content and continue.
                            lexer.TextWindow.AdvanceChar();
                            continue;
                    }
                }
            }

            private void HandleOpenBraceInContent(ArrayBuilder<Interpolation>? interpolations)
            {
                var openBracePosition = lexer.TextWindow.Position;
                lexer.TextWindow.AdvanceChar();
                if (lexer.TextWindow.PeekChar() == '{')
                {
                    lexer.TextWindow.AdvanceChar();
                    TrySetError(
                        lexer.MakeError(openBracePosition - 1, width: 2, ErrorCode.ERR_DoubleBraceInInterpolation));
                }
                else
                {
                    ScanInterpolatedStringLiteralHoleBalancedText(endingChar: '}');
                    int closeBracePosition = lexer.TextWindow.Position;

                    if (lexer.TextWindow.PeekChar() == '}')
                    {
                        lexer.TextWindow.AdvanceChar();
                    }
                    else
                    {
                        TrySetError(
                            lexer.MakeError(openBracePosition - 1, width: 2, ErrorCode.ERR_UnclosedExpressionHole));
                    }

                    interpolations?.Add(
                        new Interpolation(
                            new Range(openBracePosition, openBracePosition + 1),
                            new Range(closeBracePosition, lexer.TextWindow.Position)));
                }
            }

            /// <summary>Scan past the hole inside an interpolated string literal, leaving the current character on the '}' (if any)</summary>
            private void ScanInterpolatedStringLiteralHoleBalancedText(char endingChar)
            {
                while (true)
                {
                    var ch = lexer.TextWindow.PeekChar();

                    // Note: within a hole newlines are always allowed.  The restriction on if newlines are allowed or not
                    // is only within a text-portion of the interpolated string.
                    if (IsAtEnd(allowNewline: true))
                    {
                        // the caller will complain
                        return;
                    }

                    switch (ch)
                    {
                        case '`':
                        {
                            var discarded = default(TokenInfo);
                            lexer.ScanInterpolatedStringLiteral(ref discarded);
                            continue;
                        }

                        case '}':
                        case ')':
                        case ']':
                            if (ch == endingChar) return;

                            TrySetError(
                                lexer.MakeError(
                                    lexer.TextWindow.Position,
                                    width: 1,
                                    ErrorCode.ERR_SyntaxError,
                                    endingChar.ToString()));
                            goto default;

                        case '"':
                            if (RecoveringFromRunawayLexing())
                            {
                                // When recovering from mismatched delimiters, we consume the next
                                // quote character as the close quote for the interpolated string. In
                                // practice this gets us out of trouble in scenarios we've encountered.
                                // See, for example, https://github.com/dotnet/roslyn/issues/44789
                                return;
                            }

                            // handle string literal inside an expression hole.
                            ScanInterpolatedStringLiteralNestedString();
                            continue;

                        case '\'':
                            // handle character literal inside an expression hole.
                            ScanInterpolatedStringLiteralNestedString();
                            continue;

                        case '/':
                            if (!lexer._options.SyntaxOptions.AcceptCCommentSyntax || !lexer.TryScanCComment(out _))
                                lexer.TextWindow.AdvanceChar();

                            continue;

                        case '-':
                            if (!lexer.TryScanComment(out _)) lexer.TextWindow.AdvanceChar();
                            continue;

                        case '{':
                            ScanInterpolatedStringLiteralHoleBracketed(start: '{', end: '}');
                            continue;

                        case '(':
                            ScanInterpolatedStringLiteralHoleBracketed(start: '(', end: ')');
                            continue;

                        case '[':
                            if (lexer.TextWindow.PeekChar(delta: 1) is not ('=' or '[')
                                || !lexer.TryScanLongString(out _, out _, out _))
                            {
                                // Assume it's an array index
                                ScanInterpolatedStringLiteralHoleBracketed(start: '[', end: ']');
                            }
                            continue;

                        default:
                            // part of code in the expression hole
                            lexer.TextWindow.AdvanceChar();
                            continue;
                    }
                }
            }

            /// <summary>
            ///     The lexer can run away consuming the rest of the input when delimiters are mismatched. This is a test for when
            ///     we are attempting to recover from that situation.  Note that just running into new lines will not make us think
            ///     we're in runaway lexing.
            /// </summary>
            private bool RecoveringFromRunawayLexing() => Error != null;

            private readonly void ScanInterpolatedStringLiteralNestedString()
            {
                var info = default(TokenInfo);
                lexer.ScanStringLiteral(ref info);
            }

            private void ScanInterpolatedStringLiteralHoleBracketed(char start, char end)
            {
                LorettaDebug.Assert(start == lexer.TextWindow.PeekChar());
                lexer.TextWindow.AdvanceChar();
                ScanInterpolatedStringLiteralHoleBalancedText(end);
                if (lexer.TextWindow.PeekChar() == end) lexer.TextWindow.AdvanceChar();
                // an error was given by the caller
            }
        }
        
        internal readonly struct Interpolation(Range openBraceRange, Range closeBraceRange)
        {
            public readonly Range OpenBraceRange = openBraceRange;

            /// <summary>
            /// Range of the close brace.  Empty if there was no close brace (an error condition).
            /// </summary>
            public readonly Range CloseBraceRange = closeBraceRange;
        }
    }
}
