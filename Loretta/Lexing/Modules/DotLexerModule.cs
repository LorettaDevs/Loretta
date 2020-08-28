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
            return reader.IsNext ( '.' )
                   && ( reader.Peek ( 1 ) is null
                        || ( reader.Peek ( 1 ) is Char peek2 && !CharUtils.IsDecimal ( peek2 ) ) );
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