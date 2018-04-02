using System;
using System.Collections.Generic;
using Loretta.Lexing;
using Loretta.Parsing.Abstractions;

namespace Loretta.Parsing.Nodes.Indexers
{
    // .something
    public class MemberExpression : ASTNode, IEquatable<MemberExpression>
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

        public override ASTNode Clone ( )
        {
            var expr = new MemberExpression ( this.Parent, this.Scope, this.CloneTokenList ( ) );
            expr.SetBase ( this.Base.Clone ( ) );
            expr.SetIndexer ( this.Indexer );
            expr.SelfRef = this.SelfRef;
            return expr;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as MemberExpression );
        }

        public Boolean Equals ( MemberExpression other )
        {
            return other != null &&
                    EqualityComparer<ASTNode>.Default.Equals ( this.Base, other.Base ) &&
                     this.Indexer == other.Indexer &&
                     this.SelfRef == other.SelfRef;
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = -1404545224;
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.Base );
            hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode ( this.Indexer );
            hashCode = hashCode * -1521134295 + this.SelfRef.GetHashCode ( );
            return hashCode;
        }

        public static Boolean operator == ( MemberExpression expression1, MemberExpression expression2 ) => EqualityComparer<MemberExpression>.Default.Equals ( expression1, expression2 );

        public static Boolean operator != ( MemberExpression expression1, MemberExpression expression2 ) => !( expression1 == expression2 );

        #endregion Generated Code
    }
}
