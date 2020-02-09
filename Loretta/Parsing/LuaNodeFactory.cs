using System.Collections.Generic;
using Loretta.Lexing;
using Loretta.Parsing;
using Loretta.Parsing.AST;

namespace Loretta.Parsing
{
    public static class LuaNodeFactory
    {
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
