using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.IfStatements
{
    public class IfClause : ASTNode, IEquatable<IfClause>
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

        public override ASTNode Clone ( )
        {
            var clause = new IfClause ( this.Parent, this.Scope, this.CloneTokenList ( ) );
            clause.SetCondition ( this.Condition.Clone ( ) );
            clause.SetBody ( ( StatementList ) this.Body.Clone ( ) );
            return clause;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as IfClause );
        }

        public Boolean Equals ( IfClause other )
        {
            return other != null &&
                    EqualityComparer<ASTNode>.Default.Equals ( this.Condition, other.Condition ) &&
                    EqualityComparer<StatementList>.Default.Equals ( this.Body, other.Body );
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = -2019592797;
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.Condition );
            hashCode = hashCode * -1521134295 + EqualityComparer<StatementList>.Default.GetHashCode ( this.Body );
            return hashCode;
        }

        public static Boolean operator == ( IfClause clause1, IfClause clause2 ) => EqualityComparer<IfClause>.Default.Equals ( clause1, clause2 );

        public static Boolean operator != ( IfClause clause1, IfClause clause2 ) => !( clause1 == clause2 );

        #endregion Generated Code
    }
}
