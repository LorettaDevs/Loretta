using System;
using Loretta.Parsing.AST;

namespace Loretta.Parsing
{
    public class GotoLabel
    {
        public Scope Scope { get; private set; }
        public String Identifier { get; private set; }

        public GotoLabel ( Scope scope, String identifier )
        {
            this.Scope = scope;
            this.Identifier = identifier;
        }
    }
}
