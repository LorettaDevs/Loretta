using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Constants
{
    public class StringExpression : ConstantExpression<String>
    {
        public StringExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
            if ( this.Tokens.Count != 1 )
                throw new Exception ( "A StringExpression should only be composed of a single LToken of type String." );

            this.Raw = this.Tokens[0].Raw;
            this.Value = ( String ) this.Tokens[0].Value;
        }
    }
}
