using System;
using GParse.Collections;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing;
using Loretta.Parsing.AST;

namespace Loretta.Tests.Parsing
{
    public abstract class ParserTestClass
    {
        protected static (StatementList statements, DiagnosticList diagnostics) Parse ( LuaOptions luaOptions, String code )
        {
            var diagnostics = new DiagnosticList ( );
            var lexerBuilder = new LuaLexerBuilder ( luaOptions );
            var parserBuilder = new LuaParserBuilder ( luaOptions );
            ILexer<LuaTokenType> lexer = lexerBuilder.CreateLexer ( code, diagnostics );
            LuaParser parser = parserBuilder.CreateParser ( new TokenReader<LuaTokenType> ( lexer ), diagnostics );
            return (parser.Parse ( ), diagnostics);
        }
    }
}
