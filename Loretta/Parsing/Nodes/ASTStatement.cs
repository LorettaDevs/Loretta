using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes
{
    public abstract class ASTStatement : ASTNode
    {
        public Boolean HasSemicolon { get; set; }

        protected ASTStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }
    }
}
