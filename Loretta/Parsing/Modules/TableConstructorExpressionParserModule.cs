using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using GParse;
using GParse.Errors;
using GParse.Lexing;
using GParse.Parsing;
using GParse.Parsing.Parselets;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using Loretta.Parsing.AST.Tables;

namespace Loretta.Parsing.Modules
{
    /// <summary>
    /// The module that parses table constructor literal expressions.
    /// </summary>
    public class TableConstructorExpressionParserModule : IPrefixParselet<LuaTokenType, Expression>
    {
        /// <summary>
        /// The module's instance.
        /// </summary>
        public static TableConstructorExpressionParserModule Instance { get; } = new TableConstructorExpressionParserModule ( );

        /// <summary>
        /// Registers the module in a parser builder.
        /// </summary>
        /// <param name="builder">The builder to register in.</param>
        public static void Register ( IPrattParserBuilder<LuaTokenType, Expression> builder ) =>
            builder.Register ( LuaTokenType.LCurly, Instance );

        /// <summary>
        /// The two possible field separators.
        /// </summary>
        private static readonly IEnumerable<LuaTokenType> FieldSeps = new[]
        {
            LuaTokenType.Comma,
            LuaTokenType.Semicolon
        };

        /// <inheritdoc />
        public Boolean TryParse (
            IPrattParser<LuaTokenType, Expression> parser,
            IProgress<Diagnostic> diagnosticReporter,
            [NotNullWhen ( true )] out Expression expression )
        {
            ITokenReader<LuaTokenType> reader = parser.TokenReader;
            if ( !reader.Accept ( LuaTokenType.LCurly, out Token<LuaTokenType> lcurly ) )
            {
                expression = default!;
                return false;
            }

            var fields = new List<TableField> ( );
            Token<LuaTokenType> rcurly;
            while ( !reader.Accept ( LuaTokenType.RCurly, out rcurly ) )
            {
                if ( reader.IsAhead ( LuaTokenType.Identifier ) && reader.IsAhead ( LuaTokenType.Operator, "=", 1 ) )
                {
                    Token<LuaTokenType> identifier = reader.FatalExpect ( LuaTokenType.Identifier );
                    Token<LuaTokenType> equals = reader.FatalExpect ( LuaTokenType.Operator, "=" );
                    if ( !parser.TryParseExpression ( out Expression value ) )
                    {
                        throw new FatalParsingException ( reader.Location, "Expression expected after the '='." );
                    }
                    Token<LuaTokenType> delimiter = getDelimeter ( );
                    fields.Add ( new TableField ( identifier, equals, value, delimiter ) );
                }
                else if ( reader.IsAhead ( LuaTokenType.LBracket ) )
                {
                    Token<LuaTokenType> lbracket = reader.FatalExpect ( LuaTokenType.LBracket );
                    if ( !parser.TryParseExpression ( out Expression key ) )
                    {
                        throw new FatalParsingException ( reader.Location, "Expression expected after the '['." );
                    }
                    Token<LuaTokenType> rbracket = expectType ( LuaTokenType.RBracket );
                    Token<LuaTokenType> equals = expectId ( "=" );
                    if ( !parser.TryParseExpression ( out Expression value ) )
                    {
                        throw new FatalParsingException ( reader.Location, "Expression expected." );
                    }
                    Token<LuaTokenType> delimiter = getDelimeter ( );
                    fields.Add ( new TableField (
                        lbracket,
                        key,
                        rbracket,
                        equals,
                        value,
                        delimiter ) );
                }
                else
                {
                    if ( !parser.TryParseExpression ( out Expression value ) )
                    {
                        throw new FatalParsingException ( reader.Location, "Expression expected." );
                    }
                    fields.Add ( new TableField ( value, getDelimeter ( ) ) );
                }
            }

            expression = new TableConstructorExpression ( lcurly, fields.ToArray ( ), rcurly );
            return true;

            Token<LuaTokenType> expectId ( String id )
            {
                if ( !reader.Accept ( id, out Token<LuaTokenType> token ) )
                {
                    token = default;
                    diagnosticReporter.Report ( LuaDiagnostics.SyntaxError.ThingExpected ( token.Range, $"'{id}'" ) );
                }

                return token;
            }

            Token<LuaTokenType> expectType ( LuaTokenType tokenType )
            {
                if ( !reader.Accept ( tokenType, out Token<LuaTokenType> token ) )
                {
                    token = default;
                    diagnosticReporter.Report ( LuaDiagnostics.SyntaxError.ThingExpected ( token.Range, $"{tokenType}" ) );
                }

                return token;
            }

            Token<LuaTokenType> getDelimeter ( ) => reader.IsAhead ( LuaTokenType.RCurly ) ? default : expectTypes ( FieldSeps );

            Token<LuaTokenType> expectTypes ( IEnumerable<LuaTokenType> tokenTypes )
            {
                if ( !reader.Accept ( tokenTypes, out Token<LuaTokenType> token ) )
                {
                    token = default;
                    diagnosticReporter.Report ( LuaDiagnostics.SyntaxError.ThingExpected ( token.Range, String.Join ( ", ", tokenTypes ) ) );
                }

                return token;
            }
        }
    }
}