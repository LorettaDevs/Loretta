using System;

namespace Loretta.Parsing
{
    public class Variable
    {
        public Scope Scope { get; set; }

        public String Name { get; set; }

        public Boolean IsLocal { get; }

        public Object Value { get; set; }

        public InternalData InternalData { get; } = new InternalData ( );

        public Variable ( Scope scope, String name, Boolean isLocal )
        {
            this.Scope = scope;
            this.Name = name;
            this.IsLocal = isLocal;
        }
    }
}
