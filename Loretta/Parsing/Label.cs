using System;
using System.Collections.Generic;
using Loretta.Parsing.Nodes;

namespace Loretta.Parsing
{
    public class Label : IEquatable<Label>
    {
        public Scope Scope { get; protected set; }

        public String Name { get; protected set; }

        public ASTNode Node { get; set; }

        public InternalData InternalData { get; } = new InternalData ( );

        public Label ( Scope scope, String name )
        {
            this.Scope = scope;
            this.Name = name;
        }

        public override String ToString ( ) => $"Label<'{this.Name}'>";

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as Label );
        }

        public Boolean Equals ( Label other )
        {
            return other != null &&
                    EqualityComparer<Scope>.Default.Equals ( this.Scope, other.Scope ) &&
                     this.Name == other.Name;
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = 819712150;
            hashCode = hashCode * -1521134295 + EqualityComparer<Scope>.Default.GetHashCode ( this.Scope );
            hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode ( this.Name );
            return hashCode;
        }

        public static Boolean operator == ( Label label1, Label label2 )
            => EqualityComparer<Label>.Default.Equals ( label1, label2 );

        public static Boolean operator != ( Label label1, Label label2 )
            => !( label1 == label2 );

        #endregion Generated Code
    }
}
