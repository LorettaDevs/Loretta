using System;
using System.Collections.Generic;
using System.Linq;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents an identifier expression.
    /// </summary>
    public class IdentifierExpression : Expression
    {
        private readonly LuaToken Token;

        /// <summary>
        /// The expression's variable.
        /// </summary>
        public readonly Variable? Variable;

        /// <summary>
        /// The identifier's identifier.
        /// </summary>
        public String Identifier => this.Variable?.Identifier ?? ( String ) this.Token.Value!;

        /// <inheritdoc />
        public override Boolean IsConstant => false;

        /// <inheritdoc />
        public override Object ConstantValue => throw new InvalidOperationException ( "This is not a constant node." );

        /// <summary>
        /// Initializes an new identifier expression.
        /// </summary>
        /// <param name="token">The identifier token.</param>
        /// <param name="variable">The identifier variable.</param>
        public IdentifierExpression ( LuaToken token, Variable? variable )
        {
            this.Token = token;
            this.Variable = variable;
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

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitIdentifier ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitIdentifier ( this );
    }
}