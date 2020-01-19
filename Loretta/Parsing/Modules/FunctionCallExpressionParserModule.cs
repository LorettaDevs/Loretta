using System;
using System.Collections.Generic;
using GParse;
using GParse.Lexing;
using GParse.Parsing;
using GParse.Parsing.Parselets;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.Modules
{
    public class FunctionCallExpressionParserModule : IInfixParselet<LuaTokenType, Expression>
    {
        public static FunctionCallExpressionParserModule Instance { get; private set; }

        public static void Register ( IPrattParserBuilder<LuaTokenType, Expression> builder, Int32 precedence )
        {
            Instance = new FunctionCallExpressionParserModule ( precedence );
            builder.Register ( LuaTokenType.LCurly, Instance );
            builder.Register ( LuaTokenType.String, Instance );
            builder.Register ( LuaTokenType.LongString, Instance );
            builder.Register ( LuaTokenType.LParen, Instance );
        }

        public Int32 Precedence { get; }

        public FunctionCallExpressionParserModule ( Int32 precedence )
        {
            this.Precedence = precedence;
        }

        public Boolean TryParse ( IPrattParser<LuaTokenType, Expression> parser, Expression function, IProgress<Diagnostic> diagnosticReporter, out Expression expression )
        {
            if ( parser.TokenReader.IsAhead ( LuaTokenType.LCurly )
                 && TableConstructorExpressionParserModule.Instance.TryParse ( parser, diagnosticReporter, out Expression tableExpression ) )
            {
                expression = new FunctionCallExpression ( function, tableExpression );
                return true;
            }
            else if ( parser.TokenReader.Accept ( new[] { LuaTokenType.String, LuaTokenType.LongString }, out LuaToken stringToken ) )
            {
                expression = new FunctionCallExpression ( function, new StringExpression ( stringToken ) );
                return true;
            }
            else if ( parser.TokenReader.Accept ( LuaTokenType.LParen, out LuaToken lparen ) )
            {
                ITokenReader<LuaTokenType> reader = parser.TokenReader;
                var args = new List<Expression> ( );
                var commas = new List<LuaToken> ( );
                LuaToken rparen;
                while ( !reader.Accept ( LuaTokenType.RParen, out rparen ) )
                {
                    if ( !parser.TryParseExpression ( out Expression argumentExpression ) )
                    {
                        LuaToken peek = parser.TokenReader.Lookahead ( );
                        rparen = TokenFactory.Token ( ")", LuaTokenType.RParen );
                        diagnosticReporter.Report ( LuaDiagnostics.SyntaxError.ThingExpectedAfter ( peek.Range,
                                                                                                    "Closing parenthesis",
                                                                                                    "argument list" ) );
                        break;
                    }

                    args.Add ( argumentExpression );
                    if ( reader.Accept ( LuaTokenType.RParen, out rparen ) )
                        break;

                    if ( !reader.Accept ( LuaTokenType.Comma, out LuaToken comma ) )
                    {
                        comma = reader.Consume ( );
                        diagnosticReporter.Report ( LuaDiagnostics.SyntaxError.ThingExpectedAfter ( comma.Range, "Comma", "argument" ) );
                    }
                    commas.Add ( comma );
                }

                expression = new FunctionCallExpression ( function, lparen, args, commas, rparen );
                return true;
            }
            else
            {
                expression = default;
                return false;
            }
        }
    }
}