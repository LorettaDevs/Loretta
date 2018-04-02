using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Operators
{
    public class UnaryOperatorExpression : ASTNode, IEquatable<UnaryOperatorExpression>
    {
        public ASTNode Operand { get; private set; }

        public String Operator { get; set; }

        public UnaryOperatorExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetOperand ( ASTNode expr )
        {
            if ( this.Operand != null )
                this.RemoveChild ( this.Operand );
            this.AddChild ( expr );

            this.Operand = expr;
        }

        public override ASTNode Clone ( )
        {
            var un = new UnaryOperatorExpression ( this.Parent, this.Scope, this.CloneTokenList ( ) )
            {
                Operator = this.Operator
            };
            un.SetOperand ( this.Operand.Clone ( ) );
            return un;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as UnaryOperatorExpression );
        }

        public Boolean Equals ( UnaryOperatorExpression other )
        {
            return other != null &&
                    EqualityComparer<ASTNode>.Default.Equals ( this.Operand, other.Operand ) &&
                     this.Operator == other.Operator;
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = -1368162997;
            hashCode = hashCode * -1521134295 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.Operand );
            hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode ( this.Operator );
            return hashCode;
        }

        public static Boolean operator == ( UnaryOperatorExpression expression1, UnaryOperatorExpression expression2 ) => EqualityComparer<UnaryOperatorExpression>.Default.Equals ( expression1, expression2 );

        public static Boolean operator != ( UnaryOperatorExpression expression1, UnaryOperatorExpression expression2 ) => !( expression1 == expression2 );

        #endregion Generated Code
    }
}
