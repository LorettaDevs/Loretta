using System;
using System.Text;
using GParse.Lexing;
using GParse.Lexing.Errors;
using GParse.Lexing.Settings;
using Loretta.Common;

namespace Loretta.Parser
{
    public class GLuaLexer : LexerBase
    {
        /// <summary>
        /// </summary>
        /// <param name="input">
        /// If this is not in ASCII non-helpful exceptions will be thrown.
        /// </param>
        /// <param name="isGlua"></param>
        protected GLuaLexer ( String input ) : base ( CInterop.ConvertStringToASCII ( input ) )
        {
            this.consumeNewlinesAutomatically = true;

            // This won't be really used tbh since the base lexer
            // class isn't ready to handle floating point numbers
            // just yet
            this.numberSettings = new IntegerLexSettings
            {
                DefaultType = IntegerLexSettings.NumberType.Decimal,
                HexadecimalPrefix = "0x",
            };

            // String settings. Not many pitfalls other than \
            // making a string multiline
            this.stringSettings = new StringLexSettings
            {
                CharSettings = new CharLexSettings
                {
                    // \000 -> \255
                    DecimalEscapePrefix = "\\",
                    DecimalEscapeMaxLengh = 3,

                    // \x00 -> \xFF
                    HexadecimalEscapePrefix = "\\x",
                    HexadecimalEscapeMaxLengh = 2
                },

                NewlineEscape = "\\"
            };

            this.stringSettings
                .CharSettings
                .RegisterEscapeConstant ( @"\0", '\0' )
                .RegisterEscapeConstant ( @"\a", '\a' )
                .RegisterEscapeConstant ( @"\b", '\b' )
                .RegisterEscapeConstant ( @"\f", '\f' )
                .RegisterEscapeConstant ( @"\n", '\n' )
                .RegisterEscapeConstant ( @"\r", '\r' )
                .RegisterEscapeConstant ( @"\t", '\t' )
                .RegisterEscapeConstant ( @"\v", '\v' )
                .RegisterEscapeConstant ( @"\'", '\'' )
                .RegisterEscapeConstant ( "\\\"", '"' )
                .RegisterEscapeConstant ( @"\\", '\\' );

            var notalnum = new Func<Char, Boolean> ( ch => !LJUtils.IsAlNum ( ch ) );

            #region Binary Operators

            this.tokenManager
                .AddToken ( "and", "and", TokenType.Operator, notalnum )
                .AddToken ( "or", "or", TokenType.Operator, notalnum )
                .AddToken ( "+", "+", TokenType.Operator )
                .AddToken ( "-", "-", TokenType.Operator )
                .AddToken ( "*", "*", TokenType.Operator )
                .AddToken ( "/", "/", TokenType.Operator )
                .AddToken ( "^", "^", TokenType.Operator )
                .AddToken ( "%", "%", TokenType.Operator )
                .AddToken ( "..", "..", TokenType.Operator )
                .AddToken ( "<", "<", TokenType.Operator )
                .AddToken ( ">", ">", TokenType.Operator )
                .AddToken ( "<=", "<=", TokenType.Operator )
                .AddToken ( ">=", ">=", TokenType.Operator )
                .AddToken ( "==", "==", TokenType.Operator )
                .AddToken ( "~=", "~=", TokenType.Operator )
                .AddToken ( "and", "&&", TokenType.Operator )
                .AddToken ( "or", "||", TokenType.Operator )
                .AddToken ( "~=", "!=", TokenType.Operator );

            #endregion Binary Operators

            #region Unary Operators

            this.tokenManager
                .AddToken ( "not", "not", TokenType.Operator, notalnum )
                .AddToken ( "Len", "#", TokenType.Operator )
                .AddToken ( "not", "!", TokenType.Operator );

            #endregion Unary Operators

            #region Symbols

            this.tokenManager
                .AddToken ( ",", ",", TokenType.Punctuation )
                .AddToken ( ".", ".", TokenType.Punctuation, ch => !LJUtils.IsDigit ( ch ) )
                // Cannot be used because of goto labels
                //.AddToken ( ":", ":", TokenType.Punctuation )
                .AddToken ( ";", ";", TokenType.Punctuation )
                .AddToken ( "...", "...", TokenType.Identifier )
                .AddToken ( "(", "(", TokenType.LParen )
                .AddToken ( ")", ")", TokenType.RParen )
                // Cannot use these because of multiline strings
                //.AddToken ( "[", "[", TokenType.LBracket )
                .AddToken ( "]", "]", TokenType.RBracket )
                .AddToken ( "{", "{", TokenType.LCurly )
                .AddToken ( "}", "}", TokenType.RCurly );

            #endregion Symbols

            #region Keywords

            this.tokenManager
                .AddToken ( "do", "do", TokenType.Keyword, notalnum )
                .AddToken ( "end", "end", TokenType.Keyword, notalnum )
                .AddToken ( "while", "while", TokenType.Keyword, notalnum )
                .AddToken ( "function", "function", TokenType.Keyword, notalnum )
                .AddToken ( "nil", "nil", TokenType.Keyword, notalnum )
                .AddToken ( "true", "true", TokenType.Keyword, notalnum )
                .AddToken ( "false", "false", TokenType.Keyword, notalnum )
                .AddToken ( "return", "return", TokenType.Keyword, notalnum )
                .AddToken ( "break", "break", TokenType.Keyword, notalnum )
                .AddToken ( "local", "local", TokenType.Keyword, notalnum )
                .AddToken ( "for", "for", TokenType.Keyword, notalnum )
                .AddToken ( "in", "in", TokenType.Keyword, notalnum )
                .AddToken ( "if", "if", TokenType.Keyword, notalnum )
                .AddToken ( "then", "then", TokenType.Keyword, notalnum )
                .AddToken ( "elseif", "elseif", TokenType.Keyword, notalnum )
                .AddToken ( "else", "else", TokenType.Keyword, notalnum )
                .AddToken ( "repeat", "repeat", TokenType.Keyword, notalnum )
                .AddToken ( "until", "until", TokenType.Keyword, notalnum )
                .AddToken ( "continue", "continue", TokenType.Keyword, notalnum );

            #endregion Keywords
        }

