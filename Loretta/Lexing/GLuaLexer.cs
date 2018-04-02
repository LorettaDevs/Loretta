using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GParse.Lexing;
using GParse.Lexing.Errors;
using GParse.Lexing.Settings;
using Loretta.Utils;

namespace Loretta.Lexing
{
    public class GLuaLexer : LexerBase
    {
        private readonly List<LToken> LeadingFlairAccumulator = new List<LToken> ( );

        public GLuaLexer ( Stream input ) : this ( CInterop.ReadToEndAsASCII ( input ) )
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="input">Should be in Extended ASCII</param>
        /// <param name="isGlua"></param>
        public GLuaLexer ( String input ) : base ( CInterop.ConvertStringToASCII ( input ) )
        {
            input = CInterop.ConvertStringToASCII ( input );
            if ( input.Any ( ch => ch > 255 ) )
                throw new LexException ( "Input data is not in Extended ASCII.", this.Location );
            // Silently eat up BOM
            this.Consume ( "\xEF\xBB\xBF" );

            this.consumeNewlinesAutomatically = true;
            this.whitespaceID = "whitespace";

            // This won't be really used tbh since the base lexer
            // class isn't ready to handle floating point numbers
            // just yet
            this.numberSettings = new IntegerLexSettings
            {
                DefaultType = IntegerLexSettings.NumberType.Decimal,
                HexadecimalPrefix = "0x",
            };

            this.charSettings = new CharLexSettings
            {
                // \000 -> \255
                DecimalEscapePrefix = "\\",
                DecimalEscapeMaxLengh = 3,
                DecimalEscapeMaxValue = 255,

                // \x00 -> \xFF
                HexadecimalEscapePrefix = "\\x",
                HexadecimalEscapeMaxLengh = 2
            };

            // String settings. Not many pitfalls other than \
            // making a string multiline
            this.stringSettings = new StringLexSettings
            {
                CharSettings = this.charSettings,

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
                .AddToken ( "=", "=", TokenType.Operator )
                // Arithmetic
                .AddToken ( "+", "+", TokenType.Operator )
                .AddToken ( "-", "-", TokenType.Operator, next => next != '-' )
                .AddToken ( "%", "%", TokenType.Operator )
                .AddToken ( "/", "/", TokenType.Operator )
                .AddToken ( "*", "*", TokenType.Operator )
                .AddToken ( "^", "^", TokenType.Operator )
                // Binary
                .AddToken ( "<<", "<<", TokenType.Operator )
                .AddToken ( ">>", ">>", TokenType.Operator )
                .AddToken ( "|", "|", TokenType.Operator )
                .AddToken ( "&", "&", TokenType.Operator )
                .AddToken ( "~", "~", TokenType.Operator )
                // String
                .AddToken ( "..", "..", TokenType.Operator )
                // Boolean
                .AddToken ( "==", "==", TokenType.Operator )
                .AddToken ( "<", "<", TokenType.Operator )
                .AddToken ( "<=", "<=", TokenType.Operator )
                .AddToken ( "~=", "~=", TokenType.Operator )
                .AddToken ( "!=", "!=", TokenType.Operator )
                .AddToken ( ">", ">", TokenType.Operator )
                .AddToken ( ">=", ">=", TokenType.Operator )
                .AddToken ( "&&", "&&", TokenType.Operator )
                .AddToken ( "||", "||", TokenType.Operator )
                .AddToken ( "and", "and", TokenType.Operator, notalnum )
                .AddToken ( "or", "or", TokenType.Operator, notalnum );

            #endregion Binary Operators

            #region Unary Operators

            this.tokenManager
                .AddToken ( "not", "not", TokenType.Operator, notalnum )
                .AddToken ( "#", "#", TokenType.Operator )
                .AddToken ( "!", "!", TokenType.Operator );
            // - and ~ are in binary ops since they can be either.

            #endregion Unary Operators

            #region Symbols

            this.tokenManager
                .AddToken ( ",", ",", TokenType.Punctuation )
                .AddToken ( ".", ".", TokenType.Punctuation, ch => !LJUtils.IsDigit ( ch ) )
                .AddToken ( ":", ":", TokenType.Punctuation )
                .AddToken ( "::", "::", TokenType.Punctuation )
                .AddToken ( ";", ";", TokenType.Punctuation )
                .AddToken ( "...", "...", TokenType.Identifier )
                .AddToken ( "(", "(", TokenType.LParen )
                .AddToken ( ")", ")", TokenType.RParen )
                .AddToken ( "[", "[", TokenType.LBracket, next => next != '[' && next != '=' )
                .AddToken ( "]", "]", TokenType.RBracket )
                .AddToken ( "{", "{", TokenType.LCurly )
                .AddToken ( "}", "}", TokenType.RCurly );

            #endregion Symbols

            #region Keywords

            this.tokenManager
                .AddToken ( "break", "break", TokenType.Keyword, notalnum )
                .AddToken ( "continue", "continue", TokenType.Keyword, notalnum )
                .AddToken ( "goto", "goto", TokenType.Keyword, notalnum )
                .AddToken ( "do", "do", TokenType.Keyword, notalnum )
                .AddToken ( "end", "end", TokenType.Keyword, notalnum )
                .AddToken ( "while", "while", TokenType.Keyword, notalnum )
                .AddToken ( "repeat", "repeat", TokenType.Keyword, notalnum )
                .AddToken ( "until", "until", TokenType.Keyword, notalnum )
                .AddToken ( "if", "if", TokenType.Keyword, notalnum )
                .AddToken ( "then", "then", TokenType.Keyword, notalnum )
                .AddToken ( "elseif", "elseif", TokenType.Keyword, notalnum )
                .AddToken ( "else", "else", TokenType.Keyword, notalnum )
                .AddToken ( "for", "for", TokenType.Keyword, notalnum )
                .AddToken ( "in", "in", TokenType.Keyword, notalnum )
                .AddToken ( "function", "function", TokenType.Keyword, notalnum )
                .AddToken ( "local", "local", TokenType.Keyword, notalnum )
                .AddToken ( "return", "return", TokenType.Keyword, notalnum )
                .AddToken ( "nil", "nil", TokenType.Keyword, notalnum )
                .AddToken ( "true", "true", TokenType.Keyword, notalnum )
                .AddToken ( "false", "false", TokenType.Keyword, notalnum );

            #endregion Keywords
        }

        protected override Boolean CharIsDec ( Char ch ) => LJUtils.IsDigit ( ch );

        protected override Boolean CharIsHex ( Char ch ) => LJUtils.IsXDigit ( ch );

        protected override Boolean CharIsWhitepace ( Char ch ) => LJUtils.IsSpace ( ch );

        protected override Boolean TryReadToken ( out Token tok )
        {
            var peekedChar = ( Char ) this.reader.Peek ( );
            SourceLocation start = this.reader.Location;

            // Token starts null and might be set by any of the following
            tok = null;

            // Shebang (I'll consider this a leading flair because
            // it's technnically a comment)
            if ( this.Location.Line == 0 && this.Consume ( "#!" ) )
            {
                var exec = this.reader.ReadLine ( );
                tok = this.CreateToken ( "shebang", $"#!{exec}", exec, TokenType.Comment, start.To ( this.Location ) );
            }
            // Numbers
            else if ( LJUtils.IsDigit ( peekedChar ) || ( peekedChar == '.' && LJUtils.IsDigit ( ( Char ) this.reader.Peek ( 1 ) ) ) )
            {
                try
                {
                    (var Raw, var Value) = this.ReadLuaNumber ( );
                    tok = this.CreateToken ( "number", Raw, Value, TokenType.Number, start.To ( this.reader.Location ) );
                }
                catch ( LexException )
                {
                    tok = null;
                }
            }
            // Identifiers (keywords are read by the tokenmanager)
            else if ( LJUtils.IsIdent ( peekedChar ) || peekedChar == '_' )
            {
                var ident = this.reader.ReadStringWhile ( LJUtils.IsIdent );
                tok = this.CreateToken ( "ident", ident, ident, TokenType.Identifier, start.To ( this.reader.Location ) );
            }
            // Normal strings
            else if ( this.Consume ( '\'', '"' ) )
            {
                // LexerBase string reader doesn't adds
                // delimiters to the raw
                try
                {
                    (var Raw, var Value) = this.ReadString ( peekedChar.ToString ( ), false, this.stringSettings );
                    Raw = $"{peekedChar}{Raw}{peekedChar}";
                    tok = this.CreateToken ( "string", Raw, Value, TokenType.String, start.To ( this.Location ) );
                }
                catch ( LexException )
                {
                    tok = null;
                }
            }
            // Stuff that has/is :
            else if ( this.Consume ( ':' ) )
            {
                // Goto labels
                if ( this.Consume ( ':' ) )
                {
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
                (var Raw, var Value) = this.ReadComment ( );
                tok = CreateToken ( "comment", Raw, Value, TokenType.Comment, start.To ( this.Location ) );
            }
            else if ( this.Consume ( '[' ) )
            {
                (var Raw, var Value) = this.ReadLiteralString ( );
                tok = CreateToken ( "literalstring", Raw, Value, TokenType.String, start.To ( this.Location ) );
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

            if ( this.Consume ( '.' ) )
            {
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
                    var eqs = this.reader.ReadStringUntilNot ( '=' );
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
            var eqs = this.reader.ReadStringUntilNot ( '=' );
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

        protected override Token CreateToken ( in String ID, in String raw, in Object value, in TokenType type, in SourceRange range )
        {
            var tok = new LToken ( ID, raw, value, type, range );
            // Accumulate all kinds of flairs (currently only
            // comments and whitespace)
            if ( type == TokenType.Whitespace || type == TokenType.Comment )
            {
                this.LeadingFlairAccumulator.Add ( tok );
                return null;
            }
            else
            {
                // Add the leading flairs to the token itself
                tok.LeadingFlair.AddRange ( this.LeadingFlairAccumulator );
                this.LeadingFlairAccumulator.Clear ( );

                return tok;
            }
        }

        protected override Token UpgradeToken ( in Token token ) => new LToken ( token );
    }
}
