using System;

namespace Loretta.Parsing.Visitor.ReadWrite
{
    public sealed class GlobalTable : ReadWriteContainer
    {
        public override ReadWriteContainer Parent => this;

        public GlobalTable ( Object identifier ) : base ( null!, identifier )
        {
            this.AddContainerProxy ( null!, identifier, this );
        }
    }
}