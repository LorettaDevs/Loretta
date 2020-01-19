using System.Collections.Generic;
using System.Linq;
using Loretta.Parsing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class GotoStatement : Statement
    {
        public GotoLabel Target { get; }

        public GotoStatement ( LuaToken gotoKw, LuaToken identKw, GotoLabel target )
        {
            this.Target = target;
            this.Tokens = new[] { gotoKw, identKw };
        }

        public override IEnumerable<LuaToken> Tokens { get; }
        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitGoto ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitGoto ( this );
    }
}
