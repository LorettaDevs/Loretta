using System;
using System.Collections.Generic;
using System.Text;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.IfStatements
{
    public class IfClause : ASTNode
    {
        public ASTNode Condition { get; private set; }

        public StatementList Body { get; private set; }

        public IfClause ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetCondition ( ASTNode cond )
        {
            if ( this.Condition != null )
                this.RemoveChild ( this.Condition );
            this.AddChild ( cond );
            this.Condition = cond;
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
