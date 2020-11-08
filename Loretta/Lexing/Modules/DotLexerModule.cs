using System;
using System.Diagnostics;
using GParse;
using GParse.IO;
using GParse.Lexing;
using GParse.Lexing.Modules;
using Loretta.Utilities;

namespace Loretta.Lexing.Modules
{
    /// <summary>
    /// Handles the parsing of ., .. and ... in the lexer.
    /// </summary>
    public class DotLexerModule : ILexerModule<LuaTokenType>
    {
        /// <inheritdoc />
        public String Name => "Dot Lexer Module";

        /// <inheritdoc />
        public String? Prefix => ".";

        /// <inheritdoc />
        public Boolean CanConsumeNext ( IReadOnlyCodeReader reader )
        {
            Debug.Assert ( reader.IsNext ( '.' ) );
            // . followed by non-number
            var peek = reader.Peek ( 1 );
            return peek is null || !CharUtils.IsDecimal ( peek.Value );
        }

        /// <inheritdoc />
        public Token<LuaTokenType> ConsumeNext ( ICodeReader reader, IProgress<Diagnostic> diagnosticEmitter )
        {
            Debug.Assert ( this.CanConsumeNext ( reader ) );
            SourceLocation start = reader.Location;
            reader.Advance ( 1 );
            if ( reader.Peek ( ) == '.' )
            {
                reader.Advance ( 1 );
                if ( reader.Peek ( ) == '.' )
                {
                    reader.Advance ( 1 );
                    return new Token<LuaTokenType> ( "...", "...", "...", LuaTokenType.VarArg, start.To ( reader.Location ) );
                }
                return new Token<LuaTokenType> ( "..", "..", "..", LuaTokenType.Operator, start.To ( reader.Location ) );
            }
            return new Token<LuaTokenType> ( ".", ".", ".", LuaTokenType.Dot, start.To ( reader.Location ) );
        }
    }
}