using System;
using System.Collections.Generic;
using System.Text;

namespace Loretta.Parsing.Visitor.ReadWrite
{
    public class ReadWriteContainerProxy : ReadWriteContainer
    {
        private readonly ReadWriteContainer _proxied;

        public ReadWriteContainerProxy ( Object identifier, ReadWriteContainer proxied ) : base ( identifier )
        {
            this._proxied = proxied ?? throw new ArgumentNullException ( nameof ( proxied ) );
        }

        public ReadWriteContainerProxy ( Object identifier, ReadWriteContainer parent, ReadWriteContainer proxied ) : base ( identifier, parent )
        {
            this._proxied = proxied ?? throw new ArgumentNullException ( nameof ( proxied ) );
        }

        public override ReadWriteContainer AddContainer ( Object identifier ) => this._proxied.AddContainer ( identifier );

        public override ReadWriteContainer this[Object identifier] => this._proxied[identifier];
    }
}