        protected override Boolean CharIsDec ( Char ch ) => LJUtils.IsDigit ( ch );

        protected override Boolean CharIsHex ( Char ch ) => LJUtils.IsXDigit ( ch );

        protected override Boolean CharIsWhitepace ( Char ch ) => LJUtils.IsSpace ( ch );

        protected override Boolean TryReadToken ( out Token tok )
        {
            var peek = ( Char ) this.reader.Peek ( );
            SourceLocation start = this.reader.Location;

            // Token starts null and might be set by any of the following
            tok = null;

            // BOM (byte-based, since we don't work with unicode)
            if ( this.Location == SourceLocation.Zero && this.Consume ( "\xEF\xBB\xBF" ) )
            {
                tok = this.CreateToken ( this.whitespaceID, "\xEF\xBB\xBF", "\xEF\xBB\xBF", TokenType.Whitespace, start.To ( this.Location ) );
            }
            // Numbers
            else if ( ( peek == '.' && LJUtils.IsDigit ( ( Char ) this.reader.Peek ( 1 ) ) ) || LJUtils.IsDigit ( peek ) )
            {
                var (Raw, Value) = this.ReadLuaNumber ( );
                tok = this.CreateToken ( "number", Raw, Value, TokenType.Number, start.To ( this.reader.Location ) );
            }
            // Identifiers (keywords are read by the tokenmanager)
            else if ( LJUtils.IsAlpha ( peek ) || peek == '_' )
            {
                String ident = this.reader.ReadStringWhile ( LJUtils.IsIdent );
                tok = this.CreateToken ( "ident", ident, ident, TokenType.Identifier, start.To ( this.reader.Location ) );
            }
            // Normal strings
            else if ( peek == '\'' || peek == '"' )
            {
                // Lexer base string reader doesn't adds
                // delimiters to the raw
                var (Raw, Value) = this.ReadString ( peek.ToString ( ), false, this.stringSettings );
                Raw = $"{peek}{Raw}{peek}";
                tok = this.CreateToken ( "string", Raw, Value, TokenType.String, start.To ( this.Location ) );
            }
            // Stuff that has/is :
            else if ( this.Consume ( ':' ) )
            {
                // Goto labels
                if ( this.Consume ( ':' ) )
                {
                    if ( !LJUtils.IsAlpha ( ( Char ) this.reader.Peek ( ) ) )
                        throw new LexException ( "Invalid goto label.", start );

                    var name = this.reader.ReadStringWhile ( LJUtils.IsIdent );
                    this.Expect ( "::" );

                    tok = this.CreateToken ( "gotoLabel", $"::{name}::", name, TokenType.Identifier, start.To ( this.Location ) );
                }
                // Normal self-functions
                else
                {
                    tok = this.CreateToken ( ":", ":", ":", TokenType.Punctuation, start.To ( this.Location ) );
                }
            }
            // comments
            else if ( this.reader.IsNext ( "//" ) || this.reader.IsNext ( "--" ) || this.reader.IsNext ( "/*" ) )
            {
                var (Raw, Value) = this.ReadComment ( );
                tok = CreateToken ( "comment", Raw, Value, TokenType.Comment, start.To ( this.Location ) );
            }
            else if ( this.Consume ( '[' ) )
            {
                // Long string
                if ( this.reader.IsNext ( "=" ) || this.reader.IsNext ( "[" ) )
                {
                    var (Raw, Value) = this.ReadLiteralString ( );
                    tok = CreateToken ( "string", Raw, Value, TokenType.String, start.To ( this.Location ) );
                }
                // Normal symbol
                else
                {
                    tok = CreateToken ( "[", "[", "[", TokenType.Punctuation, start.To ( this.Location ) );
                }
            }

            return tok != null;
        }

