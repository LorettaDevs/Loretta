using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes
{
    public class StatementList : ASTNode, IEquatable<StatementList>
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

        public void SetStatements ( IEnumerable<ASTNode> nodes )
        {
            foreach ( ASTNode node in this.Statements )
                this.RemoveChild ( node );
            this.Statements.Clear ( );

            foreach ( ASTNode node in nodes )
                this.AddStatement ( node );
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

        public override ASTNode Clone ( )
        {
            var list = new StatementList ( this.Parent, this.Scope, this.CloneTokenList ( ) );
            foreach ( ASTNode node in this.Statements )
                list.AddStatement ( node.Clone ( ) );
            return list;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as StatementList );
        }

        public Boolean Equals ( StatementList other )
        {
            if ( other == null || this.Statements.Count != other.Statements.Count )
                return false;
            for ( var i = 0; i < this.Statements.Count; i++ )
                if ( this.Statements[i] != other.Statements[i] )
                    return false;
            return true;
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = 271944183;
            foreach ( ASTNode stat in this.Statements )
                hashCode *= -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( stat );
            return hashCode;
        }

        public static Boolean operator == ( StatementList list1, StatementList list2 ) => EqualityComparer<StatementList>.Default.Equals ( list1, list2 );

        public static Boolean operator != ( StatementList list1, StatementList list2 ) => !( list1 == list2 );

        #endregion Generated Code
    }
}
