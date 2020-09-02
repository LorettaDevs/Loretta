using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using GParse;
using GParse.IO;
using GParse.Lexing;
using GParse.Lexing.Modules;
using Loretta.Utilities;

namespace Loretta.Lexing.Modules
{
    /// <summary>
    /// Parses short strings in the lexer.
    /// </summary>
    public class ShortStringLexerModule : ILexerModule<LuaTokenType>
    {
        /// <summary>
        /// The lookup table of single-char escapes.
        /// </summary>
        private static readonly ImmutableDictionary<Char, Char> escapes = ImmutableDictionary.CreateRange ( new[]
        {
            new KeyValuePair<Char, Char> ( 'a', '\a' ),
            new KeyValuePair<Char, Char> ( 'b', '\b' ),
            new KeyValuePair<Char, Char> ( 'f', '\f' ),
            new KeyValuePair<Char, Char> ( 'n', '\n' ),
            new KeyValuePair<Char, Char> ( 'r', '\r' ),
            new KeyValuePair<Char, Char> ( 't', '\t' ),
            new KeyValuePair<Char, Char> ( 'v', '\v' ),
            new KeyValuePair<Char, Char> ( '\\', '\\' ),
            new KeyValuePair<Char, Char> ( '\n', '\n' ),
            new KeyValuePair<Char, Char> ( '\'', '\'' ),
            new KeyValuePair<Char, Char> ( '"', '"' ),
        } );

        /// <inheritdoc />
        public String Name => "String Lexer Module";

        /// <inheritdoc />
        public String? Prefix => null;

        /// <summary>
        /// The lua options used by this module.
        /// </summary>
        public LuaOptions LuaOptions { get; }

        /// <summary>
        /// Initializes this module with the provided options.
        /// </summary>
        /// <param name="luaOptions">The options to be used by this module.</param>
        public ShortStringLexerModule ( LuaOptions luaOptions )
        {
            this.LuaOptions = luaOptions;
        }

        /// <inheritdoc />
        public Boolean CanConsumeNext ( IReadOnlyCodeReader reader ) =>
            reader.IsNext ( '\'' ) || reader.IsNext ( '"' );

        /// <summary>
        /// Parses a decimal number after the decimal escape prefix.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="rawBuffer">The builder for the raw string.</param>
        /// <param name="parsedBuffer">The builder for the parser string.</param>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        private static void ParseDecimalEscape ( ICodeReader reader, StringBuilder rawBuffer, StringBuilder parsedBuffer )
        {
            var readChars = 0;
            var numStr = reader.ReadStringWhile ( ch =>
            {
                if ( readChars < 3 && CharUtils.IsDecimal ( ch ) )
                {
                    readChars++;
                    return true;
                }

                return false;
            } );

            if ( numStr.Length < 1 )
                throw new FormatException ( "Invalid decimal number." );

            var num = Int32.Parse ( numStr, NumberStyles.None, CultureInfo.InvariantCulture );
            if ( num > 255 )
                throw new FormatException ( "Escape code's value was out of range." );

            rawBuffer.Append ( numStr );
            parsedBuffer.Append ( ( Char ) num );
        }

        /// <summary>
        /// Parses a hexadecimal number after the hexadecimal escape prefix.
        /// </summary>
        /// <param name="reader">The reader to use.</param>
        /// <param name="rawBuffer">The builder for the raw string.</param>
        /// <param name="parsedBuffer">The builder for the parser string.</param>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        private static void ParseHexadecimalEscape ( ICodeReader reader, StringBuilder rawBuffer, StringBuilder parsedBuffer )
        {
            var readChars = 0;
            var numStr = reader.ReadStringWhile ( ch =>
            {
                if ( readChars < 2 && CharUtils.IsHexadecimal ( ch ) )
                {
                    readChars++;
                    return true;
                }

                return false;
            } );

            if ( numStr.Length < 1 )
                throw new FormatException ( "Invalid hexadecimal number." );

            var num = Int32.Parse ( numStr, NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture );
            rawBuffer.Append ( numStr );
            parsedBuffer.Append ( ( Char ) num );
        }

        /// <inheritdoc />
        public Token<LuaTokenType> ConsumeNext ( ICodeReader reader, IProgress<Diagnostic> diagnosticReporter )
        {
            StringBuilder rawBuffer = new StringBuilder ( ),
                parsedBuffer = new StringBuilder ( );
            SourceLocation start = reader.Location;
            var delim = ( Char ) reader.Read ( )!;
            if ( delim != '\'' && delim != '"' )
            {
                throw new InvalidOperationException ( "Short string lexer module called when the input on the reader is invalid. Did you forget to call CanConsumeNext?" );
            }
            rawBuffer.Append ( delim );

            while ( reader.Peek ( ) is Char peek && peek != delim )
            {
                switch ( peek )
                {
                    case '\\':
                    {
                        SourceLocation escapeStart = reader.Location;
                        reader.Advance ( 1 );
                        rawBuffer.Append ( '\\' );

                        if ( !( reader.Peek ( ) is Char peek2 ) )
                        {
                            diagnosticReporter.Report ( LuaDiagnostics.SyntaxError.ThingExpectedAfter ( reader.Location, "rest of escape", "backslash" ) );
                            break;
                        }

                        if ( escapes.TryGetValue ( peek2, out var escaped ) )
                        {
                            reader.Advance ( 1 );
                            rawBuffer.Append ( peek2 );
                            parsedBuffer.Append ( escaped );
                        }
                        else if ( peek2 == '\r' && reader.Peek ( 1 ) == '\n' )
                        {
                            reader.Advance ( 2 );
                            rawBuffer.Append ( "\r\n" );
                            parsedBuffer.Append ( "\r\n" );
                        }
                        else if ( CharUtils.IsDecimal ( peek2 ) )
                        {
                            ParseDecimalEscape ( reader, rawBuffer, parsedBuffer );
                        }
                        else if ( this.LuaOptions.AcceptHexEscapesInStrings && peek2 == 'x' )
                        {
                            reader.Advance ( 1 );
                            rawBuffer.Append ( 'x' );
                            ParseHexadecimalEscape ( reader, rawBuffer, parsedBuffer );
                        }
                        else
                        {
                            diagnosticReporter.Report ( LuaDiagnostics.SyntaxError.InvalidEscapeInString ( escapeStart.To ( reader.Location ) ) );
                        }

                        break;
                    }

                    case '\r':
                    case '\n':
                        goto endloop;

                    default:
                        reader.Advance ( 1 );
                        rawBuffer.Append ( peek );
                        parsedBuffer.Append ( peek );
                        break;
                }
            }

        endloop:
            Char endingDelim;
            if ( reader.Peek ( ) != delim )
            {
                diagnosticReporter.Report ( LuaDiagnostics.SyntaxError.UnfinishedString ( start.To ( reader.Location ) ) );
                endingDelim = delim;
            }
            else
            {
                endingDelim = ( Char ) reader.Read ( )!;
            }
            rawBuffer.Append ( endingDelim );

            return new Token<LuaTokenType> ( "string", rawBuffer.ToString ( ), parsedBuffer.ToString ( ), LuaTokenType.String, start.To ( reader.Location ) );
        }
    }
}