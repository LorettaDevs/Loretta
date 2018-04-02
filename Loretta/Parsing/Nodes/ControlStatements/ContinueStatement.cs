using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.ControlStatements
{
    public class ContinueStatement : ASTStatement, IEquatable<ContinueStatement>
    {
        public ContinueStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public override ASTNode Clone ( )
        {
            return new ContinueStatement ( this.Parent, this.Scope, this.CloneTokenList ( ) );
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as ContinueStatement );
        }

        public Boolean Equals ( ContinueStatement other )
        {
            return other != null;
        }

        public override Int32 GetHashCode ( )
        {
            return 1969206592;
        }

        public static Boolean operator == ( ContinueStatement lhs, ContinueStatement rhs )
            => EqualityComparer<ContinueStatement>.Default.Equals ( lhs, rhs );

        public static Boolean operator != ( ContinueStatement lhs, ContinueStatement rhs )
            => !( lhs == rhs );

        #endregion Generated Code
    }
}
