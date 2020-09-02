using System;
using System.Collections.Generic;
using GParse;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.AST;

namespace Loretta.Parsing
{
    /// <summary>
    /// The factory for AST nodes.
    /// </summary>
    public static class ASTNodeFactory
    {
        /// <summary>
        /// Creates a new <see cref="IdentifierExpression" /> node.
        /// </summary>
        /// <param name="identifier">The node's identifier.</param>
        /// <param name="range">The range of the identifier.</param>
        /// <returns></returns>
        public static IdentifierExpression Identifier ( String identifier, SourceRange? range = null ) =>
            new IdentifierExpression ( TokenFactory.Identifier ( identifier, range ), null );

        /// <summary>
        /// Creates a new <see cref="AST.BooleanExpression" /> node.
        /// </summary>
        /// <param name="value">The boolean value.</param>
        /// <param name="range">The range of the boolean raw value.</param>
        /// <returns></returns>
        public static BooleanExpression BooleanExpression ( Boolean value, SourceRange? range = null ) =>
            new BooleanExpression ( TokenFactory.Boolean ( value, range ), value );

        /// <summary>
        /// Creates a new <see cref="AST.NumberExpression" /> node.
        /// </summary>
        /// <param name="value">The numeric value of the expression.</param>
        /// <param name="rawValue">The string form of the numeric value.</param>
        /// <param name="range">The range of the string form of the numeric value.</param>
        /// <returns></returns>
        public static NumberExpression NumberExpression ( Double value, String? rawValue = null, SourceRange? range = null ) =>
            new NumberExpression ( TokenFactory.Number ( value, rawValue, range ) );

        /// <summary>
        /// Creates a new short string <see cref="StringExpression" /> node.
        /// </summary>
        /// <param name="value">The value of the short string.</param>
        /// <param name="rawValue">The string form of the short string (including delimiters).</param>
        /// <param name="range">The range of the string form of the short string.</param>
        /// <returns></returns>
        public static StringExpression ShortString ( String value, String rawValue, SourceRange? range = null ) =>
            new StringExpression ( TokenFactory.ShortString ( value, rawValue, range ) );

        /// <summary>
        /// Creates a new long string <see cref="StringExpression" /> node.
        /// </summary>
        /// <param name="value">The value of the short string.</param>
        /// <param name="rawValue">The string form of the short string (including delimiters).</param>
        /// <param name="range">The range of the string form of the short string.</param>
        /// <returns></returns>
        public static StringExpression LongString ( String value, String rawValue, SourceRange? range = null ) =>
            new StringExpression ( TokenFactory.LongString ( value, rawValue, range ) );

        /// <summary>
        /// Creates a new <see cref="NilExpression" /> node.
        /// </summary>
        /// <param name="range">The range of the nil keyword.</param>
        /// <returns></returns>
        public static NilExpression Nil ( SourceRange? range = null ) =>
            new NilExpression ( TokenFactory.Token ( "nil", LuaTokenType.Nil, "nil", null, range ) );

        /// <summary>
        /// Creates a new prefix <see cref="UnaryOperationExpression" /> node.
        /// </summary>
        /// <param name="op">The operation's operator.</param>
        /// <param name="expression">The operation's operand.</param>
        /// <returns></returns>
        public static UnaryOperationExpression UnaryOperation ( Token<LuaTokenType> op, Expression expression ) =>
            new UnaryOperationExpression ( UnaryOperationFix.Prefix, op, expression );

        /// <summary>
        /// Creates a new postfix <see cref="UnaryOperationExpression" /> node.
        /// </summary>
        /// <param name="expression">The operation's operand.</param>
        /// <param name="op">The operation's operator.</param>
        /// <returns></returns>
        public static UnaryOperationExpression UnaryOperation ( Expression expression, Token<LuaTokenType> op ) =>
            new UnaryOperationExpression ( UnaryOperationFix.Postfix, op, expression );

        /// <summary>
        /// Creates a new <see cref="AST.IfStatement" /> node.
        /// </summary>
        /// <param name="clauses">The if statement's clauses.</param>
        /// <param name="elseBlock">The else block (if any).</param>
        /// <returns></returns>
        public static IfStatement IfStatement ( IEnumerable<IfClause> clauses, StatementList? elseBlock = null ) =>
            elseBlock == null
                ? new IfStatement ( clauses, TokenFactory.Token ( "end", LuaTokenType.Keyword ) )
                : new IfStatement ( clauses, TokenFactory.Token ( "else", LuaTokenType.Keyword ), elseBlock,
                    TokenFactory.Token ( "end", LuaTokenType.Keyword ) );

        /// <summary>
        /// Creates a new <see cref="AST.IfClause" />.
        /// </summary>
        /// <param name="condition">The clause's condition.</param>
        /// <param name="body">The clause's body.</param>
        /// <returns></returns>
        public static IfClause IfClause ( Expression condition, StatementList body ) =>
            new IfClause ( TokenFactory.Token ( "if", LuaTokenType.Keyword ), condition, TokenFactory.Token ( "then", LuaTokenType.Keyword ), body );

        /// <summary>
        /// Creates a new <see cref="AST.StatementList" />.
        /// </summary>
        /// <param name="scope">The list's scope.</param>
        /// <param name="statements">The list's statements.</param>
        /// <returns></returns>
        public static StatementList StatementList ( Scope scope, params Statement[] statements ) =>
            new StatementList ( scope, statements );
    }
}