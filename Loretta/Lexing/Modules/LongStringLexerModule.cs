using System;
using System.Diagnostics;
using System.Text;
using GParse;
using GParse.IO;
using GParse.Lexing;
using GParse.Lexing.Modules;

namespace Loretta.Lexing.Modules
{
    /// <summary>
    /// The long strings lexer module.
    /// </summary>
    public class LongStringLexerModule : ILexerModule<LuaTokenType>
    {
        /// <inheritdoc />
        public virtual String Name => "Long String Lexer Module";

        /// <inheritdoc />
        public virtual String? Prefix => "[";

        /// <inheritdoc />
        public Boolean CanConsumeNext ( IReadOnlyCodeReader reader )
        {
            Debug.Assert ( reader.IsNext ( this.Prefix! ) );
            /* [ =*  [
             * | |   |
             * 0 1  +1
             */
            return reader.Peek ( 1 ) is null or '=' or '[';
        }


        /// <inheritdoc />
        public Token<LuaTokenType> ConsumeNext ( ICodeReader reader, IProgress<Diagnostic> diagnosticEmitter )
        {
            Debug.Assert ( this.CanConsumeNext ( reader ) );
            var rawBuilder = new StringBuilder ( "[" );
            SourceLocation start = reader.Location;
            reader.Advance ( 1 );

            var eqs = reader.ReadStringWhile ( ch => ch == '=' );
            rawBuilder.Append ( eqs );
            if ( !reader.IsNext ( '[' ) )
            {
                // Pretty much the only way to get into here is /\[=+/ since on the CanConsumeNext we check for
                // [ or =.
                diagnosticEmitter.Report ( LuaDiagnostics.SyntaxError.ThingExpectedAfter ( start.To ( reader.Location ), "[", "=" ) );
            }
            else
            {
                reader.Advance ( 1 );
                rawBuilder.Append ( '[' );
            }

            var end = $"]{eqs}]";
            var offset = reader.FindOffset ( end );
            if ( offset == -1 )
            {
                Int32 closingBracketOffset = 0, startOffset, endOffset;
                while ( !findClosingSequenceOffset ( reader, closingBracketOffset, out startOffset, out endOffset ) && startOffset != -1 )
                {
                    closingBracketOffset = endOffset;
                }

                if ( startOffset == -1 )
                {
                    var body = reader.ReadToEnd ( );
                    rawBuilder.Append ( body );

                    SourceRange range = start.To ( reader.Location );
                    diagnosticEmitter.Report ( LuaDiagnostics.SyntaxError.UnfinishedLongString ( range ) );
                    return new Token<LuaTokenType> ( "long-string", rawBuilder.ToString ( ), body, LuaTokenType.LongString, range );
                }
                else
                {
                    var body = reader.ReadString ( startOffset );
                    rawBuilder.Append ( body );
                    rawBuilder.Append ( reader.ReadString ( endOffset - startOffset + 1 ) );

                    SourceRange range = start.To ( reader.Location );
                    diagnosticEmitter.Report ( LuaDiagnostics.SyntaxError.LongStringWithIncompatibleDelimiters ( range ) );
                    return new Token<LuaTokenType> ( "long-string", rawBuilder.ToString ( ), body, LuaTokenType.LongString, range );
                }
            }
            else
            {
                var body = reader.ReadString ( offset );
                rawBuilder.Append ( body );
                reader.Advance ( end.Length );
                rawBuilder.Append ( end );

                return new Token<LuaTokenType> ( "long-string", rawBuilder.ToString ( ), body, LuaTokenType.LongString, start.To ( reader.Location ) );
            }

            static Boolean findClosingSequenceOffset ( IReadOnlyCodeReader reader, Int32 offset, out Int32 start, out Int32 end )
            {
                start = end = reader.FindOffset ( ']', offset );
                if ( start == -1 )
                    return false;
                end++;

                while ( reader.IsAt ( '=', end ) )
                {
                    end++;
                }
                return reader.IsAt ( ']', end );
            }
        }
    }
}