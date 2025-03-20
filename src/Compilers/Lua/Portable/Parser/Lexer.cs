using System.Numerics;
using System.Text;
using Loretta.CodeAnalysis.Lua.Utilities;
using Loretta.CodeAnalysis.Syntax.InternalSyntax;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal sealed partial class Lexer : AbstractLexer, IDisposable
    {
        internal enum ValueKind
        {
            None = 0,
            String,
            Double,
            UInt,
            Long,
            ULong,
            Complex
        }

        internal struct TokenInfo
        {
            // scanned values
            internal SyntaxKind Kind;
            internal SyntaxKind ContextualKind;
            internal string?    Text;
            internal ValueKind  ValueKind;
            internal string?    StringValue;
            internal double     DoubleValue;
            internal uint       UIntValue;
            internal long       LongValue;
            internal ulong      ULongValue;
            internal Complex    ComplexValue;
        }

        private readonly LuaParseOptions _options;
        private readonly StringBuilder   _builder = new();

        private readonly LexerCache        _cache               = new();
        private readonly SyntaxListBuilder _leadingTriviaCache  = new(size: 10);
        private readonly SyntaxListBuilder _trailingTriviaCache = new(10);
        private          int               _badTokenCount; // cumulative count of bad tokens produced

        private static int GetFullWidth(SyntaxListBuilder builder)
        {
            var width                                           = 0;
            for (var idx = 0; idx < builder.Count; idx++) width += builder[idx]!.FullWidth;
            return width;
        }

        public Lexer(SourceText text, LuaParseOptions options) : base(text)
        {
            LorettaDebug.Assert(options is not null);
            _options                        = options;
            _createWhitespaceTriviaFunction = CreateWhitespaceTrivia;
            _createQuickTokenFunction       = CreateQuickToken;
        }

        public LuaParseOptions Options => _options;

        public override void Dispose()
        {
            _cache.Free();
            base.Dispose();
        }

        public SyntaxToken Lex() => QuickScanSyntaxToken() ?? LexSyntaxToken();

        private SyntaxToken LexSyntaxToken()
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
                token: default(CodeAnalysis.SyntaxToken),
                _leadingTriviaCache.ToListNode(),
                position: 0,
                index: 0);
        }

        internal SyntaxTriviaList LexSyntaxTrailingTrivia()
        {
            _trailingTriviaCache.Clear();
            LexSyntaxTrivia(isTrailing: true, _trailingTriviaCache);
            return new SyntaxTriviaList(
                token: default(CodeAnalysis.SyntaxToken),
                _trailingTriviaCache.ToListNode(),
                position: 0,
                index: 0);
        }

        private static SyntaxToken Create(
            ref TokenInfo           info,
            SyntaxListBuilder       leading,
            SyntaxListBuilder       trailing,
            SyntaxDiagnosticInfo[]? errors)
        {
            LorettaDebug.Assert(info.Kind != SyntaxKind.IdentifierToken || info.StringValue != null);

            var leadingNode  = leading.ToListNode();
            var trailingNode = trailing.ToListNode();

            SyntaxToken token;
            switch (info.Kind)
            {
                case SyntaxKind.IdentifierToken:
                    LorettaDebug.AssertNotNull(info.Text);
                    LorettaDebug.Assert(info.ValueKind is ValueKind.None);

                    token = SyntaxFactory.Identifier(info.ContextualKind, leadingNode, info.Text, trailingNode);
                    break;

                case SyntaxKind.NumericLiteralToken:
                    LorettaDebug.AssertNotNull(info.Text);
                    LorettaDebug.Assert(
                        info.ValueKind is ValueKind.Double or ValueKind.Long or ValueKind.ULong or ValueKind.Complex);

                    token = info.ValueKind switch
                    {
                        ValueKind.Double =>
                            SyntaxFactory.Literal(leadingNode, info.Text, info.DoubleValue, trailingNode),
                        ValueKind.Long =>
                            SyntaxFactory.Literal(leadingNode, info.Text, info.LongValue, trailingNode),
                        ValueKind.ULong => SyntaxFactory.Literal(
                            leadingNode,
                            info.Text,
                            info.ULongValue,
                            trailingNode),
                        ValueKind.Complex => SyntaxFactory.Literal(
                            leadingNode,
                            info.Text,
                            info.ComplexValue,
                            trailingNode),
                        _ => throw ExceptionUtilities.UnexpectedValue(info.ValueKind),
                    };
                    break;

                case SyntaxKind.StringLiteralToken:
                    LorettaDebug.AssertNotNull(info.Text);
                    LorettaDebug.Assert(info.ValueKind is ValueKind.String);

                    token = SyntaxFactory.Literal(leadingNode, info.Text, info.StringValue!, trailingNode);
                    break;

                case SyntaxKind.HashStringLiteralToken:
                    LorettaDebug.AssertNotNull(info.Text);
                    LorettaDebug.Assert(info.ValueKind is ValueKind.UInt);

                    token = SyntaxFactory.HashLiteral(leadingNode, info.Text, info.UIntValue, trailingNode);
                    break;
                
                case SyntaxKind.InterpolatedStringToken:
                    LorettaDebug.AssertNotNull(info.Text);

                    // we do not record a separate "value" for an interpolated string token, as it must be rescanned during parsing.
                    token = SyntaxFactory.Literal(
                        leadingNode,
                        info.Text,
                        SyntaxKind.InterpolatedStringToken,
                        info.Text,
                        trailingNode);
                    break;

                case SyntaxKind.EndOfFileToken:
                    LorettaDebug.Assert(info.Text is null);
                    LorettaDebug.Assert(info.ValueKind is ValueKind.None);

                    token = SyntaxFactory.Token(leadingNode, info.Kind, trailingNode);
                    break;

                case SyntaxKind.None:
                    LorettaDebug.AssertNotNull(info.Text);
                    LorettaDebug.Assert(info.ValueKind is ValueKind.None);

                    token = SyntaxFactory.BadToken(leadingNode, info.Text, trailingNode);
                    break;

                default:
                    LorettaDebug.Assert(SyntaxFacts.GetText(info.Kind) is not (null or ""));
                    LorettaDebug.Assert(info.ValueKind is ValueKind.None);

                    token = SyntaxFactory.Token(leadingNode, info.Kind, trailingNode);
                    break;
            }

            if (errors != null) token = token.WithDiagnosticsGreen(errors);
            return token;
        }

        private void LexSyntaxToken(ref TokenInfo info)
        {
            // Initialize for new token scan
            info.Kind           = SyntaxKind.None;
            info.ContextualKind = SyntaxKind.None;
            info.Text           = null;
            info.ValueKind      = ValueKind.None;
            var startingPosition = TextWindow.Position;

            char ch;
            switch (ch = TextWindow.PeekChar())
            {
                #region Punctuation

                case '.':
                    if ((ch = TextWindow.PeekChar(1)) == '.')
                    {
                        TextWindow.AdvanceChar(2);
                        // \.\.\.
                        if ((ch = TextWindow.PeekChar()) == '.')
                        {
                            TextWindow.AdvanceChar();
                            info.Kind = SyntaxKind.DotDotDotToken;
                        }
                        // \.\.=
                        else if (ch == '=')
                        {
                            TextWindow.AdvanceChar();
                            info.Kind = SyntaxKind.DotDotEqualsToken;
                        }
                        // \.\.
                        else
                        {
                            info.Kind = SyntaxKind.DotDotToken;
                        }
                    }
                    // \.[0-9]
                    else if (CharUtils.IsDecimal(ch))
                    {
                        goto case '1';
                    }
                    // \.
                    else
                    {
                        TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.DotToken;
                    }
                    break;

                case ';':
                    TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.SemicolonToken;
                    break;

                case ',':
                    TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.CommaToken;
                    break;

                case ':':
                    TextWindow.AdvanceChar();
                    if (_options.SyntaxOptions.AcceptGoto && TextWindow.PeekChar() == ':')
                    {
                        TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.ColonColonToken;
                    }
                    else
                    {
                        info.Kind = SyntaxKind.ColonToken;
                    }
                    break;

                case '?':
                    TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.QuestionToken;
                    break;

                #endregion Punctuation

                case '(':
                    TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.OpenParenthesisToken;
                    break;

                case ')':
                    TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.CloseParenthesisToken;
                    break;

                case '[':
                {
                    if (TryScanLongString(out var contentStart, out var contentEnd, out var isTerminated))
                    {
                        if (!isTerminated) AddError(ErrorCode.ERR_UnfinishedString);

                        info.Kind        = SyntaxKind.StringLiteralToken;
                        info.Text        = TextWindow.GetText(intern: true);
                        info.ValueKind   = ValueKind.String;
                        info.StringValue = TextWindow.GetText(contentStart, contentEnd - contentStart, intern: true);
                    }
                    else
                    {
                        TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.OpenBracketToken;
                    }
                    break;
                }

                case ']':
                    TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.CloseBracketToken;
                    break;

                case '{':
                    TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.OpenBraceToken;
                    break;

                case '}':
                    TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.CloseBraceToken;
                    break;

                #region Operators

                case '#':
                    TextWindow.AdvanceChar();
                    info.Kind = SyntaxKind.HashToken;
                    return;

                case '+':
                    TextWindow.AdvanceChar();
                    if (TextWindow.PeekChar() == '=')
                    {
                        TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.PlusEqualsToken;
                    }
                    else
                    {
                        info.Kind = SyntaxKind.PlusToken;
                    }
                    break;

                case '-':
                    TextWindow.AdvanceChar();
                    if ((ch = TextWindow.PeekChar()) == '>')
                    {
                        TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.MinusGreaterThanToken;
                    }
                    else if (ch == '=')
                    {
                        TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.MinusEqualsToken;
                    }
                    else
                    {
                        info.Kind = SyntaxKind.MinusToken;
                    }
                    break;

                case '*':
                    TextWindow.AdvanceChar();
                    if (TextWindow.PeekChar() == '=')
                    {
                        TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.StarEqualsToken;
                    }
                    else
                    {
                        info.Kind = SyntaxKind.StarToken;
                    }
                    break;

                case '/':
                    TextWindow.AdvanceChar();

                    switch (TextWindow.PeekChar())
                    {
                        case '=':
                            TextWindow.AdvanceChar();
                            info.Kind = SyntaxKind.SlashEqualsToken;
                            break;

                        case '/' when _options.SyntaxOptions.AcceptFloorDivision:
                            TextWindow.AdvanceChar();
                            info.Kind = SyntaxKind.SlashSlashToken;
                            break;

                        default: info.Kind = SyntaxKind.SlashToken; break;
                    }

                    break;

                case '^':
                    TextWindow.AdvanceChar();
                    if (TextWindow.PeekChar() == '=')
                    {
                        TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.HatEqualsToken;
                    }
                    else
                    {
                        info.Kind = SyntaxKind.HatToken;
                    }
                    break;

                case '%':
                    TextWindow.AdvanceChar();
                    if (TextWindow.PeekChar() == '=')
                    {
                        TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.PercentEqualsToken;
                    }
                    else
                    {
                        info.Kind = SyntaxKind.PercentToken;
                    }
                    break;

                case '=':
                    TextWindow.AdvanceChar();
                    if (TextWindow.PeekChar() == '=')
                    {
                        TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.EqualsEqualsToken;
                    }
                    else
                    {
                        info.Kind = SyntaxKind.EqualsToken;
                    }
                    break;

                case '~':
                    TextWindow.AdvanceChar();
                    if (TextWindow.PeekChar() == '=')
                    {
                        TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.TildeEqualsToken;
                    }
                    else
                    {
                        info.Kind = SyntaxKind.TildeToken;
                    }
                    break;

                case '!':
                    TextWindow.AdvanceChar();
                    if (TextWindow.PeekChar() == '=')
                    {
                        TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.BangEqualsToken;
                    }
                    else
                    {
                        info.Kind = SyntaxKind.BangToken;
                    }
                    break;

                case '>':
                    TextWindow.AdvanceChar();
                    if (TextWindow.PeekChar() == '=')
                    {
                        TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.GreaterThanEqualsToken;
                    }
                    else
                    {
                        info.Kind = SyntaxKind.GreaterThanToken;
                    }
                    break;

                case '<':
                    TextWindow.AdvanceChar();
                    if ((ch = TextWindow.PeekChar()) == '=')
                    {
                        TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.LessThanEqualsToken;
                    }
                    else if (ch == '<')
                    {
                        TextWindow.AdvanceChar();
                        if (!_options.SyntaxOptions.AcceptBitwiseOperators)
                            AddError(ErrorCode.ERR_BitwiseOperatorsNotSupportedInVersion);
                        info.Kind = SyntaxKind.LessThanLessThanToken;
                    }
                    else
                    {
                        info.Kind = SyntaxKind.LessThanToken;
                    }
                    break;

                case '&':
                    TextWindow.AdvanceChar();
                    if (TextWindow.PeekChar() == '&')
                    {
                        TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.AmpersandAmpersandToken;
                    }
                    else
                    {
                        info.Kind = SyntaxKind.AmpersandToken;
                    }
                    break;

                case '|':
                    TextWindow.AdvanceChar();
                    if (TextWindow.PeekChar() == '|')
                    {
                        TextWindow.AdvanceChar();
                        info.Kind = SyntaxKind.PipePipeToken;
                    }
                    else
                    {
                        info.Kind = SyntaxKind.PipeToken;
                    }
                    break;

                #endregion Operators

                #region Literals

                #region Numbers

                case '0':
                    switch (TextWindow.PeekChar(1))
                    {
                        // 0b[01_]+
                        case 'b':
                        case 'B':
                            // Skip the prefix
                            TextWindow.AdvanceChar(2);
                            info.Kind = SyntaxKind.NumericLiteralToken;
                            ParseBinaryNumber(ref info);
                            return;

                        // 0o[0-7_]+
                        case 'o':
                        case 'O':
                            // Skip the prefix
                            TextWindow.AdvanceChar(2);
                            info.Kind = SyntaxKind.NumericLiteralToken;
                            ParseOctalNumber(ref info);
                            return;

                        // 0x(?:[A-Fa-f0-9]?[A-Fa-f0-9_]*)?\.(?:[A-Fa-f0-9][A-Fa-f0-9_]*)?(?:[pP][0-9]+)?
                        case 'x':
                        case 'X':
                            // Skip the prefix
                            TextWindow.AdvanceChar(2);
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
                    ScanStringLiteral(ref info);
                    return;

                case '`':
                    if (_options.SyntaxOptions.BacktickStringType == BacktickStringType.HashLiteral)
                        ScanStringLiteral(ref info);
                    else
                        ScanInterpolatedStringLiteral(ref info);
                    break;

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
                    ScanIdentifierOrKeyword(ref info);
                    return;
                }

                #endregion Identifiers

                case SlidingTextWindow.InvalidCharacter:
                    if (!TextWindow.IsReallyAtEnd()) goto default;
                    info.Kind = SyntaxKind.EndOfFileToken;
                    break;

                default:
                    if (CharUtils.IsValidFirstIdentifierChar(ch)) goto case 'a';

                    if (_badTokenCount++ > 200)
                    {
                        // If we get too many characters that we cannot make sense of, absorb the rest of the input.
                        var end   = TextWindow.Text.Length;
                        var width = end - startingPosition;
                        info.Text = TextWindow.Text.ToString(new TextSpan(startingPosition, width));
                        TextWindow.Reset(end);
                    }
                    else
                    {
                        TextWindow.AdvanceChar();
                        info.Text = TextWindow.GetText(intern: true);
                    }
                    AddError(ErrorCode.ERR_BadCharacter, info.Text);
                    break;
            } // end switch
        }

        private int ConsumeCharSequence(char ch)
        {
            var start = TextWindow.Position;
            while (TextWindow.PeekChar() == ch) TextWindow.AdvanceChar();
            return TextWindow.Position - start;
        }

        private void LexSyntaxTrivia(bool isTrailing, SyntaxListBuilder builder)
        {
            builder.Clear();

            var onlyShebangsAndNewLines = true;
            while (true)
            {
                Start();

                var ch = TextWindow.PeekChar();
                if (ch is ' ' or '\t')
                {
                    AddTrivia(ScanWhitespace(), builder);
                    continue;
                }

                switch (ch)
                {
                    case ' ':
                    case '\t':
                    case '\v':
                    case '\f':
                        AddTrivia(ScanWhitespace(), builder);
                        onlyShebangsAndNewLines = false;
                        break;

                    case '-':
                    {
                        // Abort if it isn't a comment.
                        if (!TryScanComment(out var isTerminated)) return;

                        if (!isTerminated) AddError(ErrorCode.ERR_UnfinishedLongComment);

                        var text = TextWindow.GetText(intern: false);
                        AddTrivia(SyntaxFactory.Comment(text), builder);
                        onlyShebangsAndNewLines = false;
                        break;
                    }

                    case '/':
                    {
                        if (!_options.SyntaxOptions.AcceptCCommentSyntax) return;
                        if (!TryScanCComment(out var isTerminated)) return;
                        onlyShebangsAndNewLines = false;
                        if (!isTerminated) AddError(ErrorCode.ERR_UnfinishedLongComment);

                        var text = TextWindow.GetText(intern: false);
                        AddTrivia(SyntaxFactory.Comment(text), builder);
                        break;
                    }

                    case '\r':
                    case '\n':
                        AddTrivia(ScanEndOfLine()!, builder);
                        if (isTrailing) return;
                        break;

                    case '#':
                    {
                        if (!onlyShebangsAndNewLines || TextWindow.PeekChar(1) != '!') return;

                        ScanToEndOfLine();
                        if (!_options.SyntaxOptions.AcceptShebang)
                            AddError(ErrorCode.ERR_ShebangNotSupportedInLuaVersion);

                        var text = TextWindow.GetText(false);
                        AddTrivia(SyntaxFactory.Shebang(text), builder);
                        break;
                    }

                    default: return;
                }
            }
        }

        public bool TryScanComment(out bool isTerminated)
        {
            isTerminated = true;
            if (TextWindow.PeekChar() != '-' || TextWindow.PeekChar(delta: 1) != '-') return false;

            // Skip leading '--'
            TextWindow.AdvanceChar(n: 2);

            // Try to scan long comment
            if (TryScanLongString(out _, out _, out isTerminated)) return true;

            // Scan to end of line since it's single-line comment
            isTerminated = true;
            ScanToEndOfLine();
            return true;
        }

        public bool TryScanCComment(out bool isTerminated)
        {
            isTerminated = true;

            if (TextWindow.PeekChar() != '/') return false;

            var ch = TextWindow.PeekChar(delta: 1);
            switch (ch)
            {
                case '/': ScanToEndOfLine(); break;
                case '*': ScanMultiLineCComment(out isTerminated); break;
                default:  return false;
            }
            return true;
        }

        private SyntaxTrivia ScanWhitespace()
        {
            _createWhitespaceTriviaFunction ??= CreateWhitespaceTrivia;

            var  hashCode = Hash.FnvOffsetBias;
            var ch       = SlidingTextWindow.InvalidCharacter;

        top:
            var lastCh = ch;
            ch = TextWindow.PeekChar();

            switch (ch)
            {
                case '\t': // Horizontal tab
                case '\v': // Vertical Tab
                case '\f': // Form-feed
                case ' ':
                    TextWindow.AdvanceChar();
                    hashCode = Hash.CombineFNVHash(hashCode, ch);
                    goto top;

                case '\r': // Carriage Return
                case '\n': // Line-feed
                    break;
            }

            if (TextWindow.Width == 1 && lastCh == ' ') return SyntaxFactory.Space;
            if (TextWindow.Width == 1 && lastCh == '\t') return SyntaxFactory.Tab;
            
            var width = TextWindow.Width;

            if (width < MaxCachedTokenSize)
            {
                return _cache.LookupTrivia(
                    TextWindow.CharacterWindow,
                    TextWindow.LexemeRelativeStart,
                    width,
                    hashCode,
                    _createWhitespaceTriviaFunction);
            }

            return _createWhitespaceTriviaFunction();
        }

        private void ScanMultiLineCComment(out bool isTerminated)
        {
            isTerminated = false;

            LorettaDebug.Assert(TextWindow.PeekChar() == '/' && TextWindow.PeekChar(1) == '*');
            TextWindow.AdvanceChar(2);

            while (true)
            {
                var ch = TextWindow.PeekChar();

                if (IsAtEnd(ch)) break;

                if (ch == '*' && TextWindow.PeekChar(1) == '/')
                {
                    TextWindow.AdvanceChar(2);
                    isTerminated = true;
                    break;
                }

                TextWindow.AdvanceChar();
            }
        }

        private Func<SyntaxTrivia> _createWhitespaceTriviaFunction;

        private SyntaxTrivia CreateWhitespaceTrivia() => SyntaxFactory.Whitespace(TextWindow.GetText(intern: true));

        private void AddTrivia(LuaSyntaxNode trivia, SyntaxListBuilder builder)
        {
            if (HasErrors) trivia = trivia.WithDiagnosticsGreen(GetErrors(leadingTriviaWidth: 0));
            builder.Add(trivia);
        }

        public bool TryScanLongString(out int contentStart, out int contentEnd, out bool isTerminated)
        {
            contentStart = contentEnd = 0;
            isTerminated = false;

            var start = TextWindow.Position;
            if (TextWindow.PeekChar() != '[' || TextWindow.PeekChar(delta: 1) is not ('=' or '[')) return false;

            TextWindow.AdvanceChar();
            var initialEqualsCount = ConsumeCharSequence(ch: '=');

            if (TextWindow.PeekChar() == '[')
            {
                TextWindow.AdvanceChar();

                // Skips the leading new line if we have any.
                _ = ScanEndOfLine();

                contentStart = TextWindow.Position;
                while (true)
                {
                    var ch = TextWindow.PeekChar();

                    if (IsAtEnd(ch))
                    {
                        contentEnd = TextWindow.Position;
                        break;
                    }

                    switch (ch)
                    {
                        case '[' when !_options.SyntaxOptions.AcceptNestingOfLongStrings:
                        {
                            TextWindow.AdvanceChar();
                            if (TextWindow.PeekChar() == '[')
                            {
                                TextWindow.AdvanceChar();
                                AddError(ErrorCode.ERR_Lua51NestingInLongString);
                            }
                            continue;
                        }

                        case ']':
                        {
                            break;
                        }

                        default:
                        {
                            // If not a possible ending, just skip over it.
                            TextWindow.AdvanceChar();
                            continue;
                        }
                    }

                    contentEnd = TextWindow.Position;
                    TextWindow.AdvanceChar();

                    var equalsCount = ConsumeCharSequence(ch: '=');
                    if (initialEqualsCount != equalsCount) continue;

                    if (TextWindow.PeekChar() != ']') continue;

                    TextWindow.AdvanceChar();
                    isTerminated = true;
                    break;
                }

                return true;
            }

            TextWindow.Reset(start);

            return false;
        }

        private bool IsAtEnd(char ch) => ch == SlidingTextWindow.InvalidCharacter && TextWindow.IsReallyAtEnd();

        private void ScanToEndOfLine()
        {
            char ch;
            while (!CharUtils.IsNewLine(ch = TextWindow.PeekChar()) && !IsAtEnd(ch)) TextWindow.AdvanceChar();
        }

        private SyntaxTrivia? ScanEndOfLine()
        {
            switch (TextWindow.PeekChar())
            {
                case '\n':
                    TextWindow.AdvanceChar();
                    if (TextWindow.PeekChar() == '\r')
                    {
                        TextWindow.AdvanceChar();
                        AddError(ErrorCode.WRN_LineBreakMayAffectErrorReporting);
                        return SyntaxFactory.LineFeedCarriageReturn;
                    }
                    return SyntaxFactory.LineFeed;

                case '\r':
                    TextWindow.AdvanceChar();
                    if (TextWindow.PeekChar() == '\n')
                    {
                        TextWindow.AdvanceChar();
                        return SyntaxFactory.CarriageReturnLineFeed;
                    }
                    return SyntaxFactory.CarriageReturn;

                default: return null;
            }
        }
    }
}
