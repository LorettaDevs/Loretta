using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using GParse;
using GParse.IO;
using GParse.Lexing;
using GParse.Lexing.Modules;

namespace Loretta.Lexing.Modules
{
    /// <summary>
    /// The long comment lexer module.
    /// </summary>
    public class LongCommentLexerModule : ILexerModule<LuaTokenType>
    {
        /// <inheritdoc />
        public String Name => "Long Comment Lexer Module";

        /// <inheritdoc />
        public String? Prefix => "--[";

        /// <inheritdoc />
        public Boolean CanConsumeNext ( IReadOnlyCodeReader reader )
        {
            Debug.Assert ( reader.IsNext ( this.Prefix! ) );
            /* - - [ =* [
             * | | | |  |
             * 0 1 2 3 +3
             */
            return reader.Peek ( 3 ) is null or '=' or '[';
        }

        /// <inheritdoc />
        public Token<LuaTokenType> ConsumeNext ( ICodeReader reader, IProgress<Diagnostic> diagnosticEmitter )
        {
            Debug.Assert ( this.CanConsumeNext ( reader ) );

            SourceLocation start = reader.Location;
            reader.Advance ( 3 );
            var eqs = reader.ReadStringWhile ( ch => ch == '=' );

            if ( !reader.IsNext ( '[' ) )
            {
                reader.Restore ( start );
                reader.Advance ( 2 );
                var line = reader.ReadLine ( );

                return new Token<LuaTokenType> (
                    "comment",
                    "--" + line,
                    line,
                    LuaTokenType.Comment,
                    start.To ( reader.Location ),
                    true );
            }
            reader.Advance ( 1 );

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
                    diagnosticEmitter.Report ( LuaDiagnostics.SyntaxError.UnfinishedLongComment ( start.To ( reader.Location ) ) );
                    return new Token<LuaTokenType> (
                        "long-comment",
                        $"--[{eqs}[{body}",
                        body,
                        LuaTokenType.LongComment,
                        start.To ( reader.Location ),
                        true );
                }
                else
                {
                    var body = reader.ReadString ( startOffset );
                    var endDelim = reader.ReadString ( endOffset - startOffset + 1 );
                    diagnosticEmitter.Report ( LuaDiagnostics.SyntaxError.LongCommentWithIncompatibleDelimiters ( start.To ( reader.Location ) ) );
                    return new Token<LuaTokenType> (
                        "long-comment",
                        $"--[{eqs}[{body}{endDelim}",
                        body,
                        LuaTokenType.LongComment,
                        start.To ( reader.Location ),
                        true );
                }
            }
            else
            {
                var body = reader.ReadString ( offset );
                reader.Advance ( end.Length );

                return new Token<LuaTokenType> (
                    "long-comment",
                    $"--[{eqs}[{body}]{eqs}]",
                    body,
                    LuaTokenType.LongComment,
                    start.To ( reader.Location ),
                    true );
            }

            [MethodImpl ( MethodImplOptions.AggressiveInlining )]
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
