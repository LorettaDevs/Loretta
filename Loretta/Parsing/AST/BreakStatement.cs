using System.Collections.Generic;
using System.Linq;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class BreakStatement : Statement
    {
        private readonly LuaToken _keyword;

        public BreakStatement ( LuaToken breakKw )
        {
            this._keyword = breakKw;
        }

        public override IEnumerable<LuaToken> Tokens { get { yield return this._keyword; } }

        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitBreak ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitBreak ( this );
    }
}