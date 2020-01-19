using System;
using System.Collections.Generic;
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
    public class TableConstructorExpressionParserModule : IPrefixParselet<LuaTokenType, Expression>
    {
        public static TableConstructorExpressionParserModule Instance { get; } = new TableConstructorExpressionParserModule ( );

        public static void Register ( IPrattParserBuilder<LuaTokenType, Expression> builder ) =>
            builder.Register ( LuaTokenType.LCurly, Instance );

        private static readonly IEnumerable<LuaTokenType> FieldSeps = new[]
        {
            LuaTokenType.Comma,
            LuaTokenType.Semicolon
        };

        public Boolean TryParse ( IPrattParser<LuaTokenType, Expression> parser,
                                        IProgress<Diagnostic> diagnosticReporter,
                                        out Expression expression )
        {
            ITokenReader<LuaTokenType> reader = parser.TokenReader;
            if ( !reader.Accept ( LuaTokenType.LCurly, out Token<LuaTokenType> lcurly ) )
            {
                expression = default;
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