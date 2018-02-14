using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Operators
{
    public class BinaryOperatorExpression : ASTNode
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
    }
}
