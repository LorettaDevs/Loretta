using System;
using System.Collections.Generic;
using Loretta.Parsing.AST;

namespace Loretta.Parsing.Visitor.ReadWrite
{
    public class ReadWriteContainerProxy : ReadWriteContainer
    {
        private readonly ReadWriteContainer _proxied;

        public override IReadOnlyDictionary<Object, ReadWriteContainer> Containers => this._proxied.Containers;

        public override IReadOnlyCollection<Read> Reads => this._proxied.Reads;

        public override IReadOnlyCollection<Write> Writes => this._proxied.Writes;

        public override Boolean HasConditionalWrites => this._proxied.HasConditionalWrites;

        public override Boolean HasUndefinedWrites => this._proxied.HasUndefinedWrites;

        public override Boolean HasIndirectWrites => base.HasIndirectWrites;

        public ReadWriteContainerProxy ( LuaASTNode definer, Object identifier, ReadWriteContainer proxied ) : base ( definer, identifier )
        {
            this._proxied = proxied ?? throw new ArgumentNullException ( nameof ( proxied ) );
        }

        public ReadWriteContainerProxy ( LuaASTNode definer, Object identifier, ReadWriteContainer parent, ReadWriteContainer proxied ) : base ( definer, identifier, parent )
        {
            this._proxied = proxied ?? throw new ArgumentNullException ( nameof ( proxied ) );
        }

        public override ReadWriteContainer AddContainer ( LuaASTNode definer, Object identifier ) =>
            this._proxied.AddContainer ( definer, identifier );

        public override ReadWriteContainerProxy AddContainerProxy ( LuaASTNode definer, Object identifier, ReadWriteContainer container ) =>
            this._proxied.AddContainerProxy ( definer, identifier, container );

        public override Read AddRead ( Boolean isArgument, Expression node, Expression? alias ) =>
            this._proxied.AddRead ( isArgument, node, alias );

        public override Write AddWrite ( Boolean isUnconditional, Expression node, Expression value ) =>
            this._proxied.AddWrite ( isUnconditional, node, value );

        public override ReadWriteContainer? this[Object identifier] => this._proxied[identifier];
    }
}