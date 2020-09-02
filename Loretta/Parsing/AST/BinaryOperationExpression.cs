using System;
using System.Collections.Generic;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a binary operation expression.
    /// </summary>
    public class BinaryOperationExpression : Expression
    {
        /// <summary>
        /// The expression on the left side of the operator.
        /// </summary>
        public Expression Left { get; }

        /// <summary>
        /// The operation's operator.
        /// </summary>
        public LuaToken Operator { get; }

        /// <summary>
        /// The expression on the right side of the operator.
        /// </summary>
        public Expression Right { get; }

        /// <inheritdoc />
        public override Boolean IsConstant => false;

        /// <inheritdoc />
        public override Object ConstantValue => throw new InvalidOperationException ( "This is not a constant node." );

        /// <summary>
        /// Initializes a new binary operation expression.
        /// </summary>
        /// <param name="left">The expression on the left side of the operator.</param>
        /// <param name="op">The operation's operator.</param>
        /// <param name="right">The expression on the right side of the operator.</param>
        public BinaryOperationExpression ( Expression left, LuaToken op, Expression right )
        {
            this.Left = left ?? throw new ArgumentNullException ( nameof ( left ) );
            this.Operator = op;
            this.Right = right ?? throw new ArgumentNullException ( nameof ( right ) );
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens
        {
            get
            {
                yield return this.Operator;
            }
        }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children
        {
            get
            {
                yield return this.Left;
                yield return this.Right;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitBinaryOperation ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitBinaryOperation ( this );
    }
}