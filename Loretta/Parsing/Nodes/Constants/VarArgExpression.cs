using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Constants
{
    /// <summary>
    /// Even though this is in the constants namespace, this
    /// cannot be resolved unless we execute the code in some form
    /// or it might not be resolvable at all
    /// </summary>
    public class VarArgExpression : ASTNode, IEquatable<VarArgExpression>
    {
        public VarArgExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public override ASTNode Clone ( )
        {
            return new VarArgExpression ( this.Parent, this.Scope, this.CloneTokenList ( ) );
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as Eof );
        }

        public Boolean Equals ( VarArgExpression other )
        {
            return other != null;
        }

        public override Int32 GetHashCode ( )
        {
            return 1192400624;
        }

        public static Boolean operator == ( VarArgExpression eof1, VarArgExpression eof2 )
            => EqualityComparer<VarArgExpression>.Default.Equals ( eof1, eof2 );

        public static Boolean operator != ( VarArgExpression eof1, VarArgExpression eof2 )
            => !( eof1 == eof2 );

        #endregion Generated Code
    }
}
