using System;
using System.Diagnostics;
using GParse;
using GParse.IO;
using GParse.Lexing;
using GParse.Lexing.Modules;
using Loretta.Utilities;

namespace Loretta.Lexing.Modules
{
    public class DotLexerModule : ILexerModule<LuaTokenType>
    {
        public String Name => "Dot Lexer Module";
        public String Prefix => ".";

        public Boolean CanConsumeNext ( IReadOnlyCodeReader reader )
        {
            // . followed by non-number
            var peek = reader.Peek ( 1 );
            return peek is null || !CharUtils.IsDecimal ( peek.Value );
        }

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