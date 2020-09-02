using System;
using System.Collections.Generic;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.Visitor;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a grouped/parenthesized expression.
    /// </summary>
    public class GroupedExpression : Expression
    {
        /// <summary>
        /// The wrapped expression.
        /// </summary>
        public Expression InnerExpression { get; }

        /// <inheritdoc />
        public override Boolean IsConstant => this.InnerExpression.IsConstant;

        /// <inheritdoc />
        public override Object? ConstantValue => this.InnerExpression.ConstantValue;

        /// <summary>
        /// Initializes a new grouped expression.
        /// </summary>
        /// <param name="lparen">The left parenthesis token.</param>
        /// <param name="inner">The wrapped expression.</param>
        /// <param name="rparen">The right parenthesis.</param>
        public GroupedExpression ( Token<LuaTokenType> lparen, Expression inner, Token<LuaTokenType> rparen )
        {
            this.InnerExpression = inner ?? throw new ArgumentNullException ( nameof ( inner ) );
            this.Tokens = new[] { lparen, rparen };
        }

        /// <inheritdoc />
        public override IEnumerable<Token<LuaTokenType>> Tokens { get; }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children { get { yield return this.InnerExpression; } }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitGroupedExpression ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitGroupedExpression ( this );
    }
}