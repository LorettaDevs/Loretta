using System;
using System.Collections.Generic;

namespace Loretta.Parsing
{
    public class Variable : IEquatable<Variable>
    {
        public Scope Scope { get; set; }

        public String Name { get; set; }

        public Boolean IsLocal { get; }

        public InternalData InternalData { get; } = new InternalData ( );

        public Variable ( Scope scope, String name, Boolean isLocal )
        {
            this.Scope = scope;
            this.Name = name;
            this.IsLocal = isLocal;
        }

        #region Generated Code

        public override Boolean Equals ( Object obj )
        {
            return this.Equals ( obj as Variable );
        }

        public Boolean Equals ( Variable other )
        {
            return other != null &&
                    EqualityComparer<Scope>.Default.Equals ( this.Scope, other.Scope ) &&
                     this.Name == other.Name &&
                     this.IsLocal == other.IsLocal;
        }

        public override Int32 GetHashCode ( )
        {
            var hashCode = 585857577;
            hashCode = hashCode * -1521134295 + EqualityComparer<Scope>.Default.GetHashCode ( this.Scope );
            hashCode = hashCode * -1521134295 + EqualityComparer<String>.Default.GetHashCode ( this.Name );
            hashCode = hashCode * -1521134295 + this.IsLocal.GetHashCode ( );
            return hashCode;
        }

        public static Boolean operator == ( Variable variable1, Variable variable2 ) => EqualityComparer<Variable>.Default.Equals ( variable1, variable2 );

        public static Boolean operator != ( Variable variable1, Variable variable2 ) => !( variable1 == variable2 );

        #endregion Generated Code
    }
}
