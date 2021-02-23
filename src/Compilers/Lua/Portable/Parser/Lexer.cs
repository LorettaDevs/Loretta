using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GParse.IO;
using Loretta.CodeAnalysis.Lua.Utilities;
using Loretta.CodeAnalysis.Text;
using Tsu;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal sealed partial class Lexer
    {
        // Maximum size of tokens/trivia that we cache and use in quick scanner.
        // From what I see in our own codebase, tokens longer then 40-50 chars are 
        // not very common. 
        // So it seems reasonable to limit the sizes to some round number like 42.
        internal const int MaxCachedTokenSize = 42;

        private readonly LuaOptions _luaOptions;
        private readonly SourceText _text;
        private readonly ICodeReader _reader;

        private int _start;
        private readonly ImmutableArray<SyntaxTrivia>.Builder _triviaBuilder = ImmutableArray.CreateBuilder<SyntaxTrivia>();

        public DiagnosticBag Diagnostics { get; }

        public int Length => _reader.Length;

        public int Position => _reader.Position;

        public Lexer(LuaOptions luaOptions, SourceText text)
        {
            _luaOptions = luaOptions;
            _text = text;
            // TODO: Either make an SourceTextCodeReader or reimplement the
            // Lexer without the ICodeReader.
            _reader = new StringCodeReader(text.ToString());
            Diagnostics = new DiagnosticBag();
        }

        public SyntaxToken Lex()
        {
            var leadingTrivia = ReadTrivia(leading: true);
            var tokenStart = Position;

            (var tokenKind, var tokenValue) = ReadToken();

            var tokenLength = Position - tokenStart;

            var trailingTrivia = ReadTrivia(leading: false);

            ReadOnlyMemory<char> tokenText = SyntaxFacts.GetText(tokenKind) is string txt
                                             ? txt.AsMemory()
                                             : _text.AsMemory(tokenStart, tokenLength);

            return new SyntaxToken(
                tokenKind,
                tokenStart,
                tokenText,
                tokenValue,
                leadingTrivia,
                trailingTrivia,
                false);
        }

        private ImmutableArray<SyntaxTrivia> ReadTrivia(bool leading)
        {
            _triviaBuilder.Clear();

            while (_reader.Peek() is char peek)
            {
                _start = _reader.Position;

                switch (peek)
                {
                    case '-':
                        if (_reader.IsAt('-', 1))
                        {
                            _reader.Advance(2);
                            if (TryReadLongString(out _, out var closingNotFound))
                            {
                                if (closingNotFound)
                                {
                                    var span = TextSpan.FromBounds(_start, _reader.Position);
                                    var location = new TextLocation(_text, span);
                                    Diagnostics.ReportUnfinishedLongComment(location);
                                }

                                submitTrivia(SyntaxKind.MultiLineCommentTrivia);
                            }
                            else
                            {
                                while (_reader.Peek() is not (null or '\n' or '\r'))
                                    _reader.Advance(1);
                                submitTrivia(SyntaxKind.SingleLineCommentTrivia);
                            }
                        }
                        else
                        {
                            goto end;
                        }
                        break;

                    case '/':
                        if (_reader.IsAt('/', 1))
                        {
                            _reader.Advance(2);
                            while (_reader.Peek() is not (null or '\n' or '\r'))
                                _reader.Advance(1);

                            if (!_luaOptions.AcceptCCommentSyntax)
                            {
                                var span = TextSpan.FromBounds(_start, _reader.Position);
                                var location = new TextLocation(_text, span);
                                Diagnostics.ReportCCommentsNotSupportedInVersion(location);
                            }

                            submitTrivia(SyntaxKind.SingleLineCommentTrivia);
                        }
                        else if (_reader.IsAt('*', 1))
                        {
                            _reader.Advance(2);
                            _ = _reader.ReadSpanUntil("*/");

                            if (!_reader.IsNext("*/"))
                            {
                                var span = TextSpan.FromBounds(_start, _reader.Position);
                                var location = new TextLocation(_text, span);
                                Diagnostics.ReportUnfinishedLongComment(location);
                            }
                            else
                            {
                                _reader.Advance(2);
                            }

                            if (!_luaOptions.AcceptCCommentSyntax)
                            {
                                var span = TextSpan.FromBounds(_start, _reader.Position);
                                var location = new TextLocation(_text, span);
                                Diagnostics.ReportCCommentsNotSupportedInVersion(location);
                            }

                            submitTrivia(SyntaxKind.MultiLineCommentTrivia);
                        }
                        else
                        {
                            goto end;
                        }
                        break;

                    case '\r':
                        _reader.Advance(1);
                        if (_reader.Peek() == '\n')
                            _reader.Advance(1);
                        submitTrivia(SyntaxKind.EndOfLineTrivia);
                        if (!leading)
                            goto end;
                        break;

                    case '\n':
                        _reader.Advance(1);
                        submitTrivia(SyntaxKind.EndOfLineTrivia);
                        if (!leading)
                            goto end;
                        break;

                    case ' ':
                    case '\t':
                    {
                        // Skip first char as we know it's whitespace.
                        _reader.Advance(1);

                        char ch;
                        while ((ch = _reader.Peek().GetValueOrDefault()) != '\r'
                                && ch != '\n'
                                && char.IsWhiteSpace(ch))
                        {
                            _reader.Advance(1);
                        }
                        submitTrivia(SyntaxKind.WhitespaceTrivia);
                        break;
                    }

                    case '#':
                        if (_reader.Position == 0 && _reader.IsAt('!', 1))
                        {
                            _reader.Advance(2);
                            _ = _reader.ReadSpanLine();

                            if (!_luaOptions.AcceptShebang)
                            {
                                var span = TextSpan.FromBounds(_start, _reader.Position);
                                var location = new TextLocation(_text, span);
                                Diagnostics.ReportShebangNotSupportedInVersion(location);
                            }

                            submitTrivia(SyntaxKind.ShebangTrivia);
                            break;
                        }
                        else
                        {
                            goto end;
                        }

                    default:
                        if (char.IsWhiteSpace(peek))
                            goto case ' ';
                        else
                            goto end;
                }
            }

        end:
            return _triviaBuilder.ToImmutable();

            void submitTrivia(SyntaxKind kind)
            {
                var length = _reader.Position - _start;
                ReadOnlyMemory<char> text = _text.AsMemory(_start, length);
                _triviaBuilder.Add(new SyntaxTrivia(kind, _start, text));
            }
        }

        public (SyntaxKind kind, Option<object?> tokenValue) ReadToken()
        {
            _start = _reader.Position;

            if (_reader.Peek() is not char peek0)
                return (SyntaxKind.EndOfFileToken, null);

            switch (peek0)
            {
                #region Punctuation

                case '.':
                {
                    var peek1 = _reader.Peek(1).GetValueOrDefault();
                    if (peek1 == '.')
                    {
                        // \.\.\.
                        if (_reader.IsAt('.', 2))
                        {
                            _reader.Advance(3);
                            return (SyntaxKind.DotDotDotToken, Option.None<object?>());
                        }
                        // \.\.=
                        else if (_reader.IsAt('=', 2))
                        {
                            _reader.Advance(3);
                            return (SyntaxKind.DotDotEqualsToken, Option.None<object?>());
                        }
                        // \.\.
                        else
                        {
                            _reader.Advance(2);
                            return (SyntaxKind.DotDotToken, Option.None<object?>());
                        }
                    }
                    // \.[0-9]
                    else if (CharUtils.IsDecimal(peek1))
                    {
                        goto case '1';
                    }
                    // \.
                    else
                    {
                        _reader.Advance(1);
                        return (SyntaxKind.DotToken, Option.None<object?>());
                    }
                }

                case ';':
                    _reader.Advance(1);
                    return (SyntaxKind.SemicolonToken, Option.None<object?>());

                case ',':
                    _reader.Advance(1);
                    return (SyntaxKind.CommaToken, Option.None<object?>());

                case ':':
                    if (_reader.IsAt(':', 1))
                    {
                        _reader.Advance(2);
                        return (SyntaxKind.ColonColonToken, Option.None<object?>());
                    }
                    else
                    {
                        _reader.Advance(1);
                        return (SyntaxKind.ColonToken, Option.None<object?>());
                    }

                #endregion Punctuation

                case '(':
                    _reader.Advance(1);
                    return (SyntaxKind.OpenParenthesisToken, Option.None<object?>());

                case ')':
                    _reader.Advance(1);
                    return (SyntaxKind.CloseParenthesisToken, Option.None<object?>());

                case '[':
                {
                    if (TryReadLongString(out var contents, out var closingNotFound))
                    {
                        if (closingNotFound)
                        {
                            var span = TextSpan.FromBounds(_start, _reader.Position);
                            var location = new TextLocation(_text, span);
                            Diagnostics.ReportUnfinishedString(location);
                        }

                        return (SyntaxKind.StringLiteralToken, contents);
                    }
                    else
                    {
                        _reader.Advance(1);
                        return (SyntaxKind.OpenBracketToken, Option.None<object?>());
                    }
                }


                case ']':
                    _reader.Advance(1);
                    return (SyntaxKind.CloseBracketToken, Option.None<object?>());

                case '{':
                    _reader.Advance(1);
                    return (SyntaxKind.OpenBraceToken, Option.None<object?>());

                case '}':
                    _reader.Advance(1);
                    return (SyntaxKind.CloseBraceToken, Option.None<object?>());

                #region Operators

                case '+':
                    if (_reader.IsAt('=', 1))
                    {
                        _reader.Advance(2);
                        return (SyntaxKind.PlusEqualsToken, Option.None<object?>());
                    }
                    else
                    {
                        _reader.Advance(1);
                        return (SyntaxKind.PlusToken, Option.None<object?>());
                    }

                case '-':
                    if (_reader.IsAt('=', 1))
                    {
                        _reader.Advance(2);
                        return (SyntaxKind.MinusEqualsToken, Option.None<object?>());
                    }
                    else
                    {
                        _reader.Advance(1);
                        return (SyntaxKind.MinusToken, Option.None<object?>());
                    }

                case '*':
                    if (_reader.IsAt('=', 1))
                    {
                        _reader.Advance(2);
                        return (SyntaxKind.StartEqualsToken, Option.None<object?>());
                    }
                    else
                    {
                        _reader.Advance(1);
                        return (SyntaxKind.StarToken, Option.None<object?>());
                    }

                case '/':
                    if (_reader.IsAt('=', 1))
                    {
                        _reader.Advance(2);
                        return (SyntaxKind.SlashEqualsToken, Option.None<object?>());
                    }
                    else
                    {
                        _reader.Advance(1);
                        return (SyntaxKind.SlashToken, Option.None<object?>());
                    }

                case '^':
                    if (_reader.IsAt('=', 1))
                    {
                        _reader.Advance(2);
                        return (SyntaxKind.HatEqualsToken, Option.None<object?>());
                    }
                    else
                    {
                        _reader.Advance(1);
                        return (SyntaxKind.HatToken, Option.None<object?>());
                    }

                case '%':
                    if (_reader.IsAt('=', 1))
                    {
                        _reader.Advance(2);
                        return (SyntaxKind.PercentEqualsToken, Option.None<object?>());
                    }
                    else
                    {
                        _reader.Advance(1);
                        return (SyntaxKind.PercentToken, Option.None<object?>());
                    }

                case '=':
                    if (_reader.IsAt('=', 1))
                    {
                        _reader.Advance(2);
                        return (SyntaxKind.EqualsEqualsToken, Option.None<object?>());
                    }
                    else
                    {
                        _reader.Advance(1);
                        return (SyntaxKind.EqualsToken, Option.None<object?>());
                    }

                case '#':
                    _reader.Advance(1);
                    return (SyntaxKind.HashToken, Option.None<object?>());

                case '~':
                    if (_reader.IsAt('=', 1))
                    {
                        _reader.Advance(2);
                        return (SyntaxKind.TildeEqualsToken, Option.None<object?>());
                    }
                    break;

                case '>':
                    switch (_reader.Peek(1))
                    {
                        case '=':
                            _reader.Advance(2);
                            return (SyntaxKind.GreaterThanEqualsToken, Option.None<object?>());

                        case '>':
                            _reader.Advance(2);
                            return (SyntaxKind.GreaterThanGreaterThanToken, Option.None<object?>());

                        default:
                            _reader.Advance(1);
                            return (SyntaxKind.GreaterThanToken, Option.None<object?>());
                    }

                case '<':
                    switch (_reader.Peek(1))
                    {
                        case '=':
                            _reader.Advance(2);
                            return (SyntaxKind.LessThanEqualsToken, Option.None<object?>());

                        case '<':
                            _reader.Advance(2);
                            return (SyntaxKind.LessThanLessThanToken, Option.None<object?>());

                        default:
                            _reader.Advance(1);
                            return (SyntaxKind.LessThanToken, Option.None<object?>());
                    }

                case '&':
                    if (_reader.IsAt('&', 1))
                    {
                        _reader.Advance(2);
                        return (SyntaxKind.AmpersandAmpersandToken, Option.None<object?>());
                    }
                    else
                    {
                        _reader.Advance(1);
                        return (SyntaxKind.AmpersandToken, Option.None<object?>());
                    }

                case '|':
                    if (_reader.IsAt('|', 1))
                    {
                        _reader.Advance(2);
                        return (SyntaxKind.PipePipeToken, Option.None<object?>());
                    }
                    else
                    {
                        _reader.Advance(1);
                        return (SyntaxKind.PipeToken, Option.None<object?>());
                    }

                case '!':
                    if (_reader.IsAt('=', 1))
                    {
                        _reader.Advance(2);
                        return (SyntaxKind.BangEqualsToken, Option.None<object?>());
                    }
                    else
                    {
                        _reader.Advance(1);
                        return (SyntaxKind.BangToken, Option.None<object?>());
                    }

                #endregion Operators

                #region Literals

                #region Numbers

                case '0':
                {
                    switch (_reader.Peek(1))
                    {
                        // 0b[01_]+
                        case 'b':
                        {
                            // Skip the prefix
                            _reader.Advance(2);
                            var val = this.ParseBinaryNumber();
                            return (SyntaxKind.NumericLiteralToken, val);
                        }

                        // 0o[0-7_]+
                        case 'o':
                        {
                            // Skip the prefix
                            _reader.Advance(2);
                            var val = this.ParseOctalNumber();
                            return (SyntaxKind.NumericLiteralToken, val);
                        }

                        // 0x(?:[A-Fa-f0-9]?[A-Fa-f0-9_]*)?\.(?:[A-Fa-f0-9][A-Fa-f0-9_]*)?(?:[pP][0-9]+)?
                        case 'x':
                        {
                            // Skip the prefix
                            _reader.Advance(2);
                            var val = this.ParseHexadecimalNumber();
                            return (SyntaxKind.NumericLiteralToken, val);
                        }
                    }

                    goto case '1';
                }

                // [0-9]*(?:\.[0-9]*)?(?:[eE][0-9]+)?
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
                    var val = this.ParseDecimalNumber();
                    return (SyntaxKind.NumericLiteralToken, val);
                }

                #endregion Numbers

                case '"':
                case '\'':
                {
                    var val = this.ParseShortString();
                    return (SyntaxKind.StringLiteralToken, val);
                }

                #endregion Literals

                #region Identifiers

                case 'a':
                case 'b':
                case 'c':
                case 'd':
                case 'e':
                case 'f':
                case 'g':
                case 'h':
                case 'i':
                case 'j':
                case 'k':
                case 'l':
                case 'm':
                case 'n':
                case 'o':
                case 'p':
                case 'q':
                case 'r':
                case 's':
                case 't':
                case 'u':
                case 'v':
                case 'w':
                case 'x':
                case 'y':
                case 'z':
                case 'A':
                case 'B':
                case 'C':
                case 'D':
                case 'E':
                case 'F':
                case 'G':
                case 'H':
                case 'I':
                case 'J':
                case 'K':
                case 'L':
                case 'M':
                case 'N':
                case 'O':
                case 'P':
                case 'Q':
                case 'R':
                case 'S':
                case 'T':
                case 'U':
                case 'V':
                case 'W':
                case 'X':
                case 'Y':
                case 'Z':
                case '_':
                {
                    // We've already checked that the first letter is a valid identifier char. No need to re-check.
                    _reader.Advance(1);
                    while (CharUtils.IsValidTrailingIdentifierChar(_reader.Peek().GetValueOrDefault()))
                        _reader.Advance(1);

                    var text = _text.ToString(_start, _reader.Position - _start);
                    if (!_luaOptions.UseLuaJitIdentifierRules && text.Any(ch => ch >= 0x7F))
                    {
                        var span = new TextSpan(_start, text.Length);
                        var location = new TextLocation(_text, span);
                        Diagnostics.ReportLuajitIdentifierRulesNotSupportedInVersion(location);
                    }
                    SyntaxKind kind = SyntaxFacts.GetKeywordKind(text);
                    return (kind, default);
                }

                #endregion Identifiers

                default:
                {
                    if (CharUtils.IsValidFirstIdentifierChar(peek0))
                    {
                        goto case 'a';
                    }
                    else
                    {
                        _reader.Advance(1);
                        var span = new TextSpan(_start, 1);
                        var location = new TextLocation(_text, span);
                        Diagnostics.ReportBadCharacter(location, peek0);
                        return (SyntaxKind.BadToken, Option.None<object?>());
                    }
                }
            } // end switch

            throw new Exception("Unreacheable.");
        }

        private bool TryReadLongString([NotNullWhen(true)] out string? contents, out bool closingNotFound)
        {
            closingNotFound = true;
            var start = _reader.Position;
            if (_reader.IsNext('[')
                 && (_reader.IsAt('=', 1)
                      || _reader.IsAt('[', 1)))
            {
                _reader.Advance(1);
                var equalSigns = _reader.ReadStringWhile(ch => ch == '=');
                if (_reader.IsNext('['))
                {
                    _reader.Advance(1);

                    var closing = $"]{equalSigns}]";
                    contents = _reader.ReadStringUntil(closing);

                    if (_reader.IsNext(closing))
                    {
                        closingNotFound = false;
                        _reader.Advance(closing.Length);
                    }

                    return true;
                }

                _reader.Restore(start);
            }

            contents = null;
            return false;
        }
    }
}
