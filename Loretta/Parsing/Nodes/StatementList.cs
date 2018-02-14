using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes
{
    public class StatementList : ASTNode
    {
        public List<ASTNode> Statements { get; } = new List<ASTNode> ( );

        public StatementList ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void AddStatement ( ASTNode node )
        {
            this.AddChild ( node );
            this.Statements.Add ( node );
        }

        public void InsertStatement ( ASTNode node, Int32 index )
        {
            if ( index < 0 )
                index = Math.Max ( this.Statements.Count + index, 0 );
            this.AddChild ( node );
            this.Statements.Insert ( index, node );
        }

        public void RemoveStatement ( Int32 index )
        {
            if ( index < 0 )
                index = Math.Max ( this.Statements.Count + index, 0 );
            this.Statements.RemoveAt ( index );
        }

        public Int32 IndexOfStatement ( ASTNode stat )
        {
            return this.Statements.IndexOf ( stat );
        }
    }
}
