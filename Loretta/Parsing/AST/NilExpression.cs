using System;
using System.Collections.Generic;
using System.Linq;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.Visitor;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a nil literal expression.
    /// </summary>
    public class NilExpression : Expression
    {
        private readonly Token<LuaTokenType> Token;

        /// <inheritdoc />
        public override Boolean IsConstant => true;

        /// <inheritdoc />
        public override Object? ConstantValue => null;

        /// <summary>
        /// Initializes a new nil literal expression.
        /// </summary>
        /// <param name="tok"></param>
        public NilExpression ( Token<LuaTokenType> tok )
        {
            this.Token = tok;
        }

        /// <inheritdoc />
        public override IEnumerable<Token<LuaTokenType>> Tokens
        {
            get
            {
                yield return this.Token;
            }
        }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitNil ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitNil ( this );
    }
}