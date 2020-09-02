using System.Collections.Generic;
using System.Linq;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a continue statement.
    /// </summary>
    public class ContinueStatement : Statement
    {
        private readonly LuaToken _keyword;

        /// <summary>
        /// Initializes a new continue statement.
        /// </summary>
        /// <param name="continueKw">The continue keyword.</param>
        public ContinueStatement ( LuaToken continueKw )
        {
            this._keyword = continueKw;
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens { get { yield return this._keyword; } }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitContinue ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitContinue ( this );
    }
}