        #region Lua Number Lexing

        public (String Raw, Double Value) ReadLuaNumber ( )
        {
            return this.reader.IsNext ( "0x" ) ? this.ReadLuaHex ( ) : this.ReadLuaDec ( );
        }

        private (String Raw, Double Value) ReadLuaDec ( )
        {
            var rawNumber = new StringBuilder ( );
            String integer = this.reader.ReadStringWhile ( LJUtils.IsDigit ), fractional = null, exp = null;
            rawNumber.Append ( integer );

            if ( this.reader.IsNext ( "." ) )
            {
                this.reader.Advance ( 1 );
                rawNumber.Append ( '.' );

                fractional = this.reader.ReadStringWhile ( LJUtils.IsDigit );
                rawNumber.Append ( fractional );
            }

            if ( this.reader.IsNext ( "e" ) || this.reader.IsNext ( "E" ) )
            {
                rawNumber.Append ( this.reader.ReadString ( 1 ) );
                if ( this.reader.IsNext ( "+" ) || this.reader.IsNext ( "-" ) )
                    rawNumber.Append ( this.reader.ReadString ( 1 ) );

                exp = this.reader.ReadStringWhile ( LJUtils.IsDigit );
                rawNumber.Append ( exp );

                if ( String.IsNullOrEmpty ( exp ) )
                    throw new LexException ( "Malformed number literal.", this.Location );
            }

            if ( String.IsNullOrEmpty ( integer ) && String.IsNullOrEmpty ( fractional ) )
                throw new LexException ( "Malformed number literal.", this.Location );

            return (
                Raw: rawNumber.ToString ( ),
                Value: CInterop.atof ( rawNumber.ToString ( ) )
            );
        }

