using System;
using GParse;
using GParse.IO;
using GParse.Lexing;
using GParse.Lexing.Modules;

namespace Loretta.Lexing.Modules
{
    public class DotLexerModule : ILexerModule<LuaTokenType>
    {
        public String Name => "Dot Lexer Module";
        public String Prefix => ".";

        public Boolean CanConsumeNext ( IReadOnlyCodeReader reader )
        {
            return reader.IsNext ( '.' )
                   && ( reader.Peek ( 1 ) is null
                        || ( reader.Peek ( 1 ) is Char peek2 && !CharUtils.IsDecimal ( peek2 ) ) );
        }

        public Token<LuaTokenType> ConsumeNext ( ICodeReader reader, IProgress<Diagnostic> diagnosticEmitter )
        {
            if ( !this.CanConsumeNext ( reader ) )
            {
                throw new InvalidOperationException ( "ConsumeNext called on an invalid string. Did you forget to check with CanConsumeNext?" );
            }

            SourceLocation start = reader.Location;
            reader.Advance ( 1 );
            return new Token<LuaTokenType> ( ".", ".", ".", LuaTokenType.Dot, start.To ( reader.Location ) );
        }
    }
}