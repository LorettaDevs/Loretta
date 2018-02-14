using System;
using System.Collections.Generic;
using System.Text;
using Loretta.Lexing;
using Loretta.Parsing.Nodes.Constants;

namespace Loretta.Parsing.Nodes.Functions
{
    public class StringFunctionCallExpression : ASTStatement
    {
        public ASTNode Base { get; private set; } = null;

        public StringExpression Argument { get; private set; }

        public StringFunctionCallExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetArgument ( StringExpression arg )
        {
            if ( this.Argument != null )
                this.RemoveChild ( this.Argument );
            this.AddChild ( arg );
            this.Argument = arg;
        }

        public void SetBase ( ASTNode @base )
        {
            if ( this.Base != null )
                this.RemoveChild ( this.Base );
            this.AddChild ( @base );
            this.Base = @base;
        }
    }
}
