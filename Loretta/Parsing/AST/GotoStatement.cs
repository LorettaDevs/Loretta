using System.Collections.Generic;
using System.Linq;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a goto label.
    /// </summary>
    public class GotoStatement : Statement
    {
        /// <summary>
        /// The goto label target.
        /// </summary>
        public GotoLabel Target { get; }

        /// <summary>
        /// Initializes a new goto statement.
        /// </summary>
        /// <param name="gotoKw">The goto keyword.</param>
        /// <param name="identKw">The identifier.</param>
        /// <param name="target">The goto label.</param>
        public GotoStatement ( LuaToken gotoKw, LuaToken identKw, GotoLabel target )
        {
            this.Target = target;
            this.Tokens = new[] { gotoKw, identKw };
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens { get; }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitGoto ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitGoto ( this );
    }
}