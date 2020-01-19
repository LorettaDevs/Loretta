using System;
using GParse;
using GParse.Errors;
using GParse.Lexing;
using GParse.Parsing;
using GParse.Parsing.Parselets;
using Loretta.Lexing;
using Loretta.Parsing.AST;

namespace Loretta.Parsing.Modules
{
    public class GroupedExpressionParserModule : IPrefixParselet<LuaTokenType, Expression>
    {
        public static GroupedExpressionParserModule Instance { get; } = new GroupedExpressionParserModule ( );

        public static void Register ( IPrattParserBuilder<LuaTokenType, Expression> builder ) =>
            builder.Register ( LuaTokenType.LParen, Instance );

        public Boolean TryParse ( IPrattParser<LuaTokenType, Expression> parser, IProgress<Diagnostic> diagnosticEmitter, out Expression expression )
        {
            if ( !parser.TokenReader.Accept ( LuaTokenType.LParen, out Token<LuaTokenType> lparen ) )
            {
                expression = default;
                return false;
            }

            if ( !parser.TryParseExpression ( out Expression expr ) )
            {
                throw new FatalParsingException ( parser.TokenReader.Location, "Expression expected after '('." );
            }

            if ( !parser.TokenReader.Accept ( LuaTokenType.RParen, out Token<LuaTokenType> rparen ) )
            {
                rparen = default;
                diagnosticEmitter.Report ( LuaDiagnostics.SyntaxError.ThingExpected ( rparen.Range, "Closing parenthesis" ) );
            }

            expression = new GroupedExpression ( lparen, expr, rparen );
            return true;
        }
    }
}