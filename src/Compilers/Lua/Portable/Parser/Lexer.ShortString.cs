using Loretta.CodeAnalysis.Lua.Utilities;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal sealed partial class Lexer
    {
        private string ParseShortString()
        {
            _builder.Clear();
            var delim = _reader.Read()!.Value;
            RoslynDebug.Assert(delim is '"' or '\'');

            while (_reader.Peek() is char peek && peek != delim)
            {
                var charStart = _reader.Position;
                switch (peek)
                {
                    #region Escapes

                    case '\\':
                    {
                        var escapeStart = _reader.Position;
                        _reader.Position += 1;

                        switch (_reader.Peek())
                        {
                            case '\r':
                                if (_reader.IsAt(1, '\n'))
                                {
                                    _reader.Position += 2;
                                    _builder.Append("\r\n");
                                }
                                else
                                {
                                    _reader.Position += 1;
                                    _builder.Append('\r');
                                }
                                break;

                            case 'a':
                                _reader.Position += 1;
                                _builder.Append('\a');
                                break;

                            case 'b':
                                _reader.Position += 1;
                                _builder.Append('\b');
                                break;

                            case 'f':
                                _reader.Position += 1;
                                _builder.Append('\f');
                                break;

                            case 'n':
                                _reader.Position += 1;
                                _builder.Append('\n');
                                break;

                            case 'r':
                                _reader.Position += 1;
                                _builder.Append('\r');
                                break;

                            case 't':
                                _reader.Position += 1;
                                _builder.Append('\t');
                                break;

                            case 'v':
                                _reader.Position += 1;
                                _builder.Append('\v');
                                break;

                            case '\\':
                                _reader.Position += 1;
                                _builder.Append('\\');
                                break;

                            case '\n':
                                _reader.Position += 1;
                                _builder.Append('\n');
                                break;

                            case '\'':
                                _reader.Position += 1;
                                _builder.Append('\'');
                                break;

                            case '"':
                                _reader.Position += 1;
                                _builder.Append('"');
                                break;

                            case 'z':
                                _reader.Position += 1;
                                _reader.SkipWhile(static c => CharUtils.IsWhitespace(c));

                                if (!Options.SyntaxOptions.AcceptWhitespaceEscape)
                                    AddError(escapeStart, _reader.Position - escapeStart, ErrorCode.ERR_WhitespaceEscapeNotSupportedInVersion);
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
                                _reader.Position += 1;
                                var parsedCharInteger = parseHexadecimalEscapeInteger(escapeStart);
                                if (parsedCharInteger != char.MaxValue)
                                    _builder.Append(parsedCharInteger);

                                if (!Options.SyntaxOptions.AcceptHexEscapesInStrings)
                                    AddError(escapeStart, _reader.Position - escapeStart, ErrorCode.ERR_HexStringEscapesNotSupportedInVersion);
                            }
                            break;

                            case 'u':
                            {
                                _reader.Position += 1;
                                var parsed = parseUnicodeEscape(escapeStart);
                                _builder.Append(parsed);

                                if (!Options.SyntaxOptions.AcceptUnicodeEscape)
                                    AddError(escapeStart, _reader.Position - escapeStart, ErrorCode.ERR_UnicodeEscapesNotSupportedLuaInVersion);
                            }
                            break;

                            default:
                                // Skip the character after the escape.
                                _reader.Position += 1;
                                AddError(escapeStart, _reader.Position - escapeStart, ErrorCode.ERR_InvalidStringEscape);
                                break;
                        }
                    }
                    break;

                    #endregion Escapes

                    case '\r':
                    {
                        if (_reader.IsAt(1, '\n'))
                        {
                            _reader.Position += 2;
                            _builder.Append("\r\n");
                        }
                        else
                        {
                            _reader.Position += 1;
                            _builder.Append('\r');
                        }

                        AddError(charStart, _reader.Position - charStart, ErrorCode.ERR_UnescapedLineBreakInString);
                    }
                    break;

                    case '\n':
                    {
                        _reader.Position += 1;
                        _builder.Append('\n');

                        AddError(charStart, _reader.Position - charStart, ErrorCode.ERR_UnescapedLineBreakInString);
                    }
                    break;

                    default:
                        _reader.Position += 1;
                        _builder.Append(peek);
                        break;
                }
            }

            if (_reader.IsNext(delim))
            {
                _reader.Position += 1;
            }
            else
            {
                AddError(ErrorCode.ERR_UnfinishedString);
            }

            return _builder.ToString();

            char parseDecimalInteger(int start)
            {
                var readChars = 0;
                var num = 0;
                char ch;
                while (readChars < 3 && CharUtils.IsDecimal(ch = _reader.Peek().GetValueOrDefault()))
                {
                    _reader.Position += 1;
                    num = (num * 10) + (ch - '0');
                    readChars++;
                }

                if (readChars < 1 || num > 255)
                {
                    AddError(start, _reader.Position - start, ErrorCode.ERR_InvalidStringEscape);
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
                    var peek = _reader.Peek().GetValueOrDefault();
                    if (CharUtils.IsDecimal(peek))
                    {
                        _reader.Position += 1;
                        num = (byte) ((num << 4) | (uint) (peek - '0'));
                    }
                    else if (CharUtils.IsHexadecimal(peek))
                    {
                        _reader.Position += 1;
                        num = (byte) ((num << 4) | (uint) (10 + CharUtils.AsciiLowerCase(peek) - 'a'));
                    }
                    else
                    {
                        break;
                    }
                    readChars++;
                }

                if (readChars < 1)
                {
                    AddError(start, _reader.Position - start, lessThanZeroErrorCode);
                    return 0UL;
                }

                return (ulong) num;
            }

            char parseHexadecimalEscapeInteger(int start) =>
                (char) parseHexadecimalNumber(start, 2, ErrorCode.ERR_InvalidStringEscape);

            string parseUnicodeEscape(int start)
            {
                if (_reader.Peek() is not '{')
                    AddError(start, _reader.Position - start, ErrorCode.ERR_UnicodeEscapeMissingOpenBrace);
                else
                    _reader.Position += 1;

                var codepoint = parseHexadecimalNumber(start, 16, ErrorCode.ERR_HexDigitExpected);

                if (_reader.Peek() is not '}')
                    AddError(start, _reader.Position - start, ErrorCode.ERR_UnicodeEscapeMissingCloseBrace);
                else
                    _reader.Position += 1;

                if (codepoint > 0x10FFFF)
                {
                    AddError(start, _reader.Position - start, ErrorCode.ERR_EscapeTooLarge, "10FFFF");
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
