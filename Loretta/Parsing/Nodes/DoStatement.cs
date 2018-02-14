using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes
{
    // simply an excuse to create a new scope
    public class DoStatement : ASTStatement
    {
        public StatementList Body { get; private set; }

        public DoStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetBody ( StatementList body )
        {
            if ( this.Body != null )
                this.RemoveChild ( this.Body );
            this.AddChild ( body );
            this.Body = body;
        }
    }
}
