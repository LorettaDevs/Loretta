using System;
using Loretta.Parsing.AST;

namespace Loretta.Parsing.Visitor.ReadWrite
{
    public readonly struct Write
    {
        public Boolean IsUnconditional { get; }
        public Expression Node { get; }
        public Expression Value { get; }

        public Write ( Boolean isUnconditional, Expression node, Expression value )
        {
            this.IsUnconditional = isUnconditional;
            this.Node = node ?? throw new System.ArgumentNullException ( nameof ( node ) );
            this.Value = value ?? throw new System.ArgumentNullException ( nameof ( value ) );
        }
    }
}