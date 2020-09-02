using System;
using System.Collections.Generic;
using System.Linq;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.Visitor;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a vararg expression.
    /// </summary>
    public class VarArgExpression : Expression
    {
        private readonly Token<LuaTokenType> Token;

        /// <inheritdoc />
        public override Boolean IsConstant => false;

        /// <inheritdoc />
        public override Object ConstantValue => throw new InvalidOperationException ( "This is not a constant node." );

        /// <summary>
        /// Initializes a new vararg expression.
        /// </summary>
        /// <param name="token">The vararg token.</param>
        public VarArgExpression ( Token<LuaTokenType> token )
        {
            if ( token.Type != LuaTokenType.VarArg )
                throw new ArgumentException ( "Argument is not a vararg token", nameof ( token ) );
            this.Token = token;
        }

        /// <inheritdoc />
        public override IEnumerable<Token<LuaTokenType>> Tokens { get { yield return this.Token; } }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitVarArg ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitVarArg ( this );
    }
}