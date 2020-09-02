using System.Collections.Generic;
using System.Linq;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a goto label statement.
    /// </summary>
    public class GotoLabelStatement : Statement
    {
        /// <summary>
        /// The goto label.
        /// </summary>
        public GotoLabel Label { get; }

        /// <summary>
        /// Initializes a new goto label statement.
        /// </summary>
        /// <param name="ldelim">The left delimiter token.</param>
        /// <param name="label">The goto label.</param>
        /// <param name="ident">The identifier token.</param>
        /// <param name="rdelim">The right delimiter token.</param>
        public GotoLabelStatement ( LuaToken ldelim, GotoLabel label, LuaToken ident, LuaToken rdelim )
        {
            this.Label = label;
            this.Tokens = new[] { ldelim, ident, rdelim };
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens { get; }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitGotoLabel ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitGotoLabel ( this );
    }
}