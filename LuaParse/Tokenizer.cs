using LuaParse.Tokens;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace LuaParse
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

            while ( ( next = Reader.Next ( ) ) != '\0' )
            {
                if ( CLuaPort.IsAlpha ( next ) )
                    ret.Add ( LastToken = ReadIdentifier ( ) );
                else if ( Char.IsDigit ( next ) || ( next == '.' && Char.IsDigit ( Reader.Next ( 2 ) ) ) )
                    ret.Add ( LastToken = ReadNumber ( ) );
                else if ( Char.IsWhiteSpace ( next ) )
                    LastToken?.AddWhitespaceAfter ( Reader.ReadUntilNot ( Char.IsWhiteSpace ) );
                else if ( next == '-' && Reader.Next ( 2 ) == '-' )
                    ret.Add ( LastToken = ReadComment ( ) );
                else
                {
                    switch ( next )
                    {
                        case '(':
                            ret.Add ( LastToken = SingleCharToken ( TokenType.LParen ) );
                            break;

                        case ')':
                            ret.Add ( LastToken = SingleCharToken ( TokenType.RParen ) );
                            break;

                        case '[':
                            var n2 = Reader.Next ( 2 );
                            ret.Add ( LastToken = ( n2 == '=' || n2 == '[' )
                                ? ReadLongString ( $"{Reader.Read ( )}{Reader.ReadUntil ( '[' )}{Reader.Read ( )}" )
                                : SingleCharToken ( TokenType.LBracket ) );
                            break;

                        case ']':
                            ret.Add ( LastToken = SingleCharToken ( TokenType.RBracket ) );
                            break;

                        case '{':
                            ret.Add ( LastToken = SingleCharToken ( TokenType.LCurly ) );
                            break;

                        case '}':
                            ret.Add ( LastToken = SingleCharToken ( TokenType.RCurly ) );
                            break;

                        case ':':
                            ret.Add ( LastToken = SingleCharToken ( TokenType.Colon ) );
                            break;

                        case '~':
                            if ( Reader.Next ( 2 ) == '=' )
                            {
                                ret.Add ( LastToken = new Token ( Reader.ReadString ( 2 ) )
                                {
                                    Type = TokenType.BinaryOp
                                } );
                                break;
                            }
                            else goto default;

                        case '=':
                            ret.Add ( LastToken = Reader.Read ( 2 ) == '='
                                ? new Token ( Reader.ReadString ( 2 ) ) { Type = TokenType.BinaryOp }
                                : SingleCharToken ( TokenType.Equal ) );
                            break;

                        case ',':
                            ret.Add ( LastToken = SingleCharToken ( TokenType.Comma ) );
                            break;

                        case ';':
                            ret.Add ( LastToken = SingleCharToken ( TokenType.Semicolon ) );
                            break;

                        case '!':
                            ret.Add ( LastToken = SingleCharToken ( TokenType.UnaryOp ) );
                            break;

                        case '+':
                        case '-':
                            ret.Add ( LastToken = SingleCharToken ( LastToken.IsPossibleValue ( ) ? TokenType.BinaryOp : TokenType.UnaryOp ) );
                            break;

                        case '^':
                        case '*':
                        case '/':
                            ret.Add ( LastToken = SingleCharToken ( TokenType.BinaryOp ) );
                            break;

                        case '.':
                            ret.Add ( LastToken = Reader.Next ( 2 ) == '.'
                                // Concat
                                ? new Token ( Reader.ReadString ( 2 ) ) { Type = TokenType.BinaryOp }
                                // table accessor
                                : SingleCharToken ( TokenType.Period ) );
                            break;

                        case '\'':
                        case '"':
                            ret.Add ( LastToken = ReadSimpleString ( Reader.Read ( ) ) );
                            break;

                        default:
                            throw new Exception ( $"Unexpected \"{next}\" near \"{LastToken.Raw}\"\"{LastToken.WhitespaceAfter}\"" );
                    }
                }
            }
            return ret;
        }

        private Token SingleCharToken ( TokenType typ )
        {
            return new Token ( Reader.ReadString ( 1 ) )
            {
                Type = typ
            };
        }

        #region Actually Reading Stuff

        public Token ReadIdentifier ( )
        {
            var raw = Reader.ReadUntilNot ( x => CLuaPort.IsAlNum ( x ) || x == '_' );

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
            var raw = Reader.ReadUntil ( ( n1, n2 ) => n1 == '\0' || ( n1 != '\\' && n2 == quoteStart ) ) + Reader.Read ( );
            Reader.Read ( );
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
            var next1 = Reader.Next ( 1 );

            // Handle hex numbers
            if ( Reader.Next ( 1 ) == '0' && Reader.Next ( 2 ) == 'x' )
            {
                Reader.Read ( 2 );
                num.Append ( Reader.ReadUntilNot ( c => ( c >= '0' && c <= '9' ) || ( c >= 'a' && c <= 'f' ) || ( c >= 'A' && c <= 'F' ) ) );

                try
                {
                    return new NumberToken ( $"0x{num}",
                    Double.Parse ( num.ToString ( ), System.Globalization.NumberStyles.HexNumber ) );
                }
                catch ( Exception )
                {
                    throw new Exception ( $"Malformed number: {num} near {LastToken}" );
                }
            }

            while ( ( next1 = Reader.Next ( 1 ) ) != '\0' )
            {
                var next2 = Reader.Next ( 2 );

                if ( Char.IsDigit ( next1 ) )
                {
                    num.Append ( Reader.Read ( ) );
                }
                else if ( next1 == '.' && !pastdot )
                {
                    num.Append ( Reader.Read ( ) );
                    pastdot = true;
                }
                else if ( ( next1 == 'e' || next1 == 'E' ) && ( Char.IsDigit ( next2 ) || next2 == '+' || next2 == '-' ) )
                {
                    if ( pastE )
                        throw new Exception ( $"Unexpected \"{next1}\" near \"{num}\"" );

                    pastE = true;
                    num.Append ( Reader.Read ( ) );
                    num.Append ( Reader.Read ( ) );
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
                throw new Exception ( $"Malformed number: \"{num.ToString ( )}\" near \"{LastToken.Raw}\"" );
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
            var content =  Reader.ReadUntil ( end );
            Reader.Read ( end.Length );
            return new StringToken ( content, $"{start}{content}{end}" );
        }

        /// <summary>
        /// Reads a Lua Comment
        /// </summary>
        /// <returns></returns>
        public CommentToken ReadComment ( )
        {
            String content;
            Reader.Read ( 2 );
            if ( Reader.Next ( ) == '[' && Reader.Next ( 2 ) == '[' )
            {
                content = Reader.ReadUntil ( "]]" );
                Reader.Read ( 2 );
                var ret = new CommentToken ( content, $"--[[{content}]]" );
                return ret;
            }
            else if ( Reader.Next ( ) == '[' && Reader.Next ( 2 ) == '=' )
            {
                var tok = ReadLongString ( $"{Reader.Read ( )}{Reader.ReadUntil ( '[' )}" );
                return new CommentToken ( tok.Value, $"--{tok.Raw}" );
            }

            content = Reader.ReadUntil ( x => x == '\r' || x == '\n' );
            return new CommentToken ( content, $"--{content}" );
        }

        #endregion Actually Reading Stuff
    }
}
