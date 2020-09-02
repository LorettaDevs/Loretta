using System;
using System.Collections.Generic;
using System.Linq;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a number literal expression.
    /// </summary>
    public class NumberExpression : Expression
    {
        private readonly LuaToken Token;

        /// <summary>
        /// The value of the number expression.
        /// </summary>
        public readonly Double Value;

        /// <inheritdoc />
        public override Boolean IsConstant => true;

        /// <inheritdoc />
        public override Object ConstantValue => this.Value;

        /// <summary>
        /// Initializes a new number literal expression.
        /// </summary>
        /// <param name="token"></param>
        public NumberExpression ( LuaToken token )
        {
            this.Token = token;
            this.Value = ( Double ) token.Value!;
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens
        {
            get
            {
                yield return this.Token;
            }
        }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitNumber ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitNumber ( this );
    }
}