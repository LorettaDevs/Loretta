using System.Collections.Generic;
using Loretta.Lexing;
using Loretta.Parsing.Nodes.Variables;

namespace Loretta.Parsing.Nodes.Loops
{
    public class ForGenericStatement : ASTStatement
    {
        public List<VariableExpression> Variables { get; } = new List<VariableExpression> ( );

        public List<ASTNode> Generators { get; } = new List<ASTNode> ( );

        public StatementList Body { get; private set; }

        public ForGenericStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void AddVariable ( VariableExpression var )
        {
            this.AddChild ( var );
            this.Variables.Add ( var );
        }

        public void AddGenerator ( ASTNode gen )
        {
            this.AddChild ( gen );
            this.Generators.Add ( gen );
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
