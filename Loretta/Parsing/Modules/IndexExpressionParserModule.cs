using System;
using System.Diagnostics.CodeAnalysis;
using GParse;
using GParse.Errors;
using GParse.Lexing;
using GParse.Parsing;
using GParse.Parsing.Parselets;
using Loretta.Lexing;
using Loretta.Parsing.AST;

namespace Loretta.Parsing.Modules
{
    public class IndexExpressionParserModule : IInfixParselet<LuaTokenType, Expression>
    {
        public static void Register ( IPrattParserBuilder<LuaTokenType, Expression> builder, Int32 precedence )
        {
            var instance = new IndexExpressionParserModule ( precedence );
            builder.Register ( LuaTokenType.Dot, instance );
            builder.Register ( LuaTokenType.Colon, instance );
            builder.Register ( LuaTokenType.LBracket, instance );
        }

        public Int32 Precedence { get; }

        public IndexExpressionParserModule ( Int32 precedence )
        {
            this.Precedence = precedence;
        }

        public Boolean TryParse ( IPrattParser<LuaTokenType, Expression> parser, Expression indexee, IProgress<Diagnostic> diagnosticReporter, [NotNullWhen ( true )] out Expression expression )
        {
            if ( parser.TokenReader.Accept ( new[] { LuaTokenType.Dot, LuaTokenType.Colon }, out Token<LuaTokenType> sep ) )
            {
                if ( !parser.TokenReader.Accept ( LuaTokenType.Identifier, out Token<LuaTokenType> identifier ) )
                {
                    identifier = default;
                    diagnosticReporter.Report ( LuaDiagnostics.SyntaxError.ThingExpectedAfter ( parser.TokenReader.Location, "Identifier", sep.Id ) );
                }

                expression = new IndexExpression ( indexee, sep, new IdentifierExpression ( identifier, null ) );
                return true;
            }
            else if ( parser.TokenReader.Accept ( LuaTokenType.LBracket, out Token<LuaTokenType> lbracket ) )
            {
                if ( !parser.TryParseExpression ( out Expression indexer ) )
                {
                    throw new FatalParsingException ( parser.TokenReader.Location, "Expression expected after '['." );
                }

                if ( !parser.TokenReader.Accept ( LuaTokenType.RBracket, out Token<LuaTokenType> rbracket ) )
                {
                    rbracket = default;
                    diagnosticReporter.Report ( LuaDiagnostics.SyntaxError.ThingExpectedAfter ( rbracket.Range, "Closing bracket", "indexer expression" ) );
                }

                expression = new IndexExpression ( indexee, lbracket, indexer, rbracket );
                return true;
            }
            else
            {
                expression = default!;
                return false;
            }
        }
    }
}