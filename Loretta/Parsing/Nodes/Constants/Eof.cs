using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Constants
{
    public class Eof : ASTNode, IEquatable<Eof>
    {
        public Eof ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public override ASTNode Clone ( )
        {
            return new Eof ( this.Parent, this.Scope, this.CloneTokenList ( ) );
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as Eof );
        }

        public Boolean Equals ( Eof other )
        {
            return other != null;
        }

        public override Int32 GetHashCode ( )
        {
            return 1596410344;
        }

        public static Boolean operator == ( Eof eof1, Eof eof2 )
            => EqualityComparer<Eof>.Default.Equals ( eof1, eof2 );

        public static Boolean operator != ( Eof eof1, Eof eof2 )
            => !( eof1 == eof2 );

        #endregion Generated Code
    }
}
