using System;
using System.Diagnostics;
using System.Text;
using GParse.Utilities;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Syntax
{
    internal sealed partial class Lexer
    {
        private String ParseShortString ( )
        {
            var strStart = this._reader.Position;
            var parsed = new StringBuilder ( );
            var delim = this._reader.Read ( )!.Value;
            Debug.Assert ( delim is '"' or '\'' );

            while ( this._reader.Peek ( ) is Char peek && peek != delim )
            {
                var charStart = this._reader.Position;
                switch ( peek )
                {
                    #region Escapes

                    case '\\':
                    {
                        var escapeStart = this._reader.Position;
                        this._reader.Advance ( 1 );

                        switch ( this._reader.Peek ( ) )
                        {
                            case '\r':
                                if ( this._reader.IsAt ( '\n', 1 ) )
                                {
                                    this._reader.Advance ( 2 );
                                    parsed.Append ( "\r\n" );
                                }
                                else
                                {
                                    this._reader.Advance ( 1 );
                                    parsed.Append ( '\r' );
                                }
                                break;

                            case 'a':
                                this._reader.Advance ( 1 );
                                parsed.Append ( '\a' );
                                break;

                            case 'b':
                                this._reader.Advance ( 1 );
                                parsed.Append ( '\b' );
                                break;

                            case 'f':
                                this._reader.Advance ( 1 );
                                parsed.Append ( '\f' );
                                break;

                            case 'n':
                                this._reader.Advance ( 1 );
                                parsed.Append ( '\n' );
                                break;

                            case 'r':
                                this._reader.Advance ( 1 );
                                parsed.Append ( '\r' );
                                break;

                            case 't':
                                this._reader.Advance ( 1 );
                                parsed.Append ( '\t' );
                                break;

                            case 'v':
                                this._reader.Advance ( 1 );
                                parsed.Append ( '\v' );
                                break;

                            case '\\':
                                this._reader.Advance ( 1 );
                                parsed.Append ( '\\' );
                                break;

                            case '\n':
                                this._reader.Advance ( 1 );
                                parsed.Append ( '\n' );
                                break;

                            case '\'':
                                this._reader.Advance ( 1 );
                                parsed.Append ( '\'' );
                                break;

                            case '"':
                                this._reader.Advance ( 1 );
                                parsed.Append ( '"' );
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
                                parsed.Append ( parseDecimalInteger ( escapeStart ) );
                                break;

                            case 'x':
                                parsed.Append ( parseHexadecimalInteger ( escapeStart ) );
                                break;

                            default:
                            {
                                var span = TextSpan.FromBounds ( escapeStart, this._reader.Position );
                                var location = new TextLocation ( this._text, span );
                                this.Diagnostics.ReportInvalidStringEscape ( location );
                            }
                            break;
                        }
                    }
                    break;

                    #endregion Escapes

                    case '\r':
                    {
                        if ( this._reader.IsAt ( '\n', 1 ) )
                        {
                            this._reader.Advance ( 2 );
                            parsed.Append ( "\r\n" );
                        }
                        else
                        {
                            parsed.Append ( '\r' );
                        }

                        var span = TextSpan.FromBounds ( charStart, this._reader.Position );
                        var location = new TextLocation ( this._text, span );
                        this.Diagnostics.ReportUnescapedLineBreakInString ( location );
                    }
                    break;

                    case '\n':
                    {
                        this._reader.Advance ( 1 );
                        parsed.Append ( '\n' );

                        var span = TextSpan.FromBounds ( charStart, this._reader.Position );
                        var location = new TextLocation ( this._text, span );
                        this.Diagnostics.ReportUnescapedLineBreakInString ( location );
                    }
                    break;

                    default:
                        this._reader.Advance ( 1 );
                        parsed.Append ( peek );
                        break;
                }
            }

            if ( this._reader.IsNext ( delim ) )
            {
                this._reader.Advance ( 1 );
            }
            else
            {
                var span = TextSpan.FromBounds ( strStart, this._reader.Position );
                var location = new TextLocation ( this._text, span );
                this.Diagnostics.ReportUnfinishedString ( location );
            }

            return parsed.ToString ( );

            Char parseDecimalInteger ( Int32 start )
            {
                var readChars = 0;
                var num = 0;
                Char ch;
                while ( readChars < 3 && LoCharUtils.IsDecimal ( ch = this._reader.Peek ( ).GetValueOrDefault ( ) ) )
                {
                    this._reader.Advance ( 1 );
                    num = ( num * 10 ) + ( ch - '0' );
                    readChars++;
                }

                if ( readChars < 1 || num > 255 )
                {
                    var span = TextSpan.FromBounds ( start - 1, this._reader.Position );
                    var location = new TextLocation ( this._text, span );
                    this.Diagnostics.ReportInvalidStringEscape ( location );
                }

                return ( Char ) num;
            }

            Char parseHexadecimalInteger ( Int32 start )
            {
                var readChars = 0;
                var num = ( Byte ) 0;
                while ( readChars < 2 )
                {
                    var peek = this._reader.Peek ( ).GetValueOrDefault ( );
                    if ( LoCharUtils.IsDecimal ( peek ) )
                    {
                        this._reader.Advance ( 1 );
                        num = ( Byte ) ( ( num << 4 ) & ( peek - '0' ) );
                    }
                    else if ( LoCharUtils.IsHexadecimal ( peek ) )
                    {
                        this._reader.Advance ( 1 );
                        num = ( Byte ) ( ( num << 4 ) & ( 10 + CharUtils.AsciiLowerCase ( peek ) - 'a' ) );
                    }
                    else
                    {
                        break;
                    }
                    readChars++;
                }

                if ( readChars < 1 )
                {
                    var span = TextSpan.FromBounds ( start - 2, this._reader.Position );
                    var location = new TextLocation ( this._text, span );
                    this.Diagnostics.ReportInvalidStringEscape ( location );
                }

                return ( Char ) num;
            }
        }
    }
}
