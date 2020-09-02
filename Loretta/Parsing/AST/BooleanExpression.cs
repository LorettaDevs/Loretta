using System;
using System.Collections.Generic;
using System.Linq;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.Visitor;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a boolean literal expression.
    /// </summary>
    public class BooleanExpression : Expression
    {
        /// <summary>
        /// The boolean literal token.
        /// </summary>
        private readonly Token<LuaTokenType> Token;

        /// <summary>
        /// The boolean value of the literal.
        /// </summary>
        public Boolean Value { get; }

        /// <inheritdoc />
        public override Boolean IsConstant => true;

        /// <inheritdoc />
        public override Object ConstantValue => this.Value;

        /// <summary>
        /// Initializes a new boolean expression.
        /// </summary>
        /// <param name="token">The boolean literal token.</param>
        /// <param name="value">The boolean value of the literal.</param>
        public BooleanExpression ( Token<LuaTokenType> token, Boolean value )
        {
            this.Value = value;
            this.Token = token;
        }

        /// <inheritdoc />
        public override IEnumerable<Token<LuaTokenType>> Tokens { get { yield return this.Token; } }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitBoolean ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitBoolean ( this );
    }
}