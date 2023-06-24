using Loretta.CodeAnalysis.Lua.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal sealed partial class Lexer
    {
        private string ParseShortString()
        {
            _builder.Clear();
            var delim = TextWindow.NextChar();
            LorettaDebug.Assert(delim is '"' or '\'' or '`');

            char ch;
            while (!IsAtEnd(ch = TextWindow.PeekChar()) && ch != delim)
            {
                var charStart = TextWindow.Position;
                switch (ch)
                {
                    #region Escapes

                    case '\\':
                    {
                        var escapeStart = TextWindow.Position;
                        TextWindow.AdvanceChar();

                        switch (ch = TextWindow.PeekChar())
                        {
                            case '\n':
                            case '\r':
                            {
                                _builder.Append(TextWindow.NextChar());
                                char ch2;
                                if (CharUtils.IsNewLine(ch2 = TextWindow.PeekChar())
                                    && ch != ch2)
                                {
                                    _builder.Append(TextWindow.NextChar());
                                }
                                break;
                            }

                            case 'a':
                                TextWindow.AdvanceChar();
                                _builder.Append('\a');
                                break;

                            case 'b':
                                TextWindow.AdvanceChar();
                                _builder.Append('\b');
                                break;

                            case 'f':
                                TextWindow.AdvanceChar();
                                _builder.Append('\f');
                                break;

                            case 'n':
                                TextWindow.AdvanceChar();
                                _builder.Append('\n');
                                break;

                            case 'r':
                                TextWindow.AdvanceChar();
                                _builder.Append('\r');
                                break;

                            case 't':
                                TextWindow.AdvanceChar();
                                _builder.Append('\t');
                                break;

                            case 'v':
                                TextWindow.AdvanceChar();
                                _builder.Append('\v');
                                break;

                            case '\\':
                                TextWindow.AdvanceChar();
                                _builder.Append('\\');
                                break;

                            case '\'':
                                TextWindow.AdvanceChar();
                                _builder.Append('\'');
                                break;

                            case '"':
                                TextWindow.AdvanceChar();
                                _builder.Append('"');
                                break;

                            case 'z':
                                if (_options.SyntaxOptions.AcceptInvalidEscapes && !_options.SyntaxOptions.AcceptWhitespaceEscape)
                                    goto default;

                                TextWindow.AdvanceChar();

                                while (CharUtils.IsWhitespace(TextWindow.PeekChar()))
                                    TextWindow.AdvanceChar();

                                if (!_options.SyntaxOptions.AcceptWhitespaceEscape)
                                    AddError(escapeStart, TextWindow.Position - escapeStart, ErrorCode.ERR_WhitespaceEscapeNotSupportedInVersion);
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
                            {
                                var parsedCharInteger = parseDecimalInteger(escapeStart);
                                if (parsedCharInteger != char.MaxValue)
                                    _builder.Append(parsedCharInteger);
                                break;
                            }

                            case 'x':
                            {
                                if (_options.SyntaxOptions.AcceptInvalidEscapes && !_options.SyntaxOptions.AcceptHexEscapesInStrings)
                                    goto default;

                                TextWindow.AdvanceChar();
                                var parsedCharInteger = parseHexadecimalEscapeInteger(escapeStart);
                                if (parsedCharInteger != char.MaxValue)
                                    _builder.Append(parsedCharInteger);

                                if (!_options.SyntaxOptions.AcceptHexEscapesInStrings)
                                    AddError(escapeStart, TextWindow.Position - escapeStart, ErrorCode.ERR_HexStringEscapesNotSupportedInVersion);
                            }
                            break;

                            case 'u':
                            {
                                if (_options.SyntaxOptions.AcceptInvalidEscapes && !_options.SyntaxOptions.AcceptUnicodeEscape)
                                    goto default;

                                TextWindow.AdvanceChar();
                                var parsed = parseUnicodeEscape(escapeStart);
                                _builder.Append(parsed);

                                if (!_options.SyntaxOptions.AcceptUnicodeEscape)
                                    AddError(escapeStart, TextWindow.Position - escapeStart, ErrorCode.ERR_UnicodeEscapesNotSupportedLuaInVersion);
                            }
                            break;

                            default:
                                if (!_options.SyntaxOptions.AcceptInvalidEscapes)
                                {
                                    // Skip the character after the escape.
                                    TextWindow.AdvanceChar();
                                    AddError(escapeStart, TextWindow.Position - escapeStart, ErrorCode.ERR_InvalidStringEscape);
                                }
                                break;
                        }
                    }
                    break;

                    #endregion Escapes

                    case '\n':
                    case '\r':
                    {
                        _builder.Append(TextWindow.NextChar());
                        char ch2;
                        if (CharUtils.IsNewLine(ch2 = TextWindow.PeekChar())
                            && ch != ch2)
                        {
                            _builder.Append(TextWindow.NextChar());
                        }

                        AddError(charStart, TextWindow.Position - charStart, ErrorCode.ERR_UnescapedLineBreakInString);
                    }
                    break;

                    default:
                        _builder.Append(TextWindow.NextChar());
                        break;
                }
            }

            if (TextWindow.PeekChar() == delim)
            {
                TextWindow.AdvanceChar();
            }
            else
            {
                AddError(ErrorCode.ERR_UnfinishedString);
            }

            return TextWindow.Intern(_builder);

            char parseDecimalInteger(int start)
            {
                var readChars = 0;
                var num = 0;
                char ch;
                while (readChars < 3 && CharUtils.IsDecimal(ch = TextWindow.PeekChar()))
                {
                    TextWindow.AdvanceChar();
                    num = (num * 10) + (ch - '0');
                    readChars++;
                }

                if (readChars < 1 || num > 255)
                {
                    AddError(start, TextWindow.Position - start, ErrorCode.ERR_InvalidStringEscape);
                    return char.MaxValue;
                }

                return (char) num;
            }

            ulong parseHexadecimalNumber(int start, int maxDigits, ErrorCode lessThanZeroErrorCode)
            {
                var readChars = 0;
                var num = 0L;
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

                if (readChars < 1)
                {
                    AddError(start, TextWindow.Position - start, lessThanZeroErrorCode);
                    return 0UL;
                }

                return (ulong) num;
            }

            char parseHexadecimalEscapeInteger(int start) =>
                (char) parseHexadecimalNumber(start, 2, ErrorCode.ERR_InvalidStringEscape);

            string parseUnicodeEscape(int start)
            {
                var missingOpeningBrace = TextWindow.PeekChar() is not '{';
                if (!missingOpeningBrace)
                    TextWindow.AdvanceChar();

                var codepoint = parseHexadecimalNumber(start, 16, ErrorCode.ERR_HexDigitExpected);

                var missingClosingBrace = TextWindow.PeekChar() is not '}';
                if (!missingClosingBrace)
                    TextWindow.AdvanceChar();

                if (missingOpeningBrace)
                    AddError(start, TextWindow.Position - start, ErrorCode.ERR_UnicodeEscapeMissingOpenBrace);
                if (missingClosingBrace)
                    AddError(start, TextWindow.Position - start, ErrorCode.ERR_UnicodeEscapeMissingCloseBrace);
                if (codepoint > 0x10FFFF)
                {
                    AddError(start, TextWindow.Position - start, ErrorCode.ERR_EscapeTooLarge, "10FFFF");
                    codepoint = 0x10FFFF;
                }

                // Return the codepoint itself if it's in the BMP.
                // NOTE: It *is* technically incorrect to consider a surrogate
                // an Unicode codepoint but Lua accepts it so we do it as well.
                if (codepoint <= 0xFFFF)
                    return char.ToString((char) codepoint);

                return char.ConvertFromUtf32((int) codepoint);
            }
        }
    }
}
