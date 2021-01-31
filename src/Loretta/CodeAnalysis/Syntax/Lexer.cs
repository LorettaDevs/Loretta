using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GParse.IO;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;
using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    internal sealed partial class Lexer
    {
        private readonly LuaOptions _luaOptions;
        private readonly SourceText _text;
        private readonly ICodeReader _reader;

        private Int32 _start;
        private readonly ImmutableArray<SyntaxTrivia>.Builder _triviaBuilder = ImmutableArray.CreateBuilder<SyntaxTrivia> ( );

        public DiagnosticBag Diagnostics { get; }

        public Int32 Length => this._reader.Length;

        public Int32 Position => this._reader.Position;

        public Lexer ( SyntaxTree syntaxTree )
        {
            this._luaOptions = syntaxTree.Options;
            this._text = syntaxTree.Text;
            this._reader = this._text.GetReader ( );
            this.Diagnostics = new DiagnosticBag ( );
        }

        public SyntaxToken Lex ( )
        {
            ImmutableArray<SyntaxTrivia> leadingTrivia = this.ReadTrivia ( leading: true );
            var tokenStart = this._reader.Position;

            (SyntaxKind tokenKind, Option<Object?> tokenValue) = this.ReadToken ( );

            var tokenLength = this._reader.Position - tokenStart;

            ImmutableArray<SyntaxTrivia> trailingTrivia = this.ReadTrivia ( leading: false );

            ReadOnlyMemory<Char> tokenText = SyntaxFacts.GetText ( tokenKind ) is String txt
                                             ? txt.AsMemory ( )
                                             : this._text.AsMemory ( tokenStart, tokenLength );

            return new SyntaxToken (
                tokenKind,
                tokenStart,
                tokenText,
                tokenValue,
                leadingTrivia,
                trailingTrivia );
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
                                    var span = TextSpan.FromBounds ( this._start, this._reader.Position );
                                    var location = new TextLocation ( this._text, span );
                                    this.Diagnostics.ReportUnfinishedLongComment ( location );
                                }

                                submitTrivia ( SyntaxKind.MultiLineCommentTrivia );
                            }
                            else
                            {
                                while ( this._reader.Peek ( ) is not ( null or '\n' or '\r' ) )
                                    this._reader.Advance ( 1 );
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
                            while ( this._reader.Peek ( ) is not ( null or '\n' or '\r' ) )
                                this._reader.Advance ( 1 );

                            if ( !this._luaOptions.AcceptCCommentSyntax )
                            {
                                var span = TextSpan.FromBounds ( this._start, this._reader.Position );
                                var location = new TextLocation ( this._text, span );
                                this.Diagnostics.ReportCCommentsNotSupportedInVersion ( location );
                            }

                            submitTrivia ( SyntaxKind.SingleLineCommentTrivia );
                        }
                        else if ( this._reader.IsAt ( '*', 1 ) )
                        {
                            this._reader.Advance ( 2 );
                            _ = this._reader.ReadSpanUntil ( "*/" );

                            if ( !this._reader.IsNext ( "*/" ) )
                            {
                                var span = TextSpan.FromBounds ( this._start, this._reader.Position );
                                var location = new TextLocation ( this._text, span );
                                this.Diagnostics.ReportUnfinishedLongComment ( location );
                            }
                            else
                            {
                                this._reader.Advance ( 2 );
                            }

                            if ( !this._luaOptions.AcceptCCommentSyntax )
                            {
                                var span = TextSpan.FromBounds ( this._start, this._reader.Position );
                                var location = new TextLocation ( this._text, span );
                                this.Diagnostics.ReportCCommentsNotSupportedInVersion ( location );
                            }

                            submitTrivia ( SyntaxKind.MultiLineCommentTrivia );
                        }
                        else
                        {
                            goto end;
                        }
                        break;

                    case '\r':
                        this._reader.Advance ( 1 );
                        if ( this._reader.Peek ( ) == '\n' )
                            this._reader.Advance ( 1 );
                        submitTrivia ( SyntaxKind.LineBreakTrivia );
                        if ( !leading )
                            goto end;
                        break;

                    case '\n':
                        this._reader.Advance ( 1 );
                        submitTrivia ( SyntaxKind.LineBreakTrivia );
                        if ( !leading )
                            goto end;
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
                                var span = TextSpan.FromBounds ( this._start, this._reader.Position );
                                var location = new TextLocation ( this._text, span );
                                this.Diagnostics.ReportShebangNotSupportedInVersion ( location );
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
                ReadOnlyMemory<Char> text = this._text.AsMemory ( this._start, length );
                this._triviaBuilder.Add ( new SyntaxTrivia ( kind, this._start, text ) );
            }
        }

        public (SyntaxKind kind, Option<Object?> tokenValue) ReadToken ( )
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
                            return (SyntaxKind.DotDotDotToken, Option.None<Object?> ( ));
                        }
                        // \.\.=
                        else if ( this._reader.IsAt ( '=', 2 ) )
                        {
                            this._reader.Advance ( 3 );
                            return (SyntaxKind.DotDotEqualsToken, Option.None<Object?> ( ));
                        }
                        // \.\.
                        else
                        {
                            this._reader.Advance ( 2 );
                            return (SyntaxKind.DotDotToken, Option.None<Object?> ( ));
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
                        return (SyntaxKind.DotToken, Option.None<Object?> ( ));
                    }
                }

                case ';':
                    this._reader.Advance ( 1 );
                    return (SyntaxKind.SemicolonToken, Option.None<Object?> ( ));

                case ',':
                    this._reader.Advance ( 1 );
                    return (SyntaxKind.CommaToken, Option.None<Object?> ( ));

                case ':':
                    if ( this._reader.IsAt ( ':', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.ColonColonToken, Option.None<Object?> ( ));
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.ColonToken, Option.None<Object?> ( ));
                    }

                #endregion Punctuation

                case '(':
                    this._reader.Advance ( 1 );
                    return (SyntaxKind.OpenParenthesisToken, Option.None<Object?> ( ));

                case ')':
                    this._reader.Advance ( 1 );
                    return (SyntaxKind.CloseParenthesisToken, Option.None<Object?> ( ));

                case '[':
                {
                    if ( this.TryReadLongString ( out var contents, out var closingNotFound ) )
                    {
                        if ( closingNotFound )
                        {
                            var span = TextSpan.FromBounds ( this._start, this._reader.Position );
                            var location = new TextLocation ( this._text, span );
                            this.Diagnostics.ReportUnfinishedString ( location );
                        }

                        return (SyntaxKind.LongStringToken, contents);
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.OpenBracketToken, Option.None<Object?> ( ));
                    }
                }


                case ']':
                    this._reader.Advance ( 1 );
                    return (SyntaxKind.CloseBracketToken, Option.None<Object?> ( ));

                case '{':
                    this._reader.Advance ( 1 );
                    return (SyntaxKind.OpenBraceToken, Option.None<Object?> ( ));

                case '}':
                    this._reader.Advance ( 1 );
                    return (SyntaxKind.CloseBraceToken, Option.None<Object?> ( ));

                #region Operators

                case '+':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.PlusEqualsToken, Option.None<Object?> ( ));
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.PlusToken, Option.None<Object?> ( ));
                    }

                case '-':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.MinusEqualsToken, Option.None<Object?> ( ));
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.MinusToken, Option.None<Object?> ( ));
                    }

                case '*':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.StartEqualsToken, Option.None<Object?> ( ));
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.StarToken, Option.None<Object?> ( ));
                    }

                case '/':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.SlashEqualsToken, Option.None<Object?> ( ));
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.SlashToken, Option.None<Object?> ( ));
                    }

                case '^':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.HatEqualsToken, Option.None<Object?> ( ));
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.HatToken, Option.None<Object?> ( ));
                    }

                case '%':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.PercentEqualsToken, Option.None<Object?> ( ));
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.PercentToken, Option.None<Object?> ( ));
                    }

                case '=':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.EqualsEqualsToken, Option.None<Object?> ( ));
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.EqualsToken, Option.None<Object?> ( ));
                    }

                case '#':
                    this._reader.Advance ( 1 );
                    return (SyntaxKind.HashToken, Option.None<Object?> ( ));

                case '~':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.TildeEqualsToken, Option.None<Object?> ( ));
                    }
                    break;

                case '>':
                    switch ( this._reader.Peek ( 1 ) )
                    {
                        case '=':
                            this._reader.Advance ( 2 );
                            return (SyntaxKind.GreaterThanEqualsToken, Option.None<Object?> ( ));

                        case '>':
                            this._reader.Advance ( 2 );
                            return (SyntaxKind.GreaterThanGreaterThanToken, Option.None<Object?> ( ));

                        default:
                            this._reader.Advance ( 1 );
                            return (SyntaxKind.GreaterThanToken, Option.None<Object?> ( ));
                    }

                case '<':
                    switch ( this._reader.Peek ( 1 ) )
                    {
                        case '=':
                            this._reader.Advance ( 2 );
                            return (SyntaxKind.LessThanEqualsToken, Option.None<Object?> ( ));

                        case '<':
                            this._reader.Advance ( 2 );
                            return (SyntaxKind.LessThanLessThanToken, Option.None<Object?> ( ));

                        default:
                            this._reader.Advance ( 1 );
                            return (SyntaxKind.LessThanToken, Option.None<Object?> ( ));
                    }

                case '&':
                    if ( this._reader.IsAt ( '&', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.AmpersandAmpersandToken, Option.None<Object?> ( ));
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.AmpersandToken, Option.None<Object?> ( ));
                    }

                case '|':
                    if ( this._reader.IsAt ( '|', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.PipePipeToken, Option.None<Object?> ( ));
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.PipeToken, Option.None<Object?> ( ));
                    }

                case '!':
                    if ( this._reader.IsAt ( '=', 1 ) )
                    {
                        this._reader.Advance ( 2 );
                        return (SyntaxKind.BangEqualsToken, Option.None<Object?> ( ));
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        return (SyntaxKind.BangToken, Option.None<Object?> ( ));
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
                    // We've already checked that the first letter is a valid identifier char. No need to re-check.
                    this._reader.Advance ( 1 );
                    while ( LoCharUtils.IsValidTrailingIdentifierChar ( this._reader.Peek ( ).GetValueOrDefault ( ) ) )
                        this._reader.Advance ( 1 );

                    var text = this._text.ToString ( this._start, this._reader.Position - this._start );
                    if ( !this._luaOptions.UseLuaJitIdentifierRules && text.Any ( ch => ch >= 0x7F ) )
                    {
                        var span = new TextSpan ( this._start, text.Length );
                        var location = new TextLocation ( this._text, span );
                        this.Diagnostics.ReportLuajitIdentifierRulesNotSupportedInVersion ( location );
                    }
                    SyntaxKind kind = SyntaxFacts.GetKeywordKind ( text );
                    return (kind, default);
                }

                #endregion Identifiers

                default:
                {
                    if ( LoCharUtils.IsValidFirstIdentifierChar ( peek0 ) )
                    {
                        goto case 'a';
                    }
                    else
                    {
                        this._reader.Advance ( 1 );
                        var span = new TextSpan ( this._start, 1 );
                        var location = new TextLocation ( this._text, span );
                        this.Diagnostics.ReportBadCharacter ( location, peek0 );
                        return (SyntaxKind.BadToken, Option.None<Object?> ( ));
                    }
                }
            } // end switch

            throw new Exception ( "Unreacheable." );
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
    }
}
