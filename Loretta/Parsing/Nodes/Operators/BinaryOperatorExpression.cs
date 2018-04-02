using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Operators
{
    public class BinaryOperatorExpression : ASTNode, IEquatable<BinaryOperatorExpression>
    {
        public ASTNode LeftOperand { get; private set; }

        public ASTNode RightOperand { get; private set; }

        public String Operator { get; set; }

        public BinaryOperatorExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetLeftOperand ( ASTNode expr )
        {
            if ( this.LeftOperand != null )
                this.RemoveChild ( this.LeftOperand );
            this.AddChild ( expr );

            this.LeftOperand = expr;
        }

        public void SetRightOperand ( ASTNode expr )
        {
            if ( this.RightOperand != null )
                this.RemoveChild ( this.RightOperand );
            this.AddChild ( expr );

            this.RightOperand = expr;
        }

        public override ASTNode Clone ( )
        {
            var binop = new BinaryOperatorExpression ( this.Parent, this.Scope, this.CloneTokenList ( ) );
            binop.SetLeftOperand ( this.LeftOperand.Clone ( ) );
            binop.SetRightOperand ( this.RightOperand.Clone ( ) );
            binop.Operator = this.Operator;
            return binop;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as BinaryOperatorExpression );
        }

        public Boolean Equals ( BinaryOperatorExpression other )
        {
            return other != null &&
                    EqualityComparer<ASTNode>.Default.Equals ( this.LeftOperand, other.LeftOperand ) &&
                    EqualityComparer<ASTNode>.Default.Equals ( this.RightOperand, other.RightOperand ) &&
                     this.Operator == other.Operator;
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = 247324312;
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.LeftOperand );
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.RightOperand );
            hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode ( this.Operator );
            return hashCode;
        }

        public static Boolean operator == ( BinaryOperatorExpression expression1, BinaryOperatorExpression expression2 ) => EqualityComparer<BinaryOperatorExpression>.Default.Equals ( expression1, expression2 );

        public static Boolean operator != ( BinaryOperatorExpression expression1, BinaryOperatorExpression expression2 ) => !( expression1 == expression2 );

        #endregion Generated Code
    }
}
