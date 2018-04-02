using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Constants
{
    public class ParenthesisExpression : ASTNode, IEquatable<ParenthesisExpression>
    {
        public ASTNode Expression { get; private set; }

        public ParenthesisExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
            this.Expression = null;
        }

        public void SetExpression ( ASTNode node )
        {
            this.Expression = node;
            this.AddChild ( node );
        }

        public override ASTNode Clone ( )
        {
            var paren = new ParenthesisExpression ( this.Parent, this.Scope, this.CloneTokenList ( ) );
            paren.SetExpression ( this.Expression.Clone ( ) );
            return paren;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as ParenthesisExpression );
        }

        public Boolean Equals ( ParenthesisExpression other )
        {
            return other != null &&
                    EqualityComparer<ASTNode>.Default.Equals ( this.Expression, other.Expression );
        }

        public override Int32 GetHashCode ( )
        {
            return -1489834557 + EqualityComparer<ASTNode>.Default.GetHashCode ( this.Expression );
        }

        public static Boolean operator == ( ParenthesisExpression expression1, ParenthesisExpression expression2 )
            => EqualityComparer<ParenthesisExpression>.Default.Equals ( expression1, expression2 );

        public static Boolean operator != ( ParenthesisExpression expression1, ParenthesisExpression expression2 )
            => !( expression1 == expression2 );

        #endregion Generated Code
    }
}
