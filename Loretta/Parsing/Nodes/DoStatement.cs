using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes
{
    // simply an excuse to create a new scope
    public class DoStatement : ASTStatement, IEquatable<DoStatement>
    {
        public StatementList Body { get; private set; }

        public DoStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetBody ( StatementList body )
        {
            if ( this.Body != null )
                this.RemoveChild ( this.Body );
            this.AddChild ( body );
            this.Body = body;
        }

        public override ASTNode Clone ( )
        {
            var dos = new DoStatement ( this.Parent, this.Scope, this.CloneTokenList ( ) );
            dos.SetBody ( ( StatementList ) this.Body.Clone ( ) );
            return dos;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as DoStatement );
        }

        public Boolean Equals ( DoStatement other )
        {
            return other != null &&
                    EqualityComparer<StatementList>.Default.Equals ( this.Body, other.Body );
        }

        public override Int32 GetHashCode ( )
        {
            return 169287901 + EqualityComparer<StatementList>.Default.GetHashCode ( this.Body );
        }

        public static Boolean operator == ( DoStatement statement1, DoStatement statement2 ) => EqualityComparer<DoStatement>.Default.Equals ( statement1, statement2 );

        public static Boolean operator != ( DoStatement statement1, DoStatement statement2 ) => !( statement1 == statement2 );

        #endregion Generated Code
    }
}
