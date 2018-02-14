using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Constants
{
    /// <summary>
    /// Even though this is in the constants namespace, this
    /// cannot be resolved unless we execute the code in some form
    /// or it might not be resolvable at all
    /// </summary>
    public class VarArgExpression : ASTNode
    {
        public VarArgExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }
    }
}
