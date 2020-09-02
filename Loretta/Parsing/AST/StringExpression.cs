using System;
using System.Collections.Generic;
using System.Linq;
using Loretta.Lexing;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a string literal expression.
    /// </summary>
    public class StringExpression : Expression
    {
        private readonly LuaToken Token;

        /// <summary>
        /// Whether this is a long/literal string literal.
        /// </summary>
        public Boolean IsLong => this.Token.Type == LuaTokenType.LongString;

        /// <summary>
        /// The string's value.
        /// </summary>
        public String Value => ( String ) this.Token.Value!;

        /// <inheritdoc />
        public override Boolean IsConstant => true;

        /// <inheritdoc />
        public override Object ConstantValue => this.Value;

        /// <summary>
        /// Initializes a new string literal expression.
        /// </summary>
        /// <param name="token">The string literal token.</param>
        public StringExpression ( LuaToken token )
        {
            this.Token = token;
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

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitString ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitString ( this );
    }
}