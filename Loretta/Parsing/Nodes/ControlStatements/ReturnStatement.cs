using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.ControlStatements
{
    public class ReturnStatement : ASTStatement, IEquatable<ReturnStatement>
    {
        public List<ASTNode> Returns { get; } = new List<ASTNode> ( );

        public ReturnStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void AddReturn ( ASTNode value )
        {
            this.AddChild ( value );
            this.Returns.Add ( value );
        }

        public void SetReturns ( IEnumerable<ASTNode> returns )
        {
            this.Children.Clear ( );
            this.Returns.Clear ( );
            foreach ( ASTNode ret in returns )
                this.AddReturn ( ret );
        }

        public override ASTNode Clone ( )
        {
            var ret = new ReturnStatement ( this.Parent, this.Scope, this.CloneTokenList ( ) );
            foreach ( ASTNode val in this.Returns )
                ret.AddReturn ( val.Clone ( ) );
            return ret;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as ReturnStatement );
        }

        public Boolean Equals ( ReturnStatement other )
        {
            if ( other == null || this.Returns.Count != other.Returns.Count )
                return false;

            for ( var i = 0; i < this.Returns.Count; i++ )
                if ( this.Returns[i] != other.Returns[i] )
                    return false;
            return true;
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = -999256988;
            foreach ( ASTNode node in this.Returns )
                hashCode = hashCode + node.GetHashCode ( );
            return hashCode;
        }

        public static Boolean operator == ( ReturnStatement statement1, ReturnStatement statement2 ) => EqualityComparer<ReturnStatement>.Default.Equals ( statement1, statement2 );

        public static Boolean operator != ( ReturnStatement statement1, ReturnStatement statement2 ) => !( statement1 == statement2 );

        #endregion Generated Code
    }
}
