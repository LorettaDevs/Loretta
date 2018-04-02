using System;
using System.Collections.Generic;
using GParse.Parsing.Errors;
using Loretta.Lexing;
using Loretta.Parsing.Abstractions;

namespace Loretta.Parsing.Nodes.Variables
{
    public class LocalVariableStatement : ASTStatement, IAssignmentNode, IEquatable<LocalVariableStatement>
    {
        public List<ASTNode> Variables { get; } = new List<ASTNode> ( );

        public List<ASTNode> Assignments { get; } = new List<ASTNode> ( );

        public LocalVariableStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        /// <summary>
        /// Adds a variable to this statment
        /// </summary>
        /// <param name="varExpr"></param>
        public void AddVariable ( VariableExpression varExpr )
        {
            if ( varExpr.Variable == null || !varExpr.Variable.IsLocal )
                throw new ParseException ( this.Tokens[0].Range.Start, "Non local variable added to LocalVariableStatement" );

            this.Variables.Add ( varExpr );
            this.AddChild ( varExpr );
        }

        /// <summary>
        /// Remove variable
        /// </summary>
        /// <param name="expr"></param>
        public void RemoveVariable ( VariableExpression expr )
        {
            this.Variables.Remove ( expr );
            this.RemoveChild ( expr );
        }

        /// <summary>
        /// Sets the list of variables clearing up the old ones
        /// </summary>
        /// <param name="variables"></param>
        public void SetVariables ( IEnumerable<ASTNode> variables )
        {
            foreach ( ASTNode variable in this.Variables )
                this.RemoveChild ( variable );
            this.Variables.Clear ( );

            foreach ( VariableExpression variable in variables )
                this.AddVariable ( variable );
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
        /// Removes an assignment
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
            var stat = new LocalVariableStatement ( this.Parent, this.Scope, this.CloneTokenList ( ) );

            foreach ( VariableExpression var in this.Variables )
                stat.AddVariable ( ( VariableExpression ) var.Clone ( ) );
            foreach ( ASTNode ass in this.Assignments )
                stat.AddAssignment ( ass.Clone ( ) );

            return stat;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as LocalVariableStatement );
        }

        public Boolean Equals ( LocalVariableStatement other )
        {
            if ( other == null || this.Variables.Count != other.Variables.Count || this.Assignments.Count != other.Assignments.Count )
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

        public static Boolean operator == ( LocalVariableStatement statement1, LocalVariableStatement statement2 ) => EqualityComparer<LocalVariableStatement>.Default.Equals ( statement1, statement2 );

        public static Boolean operator != ( LocalVariableStatement statement1, LocalVariableStatement statement2 ) => !( statement1 == statement2 );

        #endregion Generated Code
    }
}
