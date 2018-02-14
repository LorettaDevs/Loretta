using System;
using System.Collections.Generic;
using System.Text;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Constants
{
    public class NilExpression : ConstantExpression<Object>
    {
        public NilExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
            if ( this.Tokens.Count != 1 || this.Tokens[0].ID != "nil" )
                throw new Exception ( "A NilExpression should be composed of only a single LToken of id 'nil'" );

            this.Raw = "nil";
            this.Value = null;
        }
    }
}
