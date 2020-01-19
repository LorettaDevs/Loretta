using System.Collections.Generic;
using System.Linq;
using GParse.Lexing;
using Loretta.Lexing;
using Loretta.Parsing.AST;
using Loretta.Parsing.Visitor;

namespace Loretta.Parsing.AST
{
    public class ExpressionStatement : Statement
    {
        public Expression Expression { get; }

        public ExpressionStatement ( Expression expression )
        {
            this.Expression = expression ?? throw new System.ArgumentNullException ( nameof ( expression ) );
        }

        public override IEnumerable<Token<LuaTokenType>> Tokens => Enumerable.Empty<Token<LuaTokenType>> ( );
        public override IEnumerable<LuaASTNode> Children { get { yield return this.Expression; } }

        internal override void Accept ( ITreeVisitor visitor ) => visitor.VisitExpressionStatement ( this );
        internal override T Accept<T> ( ITreeVisitor<T> visitor ) => visitor.VisitExpressionStatement ( this );
    }
}
