using System.Collections.Generic;
using System.Linq;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    /// <summary>
    /// Represents a break statement.
    /// </summary>
    public class BreakStatement : Statement
    {
        private readonly LuaToken _keyword;

        /// <summary>
        /// Initializes a new break statement.
        /// </summary>
        /// <param name="breakKw"></param>
        public BreakStatement ( LuaToken breakKw )
        {
            this._keyword = breakKw;
        }

        /// <inheritdoc />
        public override IEnumerable<LuaToken> Tokens { get { yield return this._keyword; } }

        /// <inheritdoc />
        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitBreak ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitBreak ( this );
    }
}