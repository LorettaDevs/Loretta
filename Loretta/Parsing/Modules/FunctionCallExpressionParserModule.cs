using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GParse;
using GParse.Lexing;
using GParse.Parsing;
using GParse.Parsing.Parselets;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.Modules
{
    /// <summary>
    /// The module that parses function call expressions.
    /// </summary>
    public class FunctionCallExpressionParserModule : IInfixParselet<LuaTokenType, Expression>
    {
        /// <summary>
        /// Registers the module in a parser builder with the provided precedence.
        /// </summary>
        /// <param name="builder">The builder to register the module in.</param>
        /// <param name="precedence">The precedence to register with.</param>
        public static void Register ( IPrattParserBuilder<LuaTokenType, Expression> builder, Int32 precedence )
        {
            var instance = new FunctionCallExpressionParserModule ( precedence );
            builder.Register ( LuaTokenType.LCurly, instance );
            builder.Register ( LuaTokenType.String, instance );
            builder.Register ( LuaTokenType.LongString, instance );
            builder.Register ( LuaTokenType.LParen, instance );
        }

        /// <summary>
        /// The configured function call precedence.
        /// </summary>
        public Int32 Precedence { get; }

        /// <summary>
        /// Initializes a new function call expression parser module.
        /// </summary>
        /// <param name="precedence">The precedence to be used as the function call precedence.</param>
        public FunctionCallExpressionParserModule ( Int32 precedence )
        {
            this.Precedence = precedence;
        }

        /// <inheritdoc />
        public Boolean TryParse (
            IPrattParser<LuaTokenType, Expression> parser,
            Expression function,
            IProgress<Diagnostic> diagnosticReporter,
            [NotNullWhen ( true )] out Expression expression )
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
                        diagnosticReporter.Report ( LuaDiagnostics.SyntaxError.ThingExpectedAfter (
                            peek.Range,
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
                expression = default!;
                return false;
            }
        }
    }
}