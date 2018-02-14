using System;
using System.Collections.Generic;
using System.Text;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Constants
{
    public class ParenthesisExpression : ASTNode
    {
        public ASTNode Expression { get; private set; }

        public ParenthesisExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
            this.Expression = null;
        }

        public void SetExpression ( ASTNode node )
        {
            this.Expression = node;
            this.AddChild ( node );
        }
    }
}