        private (String Raw, Double Value) ReadLuaHex ( )
        {
            var rawNumber = new StringBuilder ( );
            this.reader.Advance ( 2 );
            rawNumber.Append ( "0x" );

            String integer = this.reader.ReadStringWhile ( LJUtils.IsXDigit ), fractional = null, shift = null;
            rawNumber.Append ( integer );

            if ( this.reader.IsNext ( "." ) )
            {
                this.reader.Advance ( 1 );
                rawNumber.Append ( '.' );

                fractional = this.reader.ReadStringWhile ( LJUtils.IsXDigit );
                rawNumber.Append ( fractional );
            }

            if ( this.reader.IsNext ( "p" ) || this.reader.IsNext ( "P" ) )
            {
                rawNumber.Append ( this.reader.ReadString ( 1 ) );

                if ( this.reader.IsNext ( "+" ) || this.reader.IsNext ( "-" ) )
                    rawNumber.Append ( this.reader.ReadString ( 1 ) );

                shift = this.reader.ReadStringWhile ( LJUtils.IsDigit );
                rawNumber.Append ( shift );

                if ( String.IsNullOrEmpty ( shift ) )
                    throw new LexException ( "Malformed number literal (missing exponent).", this.Location );
            }

            if ( String.IsNullOrEmpty ( integer ) && String.IsNullOrEmpty ( fractional ) )
                throw new LexException ( "Malformed number literal (missing both integral part and fractional part).", this.Location );

            return (
                Raw: rawNumber.ToString ( ),
                Value: CInterop.atof ( rawNumber.ToString ( ) )
            );
        }

        #endregion Lua Number Lexing

        #region Lua Comment Lexing

        public (String Raw, String Value) ReadComment ( )
        {
            var raw = new StringBuilder ( );
            var value = new StringBuilder ( );

            if ( this.Consume ( "--" ) )
            {
                raw.Append ( "--" );

                // Multiline comment -_-
                if ( this.Consume ( '[' ) )
                {
                    // read '[', {'='}, '['
                    String eqs = this.reader.ReadStringUntilNot ( '=' );
                    this.Expect ( '[' );

                    // Raw initial comment delimiter
                    raw.Append ( '[' ).Append ( eqs ).Append ( '[' );

                    // Read the comment until ']', {'='}, ']'
                    var end = $"]{eqs}]";
                    var comment = this.reader.ReadStringUntil ( end );
                    raw.Append ( comment );
                    value.Append ( comment );

                    // Assert we have the ending delimiter
                    this.Expect ( end );
                    raw.Append ( end );
                }
                else
                {
                    var line = this.reader.ReadLine ( );
                    raw.Append ( line );
                    value.Append ( line );
                }
            }
            else if ( this.Consume ( "/*" ) )
            {
                // Read the start
                raw.Append ( "/*" );

                // Read the comment's contents
                var comment = this.reader.ReadStringUntil ( "*/" );
                raw.Append ( comment );
                value.Append ( comment );

                // Assert delimiter
                this.Expect ( "*/" );
                raw.Append ( "*/" );
            }
            else if ( this.Consume ( "//" ) )
            {
                // Read the start
                raw.Append ( "//" );

                // Read the comment's contents
                var comment = this.reader.ReadLine ( );
                raw.Append ( comment );
                value.Append ( comment );
            }

            return (raw.ToString ( ), value.ToString ( ));
        }

        #endregion Lua Comment Lexing

        #region Lua Literal String Lexing

        public (String Raw, String Value) ReadLiteralString ( )
        {
            var raw = new StringBuilder ( "[" );

            // Read rest of the start
            String eqs = this.reader.ReadStringUntilNot ( '=' );
            this.Expect ( '[' );
            raw.Append ( eqs ).Append ( '[' );

            // Ending delimiter
            var end = $"]{eqs}]";

            // Read the string contents
            var value = this.reader.ReadStringUntil ( end );

            // Read the delimiter at the end
            this.Expect ( end );
            raw.Append ( end );

            return (raw.ToString ( ), value);
        }

        #endregion Lua Literal String Lexing

        #region Ugly ReadChar Override

        // This is required due to decimal escape sequences
        // allowing for > 255 character codes but lua strings run
        // on ascii ([0,255] char range)
        protected new(String Raw, Char Value) ReadChar ( String Delimiter, CharLexSettings conf = default ( CharLexSettings ) )
        {
            var (Raw, Value) = base.ReadChar ( Delimiter, conf );
            if ( Value > 255 )
                throw new LexException ( "Invalid character escape.", this.Location );
            return (Raw, Value);
        }

        #endregion Ugly ReadChar Override

        protected override Token CreateToken ( String ID, String raw, Object value, TokenType type, SourceRange range )
            => new LToken ( ID, raw, value, type, range );

        protected override Token UpgradeToken ( Token token ) => new LToken ( token );
    }
}
