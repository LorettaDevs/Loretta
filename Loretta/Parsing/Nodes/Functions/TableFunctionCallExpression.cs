using System;
using System.Collections.Generic;
using System.Text;
using Loretta.Lexing;
using Loretta.Parsing.Nodes.Constants;

namespace Loretta.Parsing.Nodes.Functions
{
    public class TableFunctionCallExpression : ASTStatement
    {
        public TableConstructorExpression Argument { get; private set; } = null;

        public ASTNode Base { get; private set; } = null;

        public TableFunctionCallExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetArgument ( TableConstructorExpression arg )
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
