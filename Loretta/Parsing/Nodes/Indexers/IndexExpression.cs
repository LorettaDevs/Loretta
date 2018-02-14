using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Indexers
{
    // [expr]
    public class IndexExpression : ASTNode
    {
        public ASTNode Base { get; private set; }

        public ASTNode Indexer { get; private set; }

        public IndexExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetBase ( ASTNode node )
        {
            if ( this.Base != null )
                this.RemoveChild ( this.Base );
            this.AddChild ( node );

            this.Base = node;
        }

        public void SetIndexer ( ASTNode node )
        {
            if ( this.Indexer != null )
                this.RemoveChild ( this.Indexer );
            this.AddChild ( node );

            this.Indexer = node;
        }
    }
}
