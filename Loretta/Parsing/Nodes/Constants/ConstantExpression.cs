using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Constants
{
    public abstract class ConstantExpression : ASTNode
    {
        public String Raw;

        protected ConstantExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }
    }

    public abstract class ConstantExpression<T> : ConstantExpression, IConstantNode<T>, IEquatable<ConstantExpression<T>>
    {
        public T Value { get; protected set; }

        String IConstantNode<T>.Raw => this.Raw;

        protected ConstantExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public override String ToString ( ) => $"{this.GetType ( ).Name}<{this.Raw} | {this.Value}>";

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as ConstantExpression<T> );
        }

        public Boolean Equals ( ConstantExpression<T> other )
        {
            return other != null &&
                    EqualityComparer<T>.Default.Equals ( this.Value, other.Value ) &&
                    this.Raw == other.Raw;
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = 371763517;
            hashCode = hashCode * -1521134295 + EqualityComparer<T>.Default.GetHashCode ( this.Value );
            hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode ( this.Raw );
            return hashCode;
        }

        public static Boolean operator == ( ConstantExpression<T> expression1, ConstantExpression<T> expression2 )
            => EqualityComparer<ConstantExpression<T>>.Default.Equals ( expression1, expression2 );

        public static Boolean operator != ( ConstantExpression<T> expression1, ConstantExpression<T> expression2 )
            => !( expression1 == expression2 );

        #endregion Generated Code
    }
}
