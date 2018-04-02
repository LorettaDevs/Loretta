using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Constants
{
    public class BooleanExpression : ConstantExpression<Boolean>
    {
        public BooleanExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
            if ( this.Tokens.Count != 1 || ( this.Tokens[0].ID != "true" && this.Tokens[0].ID != "false" ) )
                throw new Exception ( "A BooleanExpression should only be composed of a single LToken of ID 'false' or 'true'" );

            this.Raw = this.Tokens[0].Raw;
            this.Value = this.Tokens[0].Raw == "true";
        }

        public override ASTNode Clone ( )
        {
            return new BooleanExpression ( this.Parent, this.Scope, this.CloneTokenList ( ) );
        }
    }
}
