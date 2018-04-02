using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.ControlStatements
{
    public class BreakStatement : ASTStatement, IEquatable<BreakStatement>
    {
        public BreakStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public override ASTNode Clone ( )
        {
            return new BreakStatement ( this.Parent, this.Scope, this.CloneTokenList ( ) );
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as BreakStatement );
        }

        public Boolean Equals ( BreakStatement other )
        {
            return other != null;
        }

        public override Int32 GetHashCode ( )
        {
            return 1179400522;
        }

        public static Boolean operator == ( BreakStatement lhs, BreakStatement rhs )
            => EqualityComparer<BreakStatement>.Default.Equals ( lhs, rhs );

        public static Boolean operator != ( BreakStatement lhs, BreakStatement rhs )
            => !( lhs == rhs );

        #endregion Generated Code
    }
}
