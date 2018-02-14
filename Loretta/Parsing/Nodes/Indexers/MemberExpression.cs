using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Indexers
{
    // .something
    public class MemberExpression : ASTNode
    {
        public ASTNode Base { get; private set; } = null;

        public String Indexer { get; private set; } = null;

        /// <summary>
        /// : instead of .
        /// </summary>
        public Boolean SelfRef { get; set; }

        public MemberExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetIndexer ( String indexer )
        {
            this.Indexer = indexer;
        }

        public void SetBase ( ASTNode node )
        {
            if ( this.Base != null )
                this.RemoveChild ( this.Base );
            this.AddChild ( node );

            this.Base = node;
        }
    }
}
