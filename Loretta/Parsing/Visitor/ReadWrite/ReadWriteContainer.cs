using System;
using System.Collections.Generic;
using Loretta.Parsing.AST;

namespace Loretta.Parsing.Visitor.ReadWrite
{
    public class ReadWriteContainer
    {
        private readonly Dictionary<Object, ReadWriteContainer> _containers = new Dictionary<Object, ReadWriteContainer> ( );
        private readonly List<Read> _reads = new List<Read> ( );
        private readonly List<Write> _writes = new List<Write> ( );

        public Object Identifier { get; }

        public Variable Variable { get; }

        public virtual ReadWriteContainer Parent { get; }

        public virtual IReadOnlyCollection<Read> Reads => this._reads;

        public virtual IReadOnlyCollection<Write> Writes => this._writes;

        public ReadWriteContainer ( Object identifier )
        {
            this.Identifier = identifier ?? throw new ArgumentNullException ( nameof ( identifier ) );
        }

        public ReadWriteContainer ( Object identifier, Variable variable ) : this ( identifier )
        {
            this.Variable = variable ?? throw new ArgumentNullException ( nameof ( variable ) );
        }

        public ReadWriteContainer ( Object identifier, ReadWriteContainer parent ) : this ( identifier )
        {
            this.Parent = parent ?? throw new ArgumentNullException ( nameof ( parent ) );
        }

        public ReadWriteContainer ( Object identifier, Variable variable, ReadWriteContainer parent ) : this ( identifier, variable )
        {
            this.Parent = parent ?? throw new ArgumentNullException ( nameof ( parent ) );
        }

        public virtual Read AddRead ( Boolean canCauseMutation, Expression node )
        {
            var read = new Read ( canCauseMutation, node );
            this._reads.Add ( read );
            return read;
        }

        public virtual Write AddWrite ( Boolean isUnconditional, Expression node, Expression value )
        {
            var write = new Write ( isUnconditional, node, value );
            this._writes.Add ( write );
            return write;
        }

        public virtual ReadWriteContainer AddContainer ( Object identifier )
        {
            var container = new ReadWriteContainer ( identifier, this );
            this._containers.Add ( identifier, container );
            return container;
        }

        public virtual ReadWriteContainerProxy AddContainerProxy ( Object identifier, ReadWriteContainer container )
        {
            var proxy = new ReadWriteContainerProxy ( identifier, this, container );
            this._containers.Add ( identifier, proxy );
            return proxy;
        }

        public virtual ReadWriteContainer this[Object identifier] => this._containers.TryGetValue ( identifier, out ReadWriteContainer value ) ? value : null;
    }
}