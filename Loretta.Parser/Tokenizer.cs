using Loretta.Common;
using Loretta.Common.Tokens;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Loretta.Parser
{
    public partial class Tokenizer
    {
        private readonly String Code;
        private readonly StringReader Reader;
        private Token LastToken;

        public Tokenizer ( String Code )
        {
            this.Code = Code;
            this.Reader = new StringReader ( Code );
        }

        public static IEnumerable<Token> Tokenize ( String code )
        {
            return new Tokenizer ( code )
                .Tokenize ( );
        }

        public IEnumerable<Token> Tokenize ( )
        {
            var ret = new List<Token> ( );
            Char next;

            while ( ( next = this.Reader.Next ( ) ) != '\0' )
            {
                if ( LJUtils.IsAlpha ( next ) )
                    ret.Add ( this.LastToken = ReadIdentifier ( ) );
                else if ( Char.IsDigit ( next ) || ( next == '.' && Char.IsDigit ( this.Reader.Next ( 2 ) ) ) )
                    ret.Add ( this.LastToken = ReadNumber ( ) );
                else if ( Char.IsWhiteSpace ( next ) )
                    this.LastToken?.AddWhitespaceAfter ( this.Reader.ReadUntilNot ( Char.IsWhiteSpace ) );
                else if ( next == '-' && this.Reader.Next ( 2 ) == '-' )
                    ret.Add ( this.LastToken = ReadComment ( ) );
                else
                {
                    switch ( next )
                    {
                        case '(':
                            ret.Add ( this.LastToken = SingleCharToken ( TokenType.LParen ) );
                            break;

                        case ')':
                            ret.Add ( this.LastToken = SingleCharToken ( TokenType.RParen ) );
                            break;

                        case '[':
                            var n2 = this.Reader.Next ( 2 );
                            ret.Add ( this.LastToken = ( n2 == '=' || n2 == '[' )
                                ? ReadLongString ( $"{this.Reader.Read ( )}{this.Reader.ReadUntil ( '[' )}{this.Reader.Read ( )}" )
                                : SingleCharToken ( TokenType.LBracket ) );
                            break;

                        case ']':
                            ret.Add ( this.LastToken = SingleCharToken ( TokenType.RBracket ) );
                            break;

                        case '{':
                            ret.Add ( this.LastToken = SingleCharToken ( TokenType.LCurly ) );
                            break;

                        case '}':
                            ret.Add ( this.LastToken = SingleCharToken ( TokenType.RCurly ) );
                            break;

                        case ':':
                            ret.Add ( this.LastToken = SingleCharToken ( TokenType.Colon ) );
                            break;

                        case '~':
                            if ( this.Reader.Next ( 2 ) == '=' )
                            {
                                ret.Add ( this.LastToken = new Token ( this.Reader.ReadString ( 2 ) )
                                {
                                    Type = TokenType.BinaryOp
                                } );
                                break;
                            }
                            else goto default;

                        case '=':
                            ret.Add ( this.LastToken = this.Reader.Read ( 2 ) == '='
                                ? new Token ( this.Reader.ReadString ( 2 ) ) { Type = TokenType.BinaryOp }
                                : SingleCharToken ( TokenType.Equal ) );
                            break;

                        case ',':
                            ret.Add ( this.LastToken = SingleCharToken ( TokenType.Comma ) );
                            break;

                        case ';':
                            ret.Add ( this.LastToken = SingleCharToken ( TokenType.Semicolon ) );
                            break;

                        case '!':
                            ret.Add ( this.LastToken = SingleCharToken ( TokenType.UnaryOp ) );
                            break;

                        case '+':
                        case '-':
                            ret.Add ( this.LastToken = SingleCharToken ( this.LastToken.IsPossibleValue ( ) ? TokenType.BinaryOp : TokenType.UnaryOp ) );
                            break;

                        case '^':
                        case '*':
                        case '/':
                            ret.Add ( this.LastToken = SingleCharToken ( TokenType.BinaryOp ) );
                            break;

                        case '.':
                            ret.Add ( this.LastToken = this.Reader.Next ( 2 ) == '.'
                                // Concat
                                ? new Token ( this.Reader.ReadString ( 2 ) ) { Type = TokenType.BinaryOp }
                                // table accessor
                                : SingleCharToken ( TokenType.Period ) );
                            break;

                        case '\'':
                        case '"':
                            ret.Add ( this.LastToken = ReadSimpleString ( this.Reader.Read ( ) ) );
                            break;

                        default:
                            throw new Exception ( $"Unexpected \"{next}\" near \"{this.LastToken.Raw}\"" );
                    }
                }
            }
            return ret;
        }

        private Token SingleCharToken ( TokenType typ )
        {
            return new Token ( this.Reader.ReadString ( 1 ) )
            {
                Type = typ
            };
        }

        #region Actually Reading Stuff

        public Token ReadIdentifier ( )
        {
            var raw = this.Reader.ReadUntilNot ( x => LJUtils.IsIdent( x ) || x == '_' );

            var tok = new SyntaxToken ( raw );
            switch ( raw )
            {
                case "not":
                    tok.Type = TokenType.UnaryOp;
                    return tok;

                case "nil":
                    tok.Type = TokenType.Nil;
                    return tok;

                case "and":
                case "or":
                    tok.Type = TokenType.BinaryOp;
                    return tok;

                case "do":
                    tok.Type = TokenType.Do;
                    return tok;

                case "if":
                    tok.Type = TokenType.If;
                    return tok;

                case "then":
                    tok.Type = TokenType.Then;
                    return tok;

                case "else":
                    tok.Type = TokenType.Else;
                    return tok;

                case "elseif":
                    tok.Type = TokenType.ElseIf;
                    return tok;

                case "end":
                    tok.Type = TokenType.End;
                    return tok;

                case "in":
                    tok.Type = TokenType.In;
                    return tok;

                case "for":
                    tok.Type = TokenType.For;
                    return tok;

                case "while":
                    tok.Type = TokenType.While;
                    return tok;

                case "local":
                    tok.Type = TokenType.Local;
                    return tok;

                case "break":
                    tok.Type = TokenType.Break;
                    return tok;

                case "until":
                    tok.Type = TokenType.Until;
                    return tok;

                case "repeat":
                    tok.Type = TokenType.Repeat;
                    return tok;

                case "return":
                    tok.Type = TokenType.Return;
                    return tok;

                case "function":
                    tok.Type = TokenType.Function;
                    return tok;

                default:
                    tok.Type = TokenType.Identifier;
                    return tok;
            }
        }

        public Token ReadSimpleString ( Char quoteStart )
        {
            var raw = this.Reader.ReadUntil ( ( n1, n2 ) => n1 == '\0' || ( n1 != '\\' && n2 == quoteStart ) ) + this.Reader.Read ( );
            this.Reader.Read ( );
            var str = Regex.Replace (
                raw,
                @"\\(\d{1,3})",
                ( Match e ) => ( ( Char ) Int32.Parse ( e.Groups[1].Value ) ).ToString ( )
            );

            return new StringToken ( str, $"{quoteStart}{raw}{quoteStart}" )
            {
                Type = TokenType.String
            };
        }

        private Token ReadNumber ( )
        {
            var num = new StringBuilder ( );
            var pastdot = false;
            var pastE = false;
            var next1 = this.Reader.Next ( 1 );

            // Handle hex numbers
            if ( this.Reader.Next ( 1 ) == '0' && this.Reader.Next ( 2 ) == 'x' )
            {
                this.Reader.Read ( 2 );
                num.Append ( this.Reader.ReadUntilNot ( c => ( c >= '0' && c <= '9' ) || ( c >= 'a' && c <= 'f' ) || ( c >= 'A' && c <= 'F' ) ) );

                try
                {
                    return new NumberToken ( $"0x{num}",
                    Double.Parse ( num.ToString ( ), System.Globalization.NumberStyles.HexNumber ) );
                }
                catch ( Exception )
                {
                    throw new Exception ( $"Malformed number: {num} near {this.LastToken}" );
                }
            }

            while ( ( next1 = this.Reader.Next ( 1 ) ) != '\0' )
            {
                var next2 = this.Reader.Next ( 2 );

                if ( Char.IsDigit ( next1 ) )
                {
                    num.Append ( this.Reader.Read ( ) );
                }
                else if ( next1 == '.' && !pastdot )
                {
                    num.Append ( this.Reader.Read ( ) );
                    pastdot = true;
                }
                else if ( ( next1 == 'e' || next1 == 'E' ) && ( Char.IsDigit ( next2 ) || next2 == '+' || next2 == '-' ) )
                {
                    if ( pastE )
                        throw new Exception ( $"Unexpected \"{next1}\" near \"{num}\"" );

                    pastE = true;
                    num.Append ( this.Reader.Read ( ) );
                    num.Append ( this.Reader.Read ( ) );
                }
                else
                    break;
            }

            try
            {
                return new NumberToken ( num.ToString ( ), Double.Parse ( num.ToString ( ) ) );
            }
            catch ( Exception )
            {
                throw new Exception ( $"Malformed number: \"{num.ToString ( )}\" near \"{this.LastToken.Raw}\"" );
            }
        }

        /// <summary>
        /// Reads a long string
        /// </summary>
        /// <param name="start">The long string start</param>
        /// <returns></returns>
        public StringToken ReadLongString ( String start )
        {
            var end = start.Replace ( '[', ']' );
            var content =  this.Reader.ReadUntil ( end );
            this.Reader.Read ( end.Length );
            return new StringToken ( content, $"{start}{content}{end}" );
        }

        /// <summary>
        /// Reads a Lua Comment
        /// </summary>
        /// <returns></returns>
        public CommentToken ReadComment ( )
        {
            String content;
            this.Reader.Read ( 2 );
            if ( this.Reader.Next ( ) == '[' && this.Reader.Next ( 2 ) == '[' )
            {
                content = this.Reader.ReadUntil ( "]]" );
                this.Reader.Read ( 2 );
                var ret = new CommentToken ( content, $"--[[{content}]]" );
                return ret;
            }
            else if ( this.Reader.Next ( ) == '[' && this.Reader.Next ( 2 ) == '=' )
            {
                StringToken tok = ReadLongString ( $"{this.Reader.Read ( )}{this.Reader.ReadUntil ( '[' )}" );
                return new CommentToken ( tok.Value, $"--{tok.Raw}" );
            }

            content = this.Reader.ReadUntil ( x => x == '\r' || x == '\n' );
            return new CommentToken ( content, $"--{content}" );
        }

        #endregion Actually Reading Stuff
    }
}
