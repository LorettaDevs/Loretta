using System;
using System.Collections.Generic;
using Loretta.Lexing;
using Loretta.Parsing.Nodes.Variables;

namespace Loretta.Parsing.Nodes.Loops
{
    public class ForGenericStatement : ASTStatement, IEquatable<ForGenericStatement>
    {
        public List<VariableExpression> Variables { get; } = new List<VariableExpression> ( );

        public List<ASTNode> Generators { get; } = new List<ASTNode> ( );

        public StatementList Body { get; private set; }

        public ForGenericStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void AddVariable ( VariableExpression var )
        {
            this.AddChild ( var );
            this.Variables.Add ( var );
        }

        public void RemoveVariable ( VariableExpression var )
        {
            this.RemoveChild ( var );
            this.Variables.Remove ( var );
        }

        public void SetVariables ( IEnumerable<ASTNode> variables )
        {
            foreach ( VariableExpression variable in this.Variables )
                this.RemoveChild ( variable );
            this.Variables.Clear ( );
            foreach ( VariableExpression variable in variables )
                this.AddVariable ( variable );
        }

        public void AddGenerator ( ASTNode gen )
        {
            this.AddChild ( gen );
            this.Generators.Add ( gen );
        }

        public void RemoveGenerator ( ASTNode gen )
        {
            this.RemoveChild ( gen );
            this.Generators.Remove ( gen );
        }

        public void SetGenerators ( IEnumerable<ASTNode> gens )
        {
            foreach ( ASTNode gen in this.Generators )
                this.RemoveChild ( gen );
            this.Generators.Clear ( );
            foreach ( ASTNode gen in gens )
                this.AddGenerator ( gen );
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
            var forgen = new ForGenericStatement ( this.Parent, this.Scope, this.CloneTokenList ( ) );
            foreach ( VariableExpression var in this.Variables )
                forgen.AddVariable ( ( VariableExpression ) var.Clone ( ) );
            foreach ( ASTNode gen in this.Generators )
                forgen.AddGenerator ( gen.Clone ( ) );
            forgen.SetBody ( ( StatementList ) this.Body.Clone ( ) );
            return forgen;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as ForGenericStatement );
        }

        public Boolean Equals ( ForGenericStatement other )
        {
            if ( other == null || !EqualityComparer<StatementList>.Default.Equals ( this.Body, other.Body )
                || this.Variables.Count != other.Variables.Count || this.Generators.Count != other.Generators.Count )
                return false;
            for ( var i = 0; i < this.Variables.Count; i++ )
                if ( this.Variables[i] != other.Variables[i] )
                    return false;
            for ( var i = 0; i < this.Generators.Count; i++ )
                if ( this.Generators[i] != other.Generators[i] )
                    return false;
            return true;
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = 105265490;
            foreach ( VariableExpression var in this.Variables )
                hashCode *= -1521134295 + EqualityComparer<VariableExpression>.Default.GetHashCode ( var );
            foreach ( ASTNode gen in this.Generators )
                hashCode *= -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( gen );
            hashCode = hashCode * -1521134295 + EqualityComparer<StatementList>.Default.GetHashCode ( this.Body );
            return hashCode;
        }

        public static Boolean operator == ( ForGenericStatement statement1, ForGenericStatement statement2 ) => EqualityComparer<ForGenericStatement>.Default.Equals ( statement1, statement2 );

        public static Boolean operator != ( ForGenericStatement statement1, ForGenericStatement statement2 ) => !( statement1 == statement2 );

        #endregion Generated Code
    }
}
