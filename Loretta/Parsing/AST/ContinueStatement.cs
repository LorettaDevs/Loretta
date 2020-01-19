using System.Collections.Generic;
using System.Linq;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class ContinueStatement : Statement
    {
        private readonly LuaToken _keyword;

        public ContinueStatement ( LuaToken continueKw )
        {
            this._keyword = continueKw;
        }

        public override IEnumerable<LuaToken> Tokens { get { yield return this._keyword; } }

        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitContinue ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitContinue ( this );
    }
}