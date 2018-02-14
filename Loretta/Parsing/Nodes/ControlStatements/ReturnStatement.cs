using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.ControlStatements
{
    public class ReturnStatement : ASTStatement
    {
        public List<ASTNode> Returns { get; } = new List<ASTNode> ( );

        public ReturnStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void AddReturn ( ASTNode value )
        {
            this.AddChild ( value );
            this.Returns.Add ( value );
        }
    }
}
