using System;
using Loretta.Parsing.AST;

namespace Loretta.Parsing.Visitor.ReadWrite
{
    public readonly struct Read
    {
        public Boolean IsBeingAlised { get; }
        public Expression Node { get; }
        public Expression? Alias { get; }

        public Read ( Boolean isBeingAliased, Expression node, Expression? alias )
        {
            this.IsBeingAlised = isBeingAliased;
            this.Node = node ?? throw new ArgumentNullException ( nameof ( node ) );
            this.Alias = alias;
        }
    }
}