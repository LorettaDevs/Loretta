using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Loretta.CodeAnalysis.Lua.Utilities;
using Loretta.CodeAnalysis.Syntax.InternalSyntax;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{

    internal sealed partial class Lexer : AbstractLexer, IDisposable
    {
        // Maximum size of tokens/trivia that we cache and use in quick scanner.
        // From what I see in our own codebase, tokens longer then 40-50 chars are 
        // not very common. 
        // So it seems reasonable to limit the sizes to some round number like 42.
        internal const int MaxCachedTokenSize = 42;

        private struct TokenInfo
        {
            // scanned values
            internal SyntaxKind Kind;
            internal SyntaxKind ContextualKind;
            internal string? Text;
            internal string? StringValue;
            internal double DoubleValue;
        }

        private readonly LexerCache _cache = new LexerCache();
        private readonly SyntaxListBuilder _leadingTriviaCache = new(10);
        private readonly SyntaxListBuilder _trailingTriviaCache = new(10);
        private readonly StringBuilder _builder = new StringBuilder();
        private int _badTokenCount; // cumulative count of bad tokens produced

        private static int GetFullWidth(SyntaxListBuilder builder)
        {
            var width = 0;
            for (var idx = 0; idx < builder.Count; idx++)
                width += builder[idx]!.FullWidth;
            return width;
        }

        public Lexer(SourceText text, LuaParseOptions options)
            : base(text)
        {
            RoslynDebug.Assert(options is not null);
            Options = options;
            _createWhitespaceTriviaFunction = CreateWhitespaceTrivia;
        }

        public LuaParseOptions Options { get; }

        public SyntaxToken Lex()
        {
            LexSyntaxTrivia(isTrailing: false, _leadingTriviaCache);

            var info = default(TokenInfo);
            Start();
            LexSyntaxToken(ref info);
            var errors = GetErrors(GetFullWidth(_leadingTriviaCache));

            LexSyntaxTrivia(isTrailing: true, _trailingTriviaCache);

            return Create(ref info, _leadingTriviaCache, _trailingTriviaCache, errors);
        }

        internal SyntaxTriviaList LexSyntaxLeadingTrivia()
        {
            _leadingTriviaCache.Clear();
            LexSyntaxTrivia(isTrailing: false, _leadingTriviaCache);
            return new SyntaxTriviaList(
                default,
                _leadingTriviaCache.ToListNode(),
                position: 0,
                index: 0);
        }

        internal SyntaxTriviaList LexSyntaxTrailingTrivia()
        {
            _trailingTriviaCache.Clear();
            LexSyntaxTrivia(isTrailing: true, _trailingTriviaCache);
            return new SyntaxTriviaList(
                default,
                _trailingTriviaCache.ToListNode(),
                position: 0,
                index: 0);
        }

        private static SyntaxToken Create(
            ref TokenInfo info,
            SyntaxListBuilder leading,
            SyntaxListBuilder trailing,
            SyntaxDiagnosticInfo[]? errors)
        {
            RoslynDebug.Assert(info.Kind != SyntaxKind.IdentifierToken || info.StringValue != null);

            var leadingNode = leading.ToListNode();
            var trailingNode = trailing.ToListNode();

            SyntaxToken token;
            switch (info.Kind)
            {
                case SyntaxKind.IdentifierToken:
                    token = SyntaxFactory.Identifier(info.ContextualKind, leadingNode, info.Text!, trailingNode);
                    break;

                case SyntaxKind.NumericLiteralToken:
                    token = SyntaxFactory.Literal(leadingNode, info.Text!, info.DoubleValue, trailingNode);
                    break;

                case SyntaxKind.StringLiteralToken:
                    token = SyntaxFactory.Literal(leadingNode, info.Text!, info.StringValue!, trailingNode);
                    break;

                case SyntaxKind.EndOfFileToken:
                    token = SyntaxFactory.Token(leadingNode, info.Kind, trailingNode);
                    break;

                case SyntaxKind.None:
                    token = SyntaxFactory.BadToken(leadingNode, info.Text!, trailingNode);
                    break;

                default:
                    RoslynDebug.Assert(SyntaxFacts.GetText(info.Kind) is not (null or ""));
                    token = SyntaxFactory.Token(leadingNode, info.Kind, trailingNode);
                    break;
            }

            if (errors != null) token = token.WithDiagnosticsGreen(errors);
            return token;
        }

        private void LexSyntaxTrivia(bool isTrailing, SyntaxListBuilder builder)
        {
            builder.Clear();

            while (_reader.Peek() is char peek)
            {
                Start();

                switch (peek)
                {
                    case '-':
                        if (_reader.IsAt(1, '-'))
                        {
                            _reader.Position += 2;
                            if (TryReadLongString(out _, out var closingNotFound))
                            {
                                if (closingNotFound)
                                    AddError(ErrorCode.ERR_UnfinishedLongComment);
                                AddTrivia(SyntaxFactory.Comment(GetText(intern: false)), builder);
                            }
                            else
                            {
                                while (_reader.Peek() is not (null or '\n' or '\r'))
                                    _reader.Position += 1;
                                AddTrivia(SyntaxFactory.Comment(GetText(intern: false)), builder);
                            }
                        }
                        else
                        {
                            return;
                        }
                        break;

                    case '/':
                        if (_reader.IsAt(1, '/'))
                        {
                            _reader.Position += 2;
                            while (_reader.Peek() is not (null or '\n' or '\r'))
                                _reader.Position += 1;

                            if (!Options.SyntaxOptions.AcceptCCommentSyntax)
                                AddError(ErrorCode.ERR_CCommentsNotSupportedInVersion);
                            AddTrivia(SyntaxFactory.Comment(GetText(intern: false)), builder);
                        }
                        else if (_reader.IsAt(1, '*'))
                        {
                            _reader.Position += 2;
                            _reader.SkipUntil("*/");

                            if (!_reader.IsNext("*/"))
                                AddError(ErrorCode.ERR_UnfinishedLongComment);
                            else
                                _reader.Position += 2;

                            if (!Options.SyntaxOptions.AcceptCCommentSyntax)
                                AddError(ErrorCode.ERR_CCommentsNotSupportedInVersion);
                            AddTrivia(SyntaxFactory.Comment(GetText(intern: false)), builder);
                        }
                        else
                        {
                            return;
                        }
                        break;

                    case '\r':
                        _reader.Position += 1;
                        if (_reader.Peek() == '\n')
                        {
                            _reader.Position += 1;
                            AddTrivia(SyntaxFactory.CarriageReturnLineFeed, builder);
                        }
                        else
                        {
                            AddTrivia(SyntaxFactory.CarriageReturn, builder);
                        }
                        if (isTrailing)
                            return;
                        break;

                    case '\n':
                        _reader.Position += 1;
                        if (_reader.IsNext('\r'))
                        {
                            _reader.Position += 1;
                            AddError(ErrorCode.WRN_LineBreakMayAffectErrorReporting);
                            AddTrivia(SyntaxFactory.EndOfLine("\n\r"), builder);
                        }
                        else
                        {
                            AddTrivia(SyntaxFactory.LineFeed, builder);
                        }
                        if (isTrailing)
                            return;
                        break;

                    case '\t':
                    case '\v':
                    case '\f':
                    case ' ':
                    {
                        // Skip first char as we know it's whitespace.
                        _reader.Position += 1;

                        char ch;
                        while ((ch = _reader.Peek().GetValueOrDefault()) != '\r'
                                && ch != '\n'
                                && CharUtils.IsWhitespace(ch))
                        {
                            _reader.Position += 1;
                        }

                        var width = _reader.Position - _start;
                        if (width == 1 && peek == ' ')
                        {
                            AddTrivia(SyntaxFactory.Space, builder);
                        }
                        else if (width == 1 && peek == '\t')
                        {
                            AddTrivia(SyntaxFactory.Tab, builder);
                        }
                        else
                        {
                            var text = GetText(intern: false);
                            var hash = Hash.GetFNVHashCode(text);
                            AddTrivia(_cache.LookupTrivia(text, hash, _createWhitespaceTriviaFunction), builder);
                        }
                        break;
                    }

                    case '#':
                        if (_reader.Position == 0 && _reader.IsAt(1, '!'))
                        {
                            _reader.Position += 2;
                            _reader.SkipUntilLineBreak();

                            if (!Options.SyntaxOptions.AcceptShebang)
                                AddError(ErrorCode.ERR_ShebangNotSupportedInLuaVersion);
                            AddTrivia(SyntaxFactory.Shebang(GetText(intern: false)), builder);
                            break;
                        }
                        else
                        {
                            return;
                        }
                    default:
                        return;
                }
            }
        }

        private readonly Func<SyntaxTrivia> _createWhitespaceTriviaFunction;

        private SyntaxTrivia CreateWhitespaceTrivia() => SyntaxFactory.Whitespace(GetText(intern: true));

        private void AddTrivia(LuaSyntaxNode trivia, SyntaxListBuilder builder)
        {
            if (HasErrors) trivia = trivia.WithDiagnosticsGreen(GetErrors(leadingTriviaWidth: 0));
            builder.Add(trivia);
        }

        private void LexSyntaxToken(ref TokenInfo info)
        {
            // Initialize for new token scan
            info.Kind = SyntaxKind.None;
            info.ContextualKind = SyntaxKind.None;
            info.Text = null;
            info.StringValue = null;
            info.DoubleValue = default;

            if (_reader.Peek() is not char peek0)
            {
                info.Kind = SyntaxKind.EndOfFileToken;
                return;
            }

            switch (peek0)
            {
                #region Punctuation

                case '.':
                {
                    var peek1 = _reader.Peek(1).GetValueOrDefault();
                    if (peek1 == '.')
                    {
                        // \.\.\.
                        if (_reader.IsAt(2, '.'))
                        {
                            _reader.Position += 3;
                            info.Kind = SyntaxKind.DotDotDotToken;
                            return;
                        }
                        // \.\.=
                        else if (_reader.IsAt(2, '='))
                        {
                            _reader.Position += 3;
                            info.Kind = SyntaxKind.DotDotEqualsToken;
                            return;
                        }
                        // \.\.
                        else
                        {
                            _reader.Position += 2;
                            info.Kind = SyntaxKind.DotDotToken;
                            return;
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
                        _reader.Position += 1;
                        info.Kind = SyntaxKind.DotToken;
                        return;
                    }
                }

                case ';':
                    _reader.Position += 1;
                    info.Kind = SyntaxKind.SemicolonToken;
                    return;

                case ',':
                    _reader.Position += 1;
                    info.Kind = SyntaxKind.CommaToken;
                    return;

                case ':':
                    if (_reader.IsAt(1, ':'))
                    {
                        _reader.Position += 2;
                        info.Kind = SyntaxKind.ColonColonToken;
                        return;
                    }
                    else
                    {
                        _reader.Position += 1;
                        info.Kind = SyntaxKind.ColonToken;
                        return;
                    }

                #endregion Punctuation

                case '(':
                    _reader.Position += 1;
                    info.Kind = SyntaxKind.OpenParenthesisToken;
                    return;

                case ')':
                    _reader.Position += 1;
                    info.Kind = SyntaxKind.CloseParenthesisToken;
                    return;

                case '[':
                {
                    if (TryReadLongString(out var contents, out var closingNotFound))
                    {
                        if (closingNotFound)
                            AddError(ErrorCode.ERR_UnfinishedString);

                        info.Kind = SyntaxKind.StringLiteralToken;
                        info.Text = GetText(intern: false);
                        info.StringValue = contents;
                    }
                    else
                    {
                        _reader.Position += 1;
                        info.Kind = SyntaxKind.OpenBracketToken;
                    }
                    return;
                }

                case ']':
                    _reader.Position += 1;
                    info.Kind = SyntaxKind.CloseBracketToken;
                    return;

                case '{':
                    _reader.Position += 1;
                    info.Kind = SyntaxKind.OpenBraceToken;
                    return;

                case '}':
                    _reader.Position += 1;
                    info.Kind = SyntaxKind.CloseBraceToken;
                    return;

                #region Operators

                case '+':
                    if (_reader.IsAt(1, '='))
                    {
                        _reader.Position += 2;
                        info.Kind = SyntaxKind.PlusEqualsToken;
                        return;
                    }
                    else
                    {
                        _reader.Position += 1;
                        info.Kind = SyntaxKind.PlusToken;
                        return;
                    }

                case '-':
                    if (_reader.IsAt(1, '='))
                    {
                        _reader.Position += 2;
                        info.Kind = SyntaxKind.MinusEqualsToken;
                        return;
                    }
                    else
                    {
                        _reader.Position += 1;
                        info.Kind = SyntaxKind.MinusToken;
                        return;
                    }

                case '*':
                    if (_reader.IsAt(1, '='))
                    {
                        _reader.Position += 2;
                        info.Kind = SyntaxKind.StartEqualsToken;
                        return;
                    }
                    else
                    {
                        _reader.Position += 1;
                        info.Kind = SyntaxKind.StarToken;
                        return;
                    }

                case '/':
                    if (_reader.IsAt(1, '='))
                    {
                        _reader.Position += 2;
                        info.Kind = SyntaxKind.SlashEqualsToken;
                        return;
                    }
                    else
                    {
                        _reader.Position += 1;
                        info.Kind = SyntaxKind.SlashToken;
                        return;
                    }

                case '^':
                    if (_reader.IsAt(1, '='))
                    {
                        _reader.Position += 2;
                        info.Kind = SyntaxKind.HatEqualsToken;
                        return;
                    }
                    else
                    {
                        _reader.Position += 1;
                        info.Kind = SyntaxKind.HatToken;
                        return;
                    }

                case '%':
                    if (_reader.IsAt(1, '='))
                    {
                        _reader.Position += 2;
                        info.Kind = SyntaxKind.PercentEqualsToken;
                        return;
                    }
                    else
                    {
                        _reader.Position += 1;
                        info.Kind = SyntaxKind.PercentToken;
                        return;
                    }

                case '=':
                    if (_reader.IsAt(1, '='))
                    {
                        _reader.Position += 2;
                        info.Kind = SyntaxKind.EqualsEqualsToken;
                        return;
                    }
                    else
                    {
                        _reader.Position += 1;
                        info.Kind = SyntaxKind.EqualsToken;
                        return;
                    }

                case '#':
                    _reader.Position += 1;
                    info.Kind = SyntaxKind.HashToken;
                    return;

                case '~':
                    if (_reader.IsAt(1, '='))
                    {
                        _reader.Position += 2;
                        info.Kind = SyntaxKind.TildeEqualsToken;
                        return;
                    }
                    else
                    {
                        _reader.Position += 1;
                        if (!Options.SyntaxOptions.AcceptBitwiseOperators)
                            AddError(ErrorCode.ERR_BitwiseOperatorsNotSupportedInVersion);
                        info.Kind = SyntaxKind.TildeToken;
                        return;
                    }

                case '>':
                    switch (_reader.Peek(1))
                    {
                        case '=':
                            _reader.Position += 2;
                            info.Kind = SyntaxKind.GreaterThanEqualsToken;
                            return;

                        case '>':
                            _reader.Position += 2;
                            if (!Options.SyntaxOptions.AcceptBitwiseOperators)
                                AddError(ErrorCode.ERR_BitwiseOperatorsNotSupportedInVersion);
                            info.Kind = SyntaxKind.GreaterThanGreaterThanToken;
                            return;

                        default:
                            _reader.Position += 1;
                            info.Kind = SyntaxKind.GreaterThanToken;
                            return;
                    }

                case '<':
                    switch (_reader.Peek(1))
                    {
                        case '=':
                            _reader.Position += 2;
                            info.Kind = SyntaxKind.LessThanEqualsToken;
                            return;

                        case '<':
                            _reader.Position += 2;
                            if (!Options.SyntaxOptions.AcceptBitwiseOperators)
                                AddError(ErrorCode.ERR_BitwiseOperatorsNotSupportedInVersion);
                            info.Kind = SyntaxKind.LessThanLessThanToken;
                            return;

                        default:
                            _reader.Position += 1;
                            info.Kind = SyntaxKind.LessThanToken;
                            return;
                    }

                case '&':
                    if (_reader.IsAt(1, '&'))
                    {
                        _reader.Position += 2;
                        info.Kind = SyntaxKind.AmpersandAmpersandToken;
                        return;
                    }
                    else
                    {
                        _reader.Position += 1;
                        if (!Options.SyntaxOptions.AcceptBitwiseOperators)
                            AddError(ErrorCode.ERR_BitwiseOperatorsNotSupportedInVersion);
                        info.Kind = SyntaxKind.AmpersandToken;
                        return;
                    }

                case '|':
                    if (_reader.IsAt(1, '|'))
                    {
                        _reader.Position += 2;
                        info.Kind = SyntaxKind.PipePipeToken;
                        return;
                    }
                    else
                    {
                        _reader.Position += 1;
                        if (!Options.SyntaxOptions.AcceptBitwiseOperators)
                            AddError(ErrorCode.ERR_BitwiseOperatorsNotSupportedInVersion);
                        info.Kind = SyntaxKind.PipeToken;
                        return;
                    }

                case '!':
                    if (_reader.IsAt(1, '='))
                    {
                        _reader.Position += 2;
                        info.Kind = SyntaxKind.BangEqualsToken;
                        return;
                    }
                    else
                    {
                        _reader.Position += 1;
                        info.Kind = SyntaxKind.BangToken;
                        return;
                    }

                #endregion Operators

                #region Literals

                #region Numbers

                case '0':
                    switch (_reader.Peek(1))
                    {
                        // 0b[01_]+
                        case 'b':
                        case 'B':
                            // Skip the prefix
                            _reader.Position += 2;
                            info.Kind = SyntaxKind.NumericLiteralToken;
                            info.DoubleValue = ParseBinaryNumber();
                            info.Text = GetText(intern: true);
                            return;

                        // 0o[0-7_]+
                        case 'o':
                        case 'O':
                            // Skip the prefix
                            _reader.Position += 2;
                            info.Kind = SyntaxKind.NumericLiteralToken;
                            info.DoubleValue = ParseOctalNumber();
                            info.Text = GetText(intern: true);
                            return;

                        // 0x(?:[A-Fa-f0-9]?[A-Fa-f0-9_]*)?\.(?:[A-Fa-f0-9][A-Fa-f0-9_]*)?(?:[pP][0-9]+)?
                        case 'x':
                        case 'X':
                            // Skip the prefix
                            _reader.Position += 2;
                            info.Kind = SyntaxKind.NumericLiteralToken;
                            ParseHexadecimalNumber(ref info);
                            return;
                    }

                    goto case '1';

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
                    info.Kind = SyntaxKind.NumericLiteralToken;
                    ParseDecimalNumber(ref info);
                    return;

                #endregion Numbers

                case '"':
                case '\'':
                {
                    info.Kind = SyntaxKind.StringLiteralToken;
                    info.StringValue = ParseShortString();
                    info.Text = GetText(intern: true);
                    return;
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
                    _reader.Position += 1;
                    while (CharUtils.IsValidTrailingIdentifierChar(_reader.Peek().GetValueOrDefault()))
                        _reader.Position += 1;

                    info.Text = info.StringValue = GetText(intern: true);
                    if (!_cache.TryGetKeywordKind(info.Text, out info.Kind))
                        info.ContextualKind = info.Kind = SyntaxKind.IdentifierToken;

                    // Continue might be a contextual keyword or not a keyword at all so we have to check.
                    if (info.Kind is SyntaxKind.ContinueKeyword && Options.SyntaxOptions.ContinueType != ContinueType.Keyword)
                    {
                        info.Kind = SyntaxKind.IdentifierToken;
                        if (Options.SyntaxOptions.ContinueType == ContinueType.ContextualKeyword)
                            info.ContextualKind = SyntaxKind.ContinueKeyword;
                        info.Text = "continue";
                    }

                    if (!Options.SyntaxOptions.UseLuaJitIdentifierRules && info.Text.Any(ch => ch >= 0x7F))
                        AddError(ErrorCode.ERR_LuajitIdentifierRulesNotSupportedInVersion);
                    return;
                }

                #endregion Identifiers

                default:
                    if (CharUtils.IsValidFirstIdentifierChar(peek0))
                    {
                        goto case 'a';
                    }
                    else if (_badTokenCount++ > 200)
                    {
                        // If we get too many characters that we cannot make sense of, absorb the rest of the input.
                        info.Text = _reader.ReadToEnd();
                    }
                    else
                    {
                        _reader.Position += 1;
                        info.Text = GetText(intern: true);
                    }
                    AddError(ErrorCode.ERR_BadCharacter, info.Text);
                    return;
            } // end switch

            throw new Exception("Unreacheable.");
        }

        private bool TryReadLongString([NotNullWhen(true)] out string? contents, out bool closingNotFound)
        {
            closingNotFound = true;
            var start = _reader.Position;
            if (_reader.IsNext('[')
                 && (_reader.IsAt(1, '=')
                      || _reader.IsAt(1, '[')))
            {
                _reader.Position += 1;
                var equalSigns = _reader.ReadStringWhile(ch => ch == '=');
                if (_reader.IsNext('['))
                {
                    _reader.Position += 1;

                    var closing = $"]{equalSigns}]";
                    contents = _reader.ReadStringUntil(closing);

                    if (_reader.IsNext(closing))
                    {
                        closingNotFound = false;
                        _reader.Position += closing.Length;
                    }

                    return true;
                }

                _reader.Position = start;
            }

            contents = null;
            return false;
        }

        public void Dispose() => _cache.Free();
    }
}
