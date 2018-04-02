using System;
using System.Collections.Generic;
using Loretta.Lexing;
using Loretta.Parsing.Nodes.Variables;

namespace Loretta.Parsing.Nodes.Loops
{
    public class ForNumericStatement : ASTStatement, IEquatable<ForNumericStatement>
    {
        public VariableExpression Variable { get; private set; }

        public ASTNode InitialExpression { get; private set; }

        public ASTNode FinalExpression { get; private set; }

        public ASTNode IncrementExpression { get; private set; }

        public StatementList Body { get; private set; }

        public ForNumericStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetVariable ( VariableExpression incr )
        {
            if ( this.Variable != null )
                this.RemoveChild ( this.Variable );
            this.AddChild ( incr );
            this.Variable = incr;
        }

        public void SetInitialExpression ( ASTNode init )
        {
            if ( this.InitialExpression != null )
                this.RemoveChild ( this.InitialExpression );
            this.AddChild ( init );
            this.InitialExpression = init;
        }

        public void SetFinalExpression ( ASTNode final )
        {
            if ( this.FinalExpression != null )
                this.RemoveChild ( this.FinalExpression );
            this.AddChild ( final );
            this.FinalExpression = final;
        }

        public void SetIncrementExpression ( ASTNode incr )
        {
            if ( incr == null )
            {
                this.RemoveChild ( this.IncrementExpression );
                this.IncrementExpression = null;
            }
            if ( this.IncrementExpression != null )
                this.RemoveChild ( this.IncrementExpression );
            this.AddChild ( incr );
            this.IncrementExpression = incr;
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
            var fors = new ForNumericStatement ( this.Parent, this.Scope, this.CloneTokenList ( ) );
            fors.SetVariable ( ( VariableExpression ) this.Variable.Clone ( ) );
            fors.SetInitialExpression ( this.InitialExpression.Clone ( ) );
            fors.SetFinalExpression ( this.FinalExpression.Clone ( ) );
            if ( this.IncrementExpression != null )
                fors.SetIncrementExpression ( this.IncrementExpression.Clone ( ) );
            fors.SetBody ( ( StatementList ) this.Body.Clone ( ) );
            return fors;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as ForNumericStatement );
        }

        public Boolean Equals ( ForNumericStatement other )
        {
            return other != null &&
                    EqualityComparer<VariableExpression>.Default.Equals ( this.Variable, other.Variable ) &&
                    EqualityComparer<ASTNode>.Default.Equals ( this.InitialExpression, other.InitialExpression ) &&
                    EqualityComparer<ASTNode>.Default.Equals ( this.FinalExpression, other.FinalExpression ) &&
                    EqualityComparer<ASTNode>.Default.Equals ( this.IncrementExpression, other.IncrementExpression ) &&
                    EqualityComparer<StatementList>.Default.Equals ( this.Body, other.Body );
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = 1178557836;
            hashCode = hashCode * -1521134295 + EqualityComparer<VariableExpression>.Default.GetHashCode ( this.Variable );
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.InitialExpression );
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.FinalExpression );
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.IncrementExpression );
            hashCode = hashCode * -1521134295 + EqualityComparer<StatementList>.Default.GetHashCode ( this.Body );
            return hashCode;
        }

        public static Boolean operator == ( ForNumericStatement statement1, ForNumericStatement statement2 ) => EqualityComparer<ForNumericStatement>.Default.Equals ( statement1, statement2 );

        public static Boolean operator != ( ForNumericStatement statement1, ForNumericStatement statement2 ) => !( statement1 == statement2 );

        #endregion Generated Code
    }
}
