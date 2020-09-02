using System;
using System.Collections.Generic;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// The fix of the unary operation.
    /// </summary>
    public enum UnaryOperationFix
    {
        /// <summary>
        /// A prefix operation.
        /// </summary>
        Prefix,

        /// <summary>
        /// A suffix/postfix operation.
        /// </summary>
        Postfix
    }

    /// <summary>
    /// Represents a unary operation expression.
    /// </summary>
    public class UnaryOperationExpression : Expression
    {
        /// <summary>
        /// The unary operation's fix.
        /// </summary>
        public UnaryOperationFix Fix { get; }

        /// <summary>
        /// The operation's operator.
        /// </summary>
        public LuaToken Operator { get; }

        /// <summary>
        /// The operation's operand.
        /// </summary>
        public Expression Operand { get; }

        /// <inheritdoc />
        public override Boolean IsConstant => false;

        /// <inheritdoc />
        public override Object ConstantValue => throw new InvalidOperationException ( "This is not a constant node." );

        /// <summary>
        /// Initializes a new unary operation expression.
        /// </summary>
        /// <param name="fix">The operation's fix.</param>
        /// <param name="op">The operator.</param>
        /// <param name="expr">The operation's operand.</param>
        public UnaryOperationExpression ( UnaryOperationFix fix, LuaToken op, Expression expr )
        {
            this.Fix = fix;
            this.Operator = op;
            this.Operand = expr ?? throw new ArgumentNullException ( nameof ( expr ) );
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
                yield return this.Operand;
            }
        }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitUnaryOperation ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitUnaryOperation ( this );
    }
}