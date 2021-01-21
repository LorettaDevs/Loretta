using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using GParse;
using GParse.IO;
using GParse.Math;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Syntax
{
    internal partial class Lexer : IRestorablePositionContainer
    {
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public Boolean IsValidIdentifierLeadingCharacter ( Char ch ) =>
            LoCharUtils.IsValidFirstIdentifierChar ( this._luaOptions.UseLuaJitIdentifierRules, ch );

        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public Boolean IsValidIdentifierTrailingCharacter ( Char ch ) =>
            LoCharUtils.IsValidTrailingIdentifierChar ( this._luaOptions.UseLuaJitIdentifierRules, ch );

        private readonly LuaOptions _luaOptions;
        private readonly SourceText _sourceText;
        private readonly ICodeReader _reader;

        private Int32 _start;
        private readonly ImmutableArray<SyntaxTrivia>.Builder _triviaBuilder = ImmutableArray.CreateBuilder<SyntaxTrivia> ( );
        private readonly SyntaxTree syntaxTree;

        public DiagnosticList Diagnostics { get; }

        public Int32 Length => this._reader.Length;

        public Int32 Position => this._reader.Position;

        public Lexer ( SyntaxTree syntaxTree )
        {
            this.syntaxTree = syntaxTree;
            this._luaOptions = syntaxTree.Options;
            this._sourceText = syntaxTree.Text;
            this._reader = this._sourceText.GetReader ( );
            this.Diagnostics = new DiagnosticList ( );
        }

        public SyntaxToken Lex ( )
        {
            ImmutableArray<SyntaxTrivia> leadingTrivia = this.ReadTrivia ( leading: true );
            var tokenStart = this._reader.Position;

            (SyntaxKind tokenKind, var tokenValue) = this.ReadToken ( );

            var tokenLength = this._reader.Position - tokenStart;

            ImmutableArray<SyntaxTrivia> trailingTrivia = this.ReadTrivia ( leading: false );

            var tokenText = SyntaxFacts.GetText ( tokenKind )
                            ?? this._sourceText.ToString ( tokenStart, tokenLength );

            return new SyntaxToken ( this.syntaxTree, tokenKind, tokenStart, tokenText, tokenValue, leadingTrivia, trailingTrivia );
        }

        private ImmutableArray<SyntaxTrivia> ReadTrivia ( Boolean leading )
        {
            this._triviaBuilder.Clear ( );

            while ( this._reader.Peek ( ) is Char peek )
            {
                this._start = this._reader.Position;

                switch ( peek )
                {
                    case '-':
                        if ( this._reader.IsAt ( '-', 1 ) )
                        {
                            this._reader.Advance ( 2 );
                            if ( this.TryReadLongString ( out _, out var closingNotFound ) )
                            {
                                if ( closingNotFound )
                                {
                                    SourceRange? sourceRange = this._reader.GetLocation ( (this._start, this._reader.Position) );
                                    LuaDiagnostics.UnfinishedLongComment.ReportTo ( this.Diagnostics, sourceRange );
                                }

                                submitTrivia ( SyntaxKind.MultiLineCommentTrivia );
                            }
                            else
                            {
                                // A span is stack allocated so if we ignore it we won't allocate heap memory unnecessarily
                                _ = this._reader.ReadSpanLine ( );
                                submitTrivia ( SyntaxKind.SingleLineCommentTrivia );
                            }
                        }
                        else
                        {
                            goto end;
                        }
                        break;

                    case '/':
                        if ( this._reader.IsAt ( '/', 1 ) )
                        {
                            this._reader.Advance ( 2 );
                            _ = this._reader.ReadSpanLine ( );

                            if ( this._luaOptions.AcceptCCommentSyntax )
                            {

                            }

                            submitTrivia ( SyntaxKind.SingleLineCommentTrivia );
                        }
                        else if ( this._reader.IsAt ( '*', 1 ) )
                        {
                            this._reader.Advance ( 2 );
                            _ = this._reader.ReadSpanUntil ( "*/" );
                            submitTrivia ( SyntaxKind.MultiLineCommentTrivia );
                        }
                        else
                        {
                            goto end;
                        }
                        break;

                    case '\r':
                        if ( !leading )
                            goto end;
                        this._reader.Advance ( 1 );
                        if ( this._reader.Peek ( ) == '\n' )
                            this._reader.Advance ( 1 );
                        submitTrivia ( SyntaxKind.LineBreakTrivia );
                        break;

                    case '\n':
                        if ( !leading )
                            goto end;
                        this._reader.Advance ( 1 );
                        submitTrivia ( SyntaxKind.LineBreakTrivia );
                        break;

                    case ' ':
                    case '\t':
                    {
                        // Skip first char as we know it's whitespace.
                        this._reader.Advance ( 1 );

                        Char ch;
                        while ( ( ch = this._reader.Peek ( ).GetValueOrDefault ( ) ) != '\r'
                                && ch != '\n'
                                && Char.IsWhiteSpace ( ch ) )
                        {
                            this._reader.Advance ( 1 );
                        }
                        submitTrivia ( SyntaxKind.WhitespaceTrivia );
                        break;
                    }

                    case '#':
                        if ( this._reader.Position == 0 && this._reader.IsAt ( '!', 1 ) )
                        {
                            this._reader.Advance ( 2 );
                            _ = this._reader.ReadSpanLine ( );

                            if ( !this._luaOptions.AcceptShebang )
                            {
                                SourceRange range = this._reader.GetLocation ( (this._start, this._reader.Position) );
                                LuaDiagnostics.InvalidShebang.ReportTo ( this.Diagnostics, range );
                            }

                            submitTrivia ( SyntaxKind.ShebangTrivia );
                            break;
                        }
                        else
                        {
                            goto end;
                        }

                    default:
                        if ( Char.IsWhiteSpace ( peek ) )
                            goto case ' ';
                        else
                            goto end;
                }
            }

        end:
            return this._triviaBuilder.ToImmutable ( );

            void submitTrivia ( SyntaxKind kind )
            {
                var length = this._reader.Position - this._start;
                var text = this._sourceText.ToString ( this._start, length );
                this._triviaBuilder.Add ( new SyntaxTrivia ( this.syntaxTree, kind, this._start, text ) );
            }
        }

        public (SyntaxKind kind, Object? tokenValue) ReadToken ( )
        {
            this._start = this._reader.Position;

            if ( this._reader.Peek ( ) is not Char peek0 )
                return (SyntaxKind.EndOfFileToken, null);

            switch ( peek0 )
            {
                #region Punctuation

                case '.':
                {
                    var peek1 = this._reader.Peek ( 1 ).GetValueOrDefault ( );
                    if ( peek1 == '.' )
                    {
                        // \.\.\.
                        if ( this._reader.IsAt ( '.', 2 ) )
                        {
                            this._reader.Advance ( 3 );
                            return (SyntaxKind.DotDotDotToken, null);
                        }
                        // \.\.=
                        else if ( this._reader.IsAt ( '=', 2 ) )
                        {
                            this._reader.Advance ( 3 );
                            return (SyntaxKind.DotDotEqualsToken, null);
                        }
                        // \.\.
                        else
                        {
                            this._reader.Advance ( 2 );
                            return (SyntaxKind.DotDotToken, null);
                        }
                    }
                    // \.[0-9]
                    else if ( LoCharUtils.IsDecimal ( peek1 ) )
                    {
                        goto case '1';
                    }
                    // \.
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.DotToken, null);
                    }
                }

                case ';':
                    this._reader.Advance ( 1 );
                    return (SyntaxKind.SemicolonToken, null);

                case ',':
                    this._reader.Advance ( 1 );
                    return (SyntaxKind.CommaToken, null);

                case ':':
                    if ( this._reader.IsAt ( ':', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.GotoLabelDelimiterToken, null);
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.ColonToken, null);
                    }

                #endregion Punctuation

                case '(':
                    this._reader.Advance ( 1 );
                    return (SyntaxKind.OpenParenthesisToken, null);

                case ')':
                    this._reader.Advance ( 1 );
                    return (SyntaxKind.CloseParenthesisToken, null);

                case '[':
                {
                    if ( this.TryReadLongString ( out var contents, out var closingNotFound ) )
                    {
                        if ( closingNotFound )
                        {
                            SourceRange range = this._reader.GetLocation ( (this._start, this._reader.Position) );
                            LuaDiagnostics.UnfinishedString.ReportTo ( this.Diagnostics, range );
                        }

                        return (SyntaxKind.LongStringToken, contents);
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.OpenBracketToken, null);
                    }
                }


                case ']':
                    this._reader.Advance ( 1 );
                    return (SyntaxKind.CloseBracketToken, null);

                case '{':
                    this._reader.Advance ( 1 );
                    return (SyntaxKind.OpenBraceToken, null);

                case '}':
                    this._reader.Advance ( 1 );
                    return (SyntaxKind.CloseBraceToken, null);

                #region Operators

                case '+':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.PlusEqualsToken, null);
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.PlusToken, null);
                    }

                case '-':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.MinusEqualsToken, null);
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.MinusToken, null);
                    }

                case '*':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.StartEqualsToken, null);
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.StarToken, null);
                    }

                case '/':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.SlashEqualsToken, null);
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.SlashToken, null);
                    }

                case '^':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.HatEqualsToken, null);
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.HatToken, null);
                    }

                case '%':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.PercentEqualsToken, null);
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.PercentToken, null);
                    }

                case '=':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.EqualsEqualsToken, null);
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.EqualsToken, null);
                    }

                case '#':
                    this._reader.Advance ( 1 );
                    return (SyntaxKind.HashToken, null);

                case '~':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.TildeEqualsToken, null);
                    }
                    break;

                case '>':
                    switch ( this._reader.Peek ( 1 ) )
                    {
                        case '=':
                            this._reader.Advance ( 2 );
                            return (SyntaxKind.GreaterThanEqualsToken, null);

                        case '>':
                            this._reader.Advance ( 2 );
                            return (SyntaxKind.GreaterThanGreaterThanToken, null);

                        default:
                            this._reader.Advance ( 1 );
                            return (SyntaxKind.GreaterThanToken, null);
                    }

                case '<':
                    switch ( this._reader.Peek ( 1 ) )
                    {
                        case '=':
                            this._reader.Advance ( 2 );
                            return (SyntaxKind.LessThanEqualsToken, null);

                        case '<':
                            this._reader.Advance ( 2 );
                            return (SyntaxKind.LessThanLessThanToken, null);

                        default:
                            this._reader.Advance ( 1 );
                            return (SyntaxKind.LessThanToken, null);
                    }

                case '&':
                    if ( this._reader.IsAt ( '&', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.AmpersandAmpersandToken, null);
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.AmpersandToken, null);
                    }

                case '|':
                    if ( this._reader.IsAt ( '|', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.PipePipeToken, null);
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.PipeToken, null);
                    }

                case '!':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.BangEqualsToken, null);
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.BangToken, null);
                    }

                #endregion Operators

                #region Literals

                #region Numbers

                case '0':
                {
                    switch ( this._reader.Peek ( 1 ) )
                    {
                        // 0b[01_]+
                        case 'b':
                        {
                            // Skip the prefix
                            this._reader.Advance ( 2 );
                            var val = this.ParseBinaryNumber ( );
                            return (SyntaxKind.NumberToken, val);
                        }

                        // 0o[0-7_]+
                        case 'o':
                        {
                            // Skip the prefix
                            this._reader.Advance ( 2 );
                            var val = this.ParseOctalNumber ( );
                            return (SyntaxKind.NumberToken, val);
                        }

                        // 0x(?:[A-Fa-f0-9]?[A-Fa-f0-9_]*)?\.(?:[A-Fa-f0-9][A-Fa-f0-9_]*)?(?:[pP][0-9]+)?
                        case 'x':
                        {
                            // Skip the prefix
                            this._reader.Advance ( 2 );
                            var val = this.ParseHexadecimalNumber ( );
                            return (SyntaxKind.NumberToken, val);
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
                    var val = this.ParseDecimalNumber ( );
                    return (SyntaxKind.NumberToken, val);
                }

                #endregion Numbers

                case '"':
                case '\'':
                {
                    var val = this.ParseShortString ( );
                    return (SyntaxKind.ShortStringToken, val);
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
                    while ( this.IsValidIdentifierTrailingCharacter ( this._reader.Peek ( ).GetValueOrDefault ( ) ) )
                        this._reader.Advance ( 1 );

                    var text = this.GetString ( (this._start, end: this._reader.Position) );
                    return (SyntaxFacts.GetKeywordKind ( text ), null);
                }

                #endregion Identifiers

                default:
                {
                    if ( this.IsValidIdentifierLeadingCharacter ( peek0 ) )
                    {
                        goto case 'a';
                    }
                    else
                    {
                        this._reader.Restore ( this._start );
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.BadToken, null);
                    }
                }
            } // end switch

            throw new Exception ( "Unreacheable." );
        }

        private String GetString ( Range<Int32> range )
        {
            var pos = this._reader.Position;
            try
            {
                this._reader.Restore ( range.Start );
                return this._reader.ReadString ( range.End - range.Start )!;
            }
            finally
            {
                this._reader.Restore ( pos );
            }
        }

        private Boolean TryReadLongString ( [NotNullWhen ( true )] out String? contents, out Boolean closingNotFound )
        {
            closingNotFound = true;
            var start = this._reader.Position;
            if ( this._reader.IsNext ( '[' )
                 && ( this._reader.IsAt ( '=', 1 )
                      || this._reader.IsAt ( '[', 1 ) ) )
            {
                this._reader.Advance ( 1 );
                var equalSigns = this._reader.ReadStringWhile ( ch => ch == '=' );
                if ( this._reader.IsNext ( '[' ) )
                {
                    this._reader.Advance ( 1 );

                    var closing = $"]{equalSigns}]";
                    contents = this._reader.ReadStringUntil ( closing );

                    if ( this._reader.IsNext ( closing ) )
                    {
                        closingNotFound = false;
                        this._reader.Advance ( closing.Length );
                    }

                    return true;
                }

                this._reader.Restore ( start );
            }

            contents = null;
            return false;
        }

        public void Restore ( SourceLocation location ) => this._reader.Restore ( location );
        public void Restore ( Int32 position ) => this._reader.Restore ( position );

        public SourceLocation GetLocation ( ) => this._reader.GetLocation ( );
        public SourceLocation GetLocation ( Int32 position ) => this._reader.GetLocation ( position );
        public SourceRange GetLocation ( Range<Int32> range ) => this._reader.GetLocation ( range );
    }
}
