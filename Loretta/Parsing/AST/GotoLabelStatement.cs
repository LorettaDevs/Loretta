using System.Collections.Generic;
using System.Linq;
using Loretta.Parsing.Visitor;
using LuaToken = GParse.Lexing.Token<Loretta.Lexing.LuaTokenType>;

namespace Loretta.Parsing.AST
{
    public class GotoLabelStatement : Statement
    {
        public GotoLabel Label { get; }

        public GotoLabelStatement ( LuaToken ldelim, GotoLabel label, LuaToken ident, LuaToken rdelim )
        {
            this.Label = label;
            this.Tokens = new[] { ldelim, ident, rdelim };
        }

        public override IEnumerable<LuaToken> Tokens { get; }
        public override IEnumerable<LuaASTNode> Children => Enumerable.Empty<LuaASTNode> ( );

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitGotoLabel ( this );

        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitGotoLabel ( this );
    }
}