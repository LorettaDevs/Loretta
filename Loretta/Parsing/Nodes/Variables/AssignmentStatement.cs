using System;
using System.Collections.Generic;
using Loretta.Lexing;
using Loretta.Parsing.Abstractions;

namespace Loretta.Parsing.Nodes.Variables
{
    public class AssignmentStatement : ASTStatement, IAssignmentNode, IEquatable<AssignmentStatement>
    {
        public List<ASTNode> Variables { get; } = new List<ASTNode> ( );

        public List<ASTNode> Assignments { get; } = new List<ASTNode> ( );

        public AssignmentStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        /// <summary>
        /// Adds a variable to this statment
        /// </summary>
        /// <param name="varExpr"></param>
        public void AddVariable ( ASTNode varExpr )
        {
            if ( varExpr == null )
                return;
            this.Variables.Add ( varExpr );
            this.AddChild ( varExpr );
        }

        /// <summary>
        /// Removes a variable from this statement
        /// </summary>
        /// <param name="varExpr"></param>
        public void RemoveVariable ( ASTNode varExpr )
        {
            this.Variables.Remove ( varExpr );
            this.RemoveChild ( varExpr );
        }

        /// <summary>
        /// Sets the list of variables clearing up the old ones
        /// </summary>
        /// <param name="vars"></param>
        public void SetVariables ( IEnumerable<ASTNode> vars )
        {
            foreach ( ASTNode var in this.Variables )
                this.RemoveChild ( var );
            this.Variables.Clear ( );
            foreach ( ASTNode var in vars )
                this.AddVariable ( var );
        }

        /// <summary>
        /// Adds an assignment to this statement
        /// </summary>
        /// <param name="node"></param>
        public void AddAssignment ( ASTNode node )
        {
            this.Assignments.Add ( node );
            this.AddChild ( node );
        }

        /// <summary>
        /// Removes an assignment from this statement
        /// </summary>
        /// <param name="node"></param>
        public void RemoveAssignment ( ASTNode node )
        {
            this.Assignments.Remove ( node );
            this.RemoveChild ( node );
        }

        /// <summary>
        /// Sets the list of assignments clearing up the old ones
        /// </summary>
        /// <param name="assignments"></param>
        public void SetAssignments ( IEnumerable<ASTNode> assignments )
        {
            foreach ( ASTNode assignment in this.Assignments )
                this.RemoveChild ( assignment );
            this.Assignments.Clear ( );
            foreach ( ASTNode assignment in assignments )
                this.AddAssignment ( assignment );
        }

        public override ASTNode Clone ( )
        {
            var stat = new AssignmentStatement ( this.Parent, this.Scope, this.CloneTokenList ( ) );

            foreach ( ASTNode var in this.Variables )
                stat.AddVariable ( var.Clone ( ) );
            foreach ( ASTNode ass in this.Assignments )
                stat.AddAssignment ( ass.Clone ( ) );

            return stat;
        }

        #region Generate Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as AssignmentStatement );
        }

        public Boolean Equals ( AssignmentStatement other )
        {
            if ( other == null || this.Variables.Count != other.Variables.Count
                || this.Assignments.Count != other.Assignments.Count )
                return false;
            for ( var i = 0; i < this.Variables.Count; i++ )
                if ( this.Variables[i] != other.Variables[i] )
                    return false;
            for ( var i = 0; i < this.Assignments.Count; i++ )
                if ( this.Assignments[i] != other.Assignments[i] )
                    return false;
            return true;
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = 2023133067;
            foreach ( ASTNode node in this.Variables )
                hashCode *= -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( node );
            foreach ( ASTNode node in this.Assignments )
                hashCode *= -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( node );
            return hashCode;
        }

        public static Boolean operator == ( AssignmentStatement statement1, AssignmentStatement statement2 )
            => EqualityComparer<AssignmentStatement>.Default.Equals ( statement1, statement2 );

        public static Boolean operator != ( AssignmentStatement statement1, AssignmentStatement statement2 )
            => !( statement1 == statement2 );

        #endregion Generate Code
    }
}
