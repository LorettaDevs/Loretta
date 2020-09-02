using System.Collections.Generic;
using System.Linq;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.Visitor;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents an empty statement.
    /// </summary>
    public class EmptyStatement : Statement
    {
        /// <inheritdoc />
        public override IEnumerable<Token<LuaTokenType>> Tokens
        {
            get
            {
                if ( this.Semicolon.HasValue )
                    yield return this.Semicolon.Value;
            }
        }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children =>
            Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitEmptyStatement ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitEmptyStatement ( this );
    }
}