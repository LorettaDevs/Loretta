using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Operators
{
    public class UnaryOperatorExpression : ASTNode
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
    }
}
