using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Indexers
{
    // [expr]
    public class IndexExpression : ASTNode, IEquatable<IndexExpression>
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

        public override ASTNode Clone ( )
        {
            var index = new IndexExpression ( this.Parent, this.Scope, this.CloneTokenList ( ) );
            index.SetBase ( this.Base.Clone ( ) );
            index.SetIndexer ( this.Indexer.Clone ( ) );
            return index;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as IndexExpression );
        }

        public Boolean Equals ( IndexExpression other )
        {
            return other != null &&
                    EqualityComparer<ASTNode>.Default.Equals ( this.Base, other.Base ) &&
                    EqualityComparer<ASTNode>.Default.Equals ( this.Indexer, other.Indexer );
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = 89669248;
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.Base );
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.Indexer );
            return hashCode;
        }

        public static Boolean operator == ( IndexExpression expression1, IndexExpression expression2 ) => EqualityComparer<IndexExpression>.Default.Equals ( expression1, expression2 );

        public static Boolean operator != ( IndexExpression expression1, IndexExpression expression2 ) => !( expression1 == expression2 );

        #endregion Generated Code
    }
}
