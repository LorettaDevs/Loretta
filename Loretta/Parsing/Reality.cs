using System;
using System.Collections.Generic;
using System.Text;
using Loretta.Parsing.Nodes;

namespace Loretta.Parsing
{
    public class Reality
    {
        public InternalData InternalData { get; } = new InternalData ( );

        public GLuaParser Parser { get; }

        public Scope Scope { get; }

        public Reality Parent { get; }

        public Boolean IsUncertain { get; }

        public IEnumerable<Reality> Children => this._Children.AsReadOnly ( );

        private readonly List<Reality> SubRealities = new List<Reality> ( );
        private readonly List<Reality> _Children = new List<Reality> ( );
        private readonly Dictionary<ASTNode, InternalData> NodeData = new Dictionary<ASTNode, InternalData> ( );

        public Reality ( GLuaParser parser, Scope scope, Reality parentReality = null, Boolean? isUncertain = null )
        {
            this.Parser = parser;
            this.Scope = scope;
            this.Parent = parentReality;
            this.IsUncertain = isUncertain ?? parentReality?.IsUncertain ?? false;

            this.Parent?._Children.Add ( this );
        }

        public void Start ( )
        {
            this.Parser.PushReality ( this );
        }

        public void Finish ( )
        {
            foreach ( Reality subReality in this.SubRealities )
                subReality.Finish ( );

            if ( this.Parser.PopReality ( ) != this )
                throw new Exception ( "Reality popped from parser != this, shitcode alert." );
        }
    }
}
