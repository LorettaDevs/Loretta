using System;
using System.Collections.Generic;
using System.Text;
using Loretta.Parsing.AST;

namespace Loretta.Parsing.Visitor.ReadWrite
{
    public readonly struct Read
    {
        public Boolean CanCauseMutation { get; }
        public Expression Node { get; }

        public Read ( Boolean canCauseMutation, Expression node )
        {
            this.CanCauseMutation = canCauseMutation;
            this.Node = node ?? throw new ArgumentNullException ( nameof ( node ) );
        }
    }
}
