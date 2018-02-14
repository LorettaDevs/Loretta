using System;
using Loretta.Parsing.Nodes;

namespace Loretta.Parsing
{
    public class Label
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
    }
}
