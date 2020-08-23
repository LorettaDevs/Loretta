using System;
using System.Collections.Generic;
using GParse;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.AST;

namespace Loretta.Parsing
{
    public static class ASTNodeFactory
    {
        public static IdentifierExpression Identifier ( String identifier, SourceRange? range = null ) =>
            new IdentifierExpression ( TokenFactory.Identifier ( identifier, range ), null );

        public static BooleanExpression BooleanExpression ( Boolean value, SourceRange? range = null ) =>
            new BooleanExpression ( TokenFactory.Boolean ( value, range ), value );

        public static NumberExpression NumberExpression ( Double value, String? rawValue = null, SourceRange? range = null ) =>
            new NumberExpression ( TokenFactory.Number ( value, rawValue, range ) );

        public static StringExpression ShortString ( String value, String rawValue, SourceRange? range = null ) =>
            new StringExpression ( TokenFactory.ShortString ( value, rawValue, range ) );

        public static StringExpression LongString ( String value, String rawValue, SourceRange? range = null ) =>
            new StringExpression ( TokenFactory.LongString ( value, rawValue, range ) );

        public static NilExpression Nil ( SourceRange? range = null ) =>
            new NilExpression ( TokenFactory.Token ( "nil", LuaTokenType.Nil, "nil", null, range ) );

        public static UnaryOperationExpression UnaryOperation ( Token<LuaTokenType> op, Expression expression ) =>
            new UnaryOperationExpression ( UnaryOperationFix.Prefix, op, expression );

        public static UnaryOperationExpression UnaryOperation ( Expression expression, Token<LuaTokenType> op ) =>
            new UnaryOperationExpression ( UnaryOperationFix.Postfix, op, expression );

        public static IfStatement IfStatement ( IEnumerable<IfClause> clauses, StatementList? elseBlock = null ) =>
            elseBlock == null
                ? new IfStatement ( clauses, TokenFactory.Token ( "end", LuaTokenType.Keyword ) )
                : new IfStatement ( clauses, TokenFactory.Token ( "else", LuaTokenType.Keyword ), elseBlock,
                    TokenFactory.Token ( "end", LuaTokenType.Keyword ) );

        public static IfClause IfClause ( Expression condition, StatementList body ) =>
            new IfClause ( TokenFactory.Token ( "if", LuaTokenType.Keyword ), condition, TokenFactory.Token ( "then", LuaTokenType.Keyword ), body );

        public static StatementList StatementList ( Scope scope, params Statement[] statements ) =>
            new StatementList ( scope, statements );
    }
}