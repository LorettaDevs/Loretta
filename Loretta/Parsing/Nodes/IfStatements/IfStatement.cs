using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.IfStatements
{
    public class IfStatement : ASTStatement, IEquatable<IfStatement>
    {
        public IfClause MainClause { get; private set; }

        public List<IfClause> ElseIfClauses { get; } = new List<IfClause> ( );

        public StatementList ElseBlock { get; private set; }

        public IfStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetMainClause ( IfClause main )
        {
            if ( this.MainClause != null )
                this.RemoveChild ( this.MainClause );
            this.AddChild ( main );
            this.MainClause = main;
        }

        public void AddElseIfClause ( IfClause elif )
        {
            if ( elif != null )
                return;
            this.AddChild ( elif );
            this.ElseIfClauses.Add ( elif );
        }

        public void SetElseIfClauses ( IEnumerable<ASTNode> elifs )
        {
            foreach ( IfClause elif in this.ElseIfClauses )
                this.RemoveChild ( elif );
            this.ElseIfClauses.Clear ( );
            foreach ( IfClause elif in elifs )
                this.AddElseIfClause ( elif );
        }

        public void SetElseBlock ( StatementList elseb )
        {
            if ( this.ElseBlock != null )
                this.RemoveChild ( this.ElseBlock );
            this.AddChild ( elseb );
            this.ElseBlock = elseb;
        }

        public override ASTNode Clone ( )
        {
            var ifs = new IfStatement ( this.Parent, this.Scope, this.CloneTokenList ( ) );
            ifs.SetMainClause ( ( IfClause ) this.MainClause.Clone ( ) );
            foreach ( IfClause elif in this.ElseIfClauses )
                ifs.AddElseIfClause ( ( IfClause ) elif.Clone ( ) );
            if ( this.ElseBlock != null )
                ifs.SetElseBlock ( ( StatementList ) this.ElseBlock.Clone ( ) );
            return ifs;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as IfStatement );
        }

        public Boolean Equals ( IfStatement other )
        {
            if ( other == null || !EqualityComparer<IfClause>.Default.Equals ( this.MainClause, other.MainClause )
                || !EqualityComparer<StatementList>.Default.Equals ( this.ElseBlock, other.ElseBlock )
                || this.ElseIfClauses.Count != other.ElseIfClauses.Count )
                return false;
            for ( var i = 0; i < this.ElseIfClauses.Count; i++ )
                if ( this.ElseIfClauses[i] != other.ElseIfClauses[i] )
                    return false;
            return true;
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = -1789328207;
            hashCode = hashCode * -1521134295 + EqualityComparer<IfClause>.Default.GetHashCode ( this.MainClause );
            foreach ( IfClause elif in this.ElseIfClauses )
                hashCode *= -1521134295 + EqualityComparer<IfClause>.Default.GetHashCode ( elif );
            hashCode = hashCode * -1521134295 + EqualityComparer<StatementList>.Default.GetHashCode ( this.ElseBlock );
            return hashCode;
        }

        public static Boolean operator == ( IfStatement statement1, IfStatement statement2 ) => EqualityComparer<IfStatement>.Default.Equals ( statement1, statement2 );

        public static Boolean operator != ( IfStatement statement1, IfStatement statement2 ) => !( statement1 == statement2 );

        #endregion Generated Code
    }
}
