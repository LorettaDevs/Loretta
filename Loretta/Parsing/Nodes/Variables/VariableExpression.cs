using System;
using System.Collections.Generic;
using System.Text;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Variables
{
    public class VariableExpression : ASTNode
    {
        public Variable Variable { get; set; }

        public VariableExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }
    }
}
