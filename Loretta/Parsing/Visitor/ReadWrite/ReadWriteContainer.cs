using System;
using System.Collections.Generic;
using System.Linq;
using Loretta.Parsing.AST;

namespace Loretta.Parsing.Visitor.ReadWrite
{
    public class ReadWriteContainer
    {
        private readonly Dictionary<Object, ReadWriteContainer> _containers = new Dictionary<Object, ReadWriteContainer> ( );
        private readonly List<Read> _reads = new List<Read> ( );
        private readonly List<Write> _writes = new List<Write> ( );

        public LuaASTNode Definer { get; }

        public Object? Identifier { get; }

        public Variable? Variable { get; }

        public virtual ReadWriteContainer? Parent { get; }

        public virtual IReadOnlyDictionary<Object, ReadWriteContainer> Containers => this._containers;

        public virtual IReadOnlyCollection<Read> Reads => this._reads;

        public virtual IReadOnlyCollection<Write> Writes => this._writes;

        public virtual Boolean HasConditionalWrites => this.Writes.Any ( write => !write.IsUnconditional );

        public virtual Boolean HasUndefinedWrites => this.Writes.Any ( write => write.Value is UndefinedValueExpression );

        public virtual Boolean HasIndirectWrites => this.Parent?.Reads.Any ( read => read.IsBeingAlised ) is true;

        public ReadWriteContainer ( LuaASTNode definer, Object identifier )
        {
            this.Definer = definer;
            this.Identifier = identifier ?? throw new ArgumentNullException ( nameof ( identifier ) );
        }

        public ReadWriteContainer ( LuaASTNode definer, Object identifier, Variable variable ) : this ( definer, identifier )
        {
            this.Variable = variable ?? throw new ArgumentNullException ( nameof ( variable ) );
        }

        public ReadWriteContainer ( LuaASTNode definer, Object identifier, ReadWriteContainer parent ) : this ( definer, identifier )
        {
            this.Parent = parent ?? throw new ArgumentNullException ( nameof ( parent ) );
        }

        public ReadWriteContainer ( LuaASTNode definer, Object identifier, Variable variable, ReadWriteContainer parent ) : this ( definer, identifier, variable )
        {
            this.Parent = parent ?? throw new ArgumentNullException ( nameof ( parent ) );
        }

        public virtual Read AddRead ( Boolean isBeingAliased, Expression node, Expression? alias )
        {
            var read = new Read ( isBeingAliased, node, alias );
            this._reads.Add ( read );
            return read;
        }

        public virtual Write AddWrite ( Boolean isUnconditional, Expression node, Expression value )
        {
            var write = new Write ( isUnconditional, node, value );
            this._writes.Add ( write );
            return write;
        }

        public virtual ReadWriteContainer AddContainer ( LuaASTNode definer, Object identifier )
        {
            var container = new ReadWriteContainer ( definer, identifier, this );
            this._containers.Add ( identifier, container );
            return container;
        }

        public virtual ReadWriteContainerProxy AddContainerProxy ( LuaASTNode definer, Object identifier, ReadWriteContainer container )
        {
            var proxy = new ReadWriteContainerProxy ( definer, identifier, this, container );
            this._containers.Add ( identifier, proxy );
            return proxy;
        }

        public virtual ReadWriteContainer? this[Object identifier] =>
            this._containers.TryGetValue ( identifier, out ReadWriteContainer value ) ? value : null;
    }
}