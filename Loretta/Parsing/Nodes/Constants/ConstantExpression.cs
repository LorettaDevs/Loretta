using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Constants
{
    public abstract class ConstantExpression<T> : ASTNode, IConstantNode<T>
    {
        public String Raw { get; protected set; }

        public T Value { get; protected set; }

        protected ConstantExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public override String ToString ( ) => $"{this.GetType ( ).Name}<{this.Raw} | {this.Value}>";
    }
}
