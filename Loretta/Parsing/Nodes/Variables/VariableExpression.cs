using System;
using System.Collections.Generic;
using Loretta.Lexing;
using Loretta.Parsing.Abstractions;

namespace Loretta.Parsing.Nodes.Variables
{
    public class VariableExpression : ASTNode, IVariableContainer, IEquatable<VariableExpression>
    {
        public Variable Variable { get; set; }

        public VariableExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public override ASTNode Clone ( )
        {
            return new VariableExpression ( this.Parent, this.Scope, this.CloneTokenList ( ) )
            {
                Variable = this.Variable
            };
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as VariableExpression );
        }

        public Boolean Equals ( VariableExpression other )
        {
            return other != null &&
                    EqualityComparer<Variable>.Default.Equals ( this.Variable, other.Variable );
        }

        public override Int32 GetHashCode ( )
        {
            return 410573293 + EqualityComparer<Variable>.Default.GetHashCode ( this.Variable );
        }

        public static Boolean operator == ( VariableExpression expression1, VariableExpression expression2 ) => EqualityComparer<VariableExpression>.Default.Equals ( expression1, expression2 );

        public static Boolean operator != ( VariableExpression expression1, VariableExpression expression2 ) => !( expression1 == expression2 );

        #endregion Generated Code
    }
}
