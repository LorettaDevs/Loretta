using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.ControlStatements
{
    public class GotoLabelStatement : ASTStatement, IEquatable<GotoLabelStatement>
    {
        public Label Label { get; set; }

        public GotoLabelStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public override ASTNode Clone ( )
        {
            return new GotoLabelStatement ( this.Parent, this.Scope, this.CloneTokenList ( ) )
            {
                Label = this.Label
            };
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as GotoLabelStatement );
        }

        public Boolean Equals ( GotoLabelStatement other )
        {
            return other != null &&
                    EqualityComparer<Label>.Default.Equals ( this.Label, other.Label );
        }

        public override Int32 GetHashCode ( )
        {
            return 981597221 + EqualityComparer<Label>.Default.GetHashCode ( this.Label );
        }

        public static Boolean operator == ( GotoLabelStatement statement1, GotoLabelStatement statement2 ) => EqualityComparer<GotoLabelStatement>.Default.Equals ( statement1, statement2 );

        public static Boolean operator != ( GotoLabelStatement statement1, GotoLabelStatement statement2 ) => !( statement1 == statement2 );

        #endregion Generated Code
    }
}
