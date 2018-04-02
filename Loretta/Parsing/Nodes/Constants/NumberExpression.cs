using System;
using System.Collections.Generic;
using GParse.Lexing;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Constants
{
    public class NumberExpression : ConstantExpression<Double>
    {
        public NumberExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
            if ( this.Tokens.Count != 1 || this.Tokens[0].Type != TokenType.Number )
                throw new Exception ( "A NumberExpression should only be composed of a single LToken of type Number" );

            this.Raw = this.Tokens[0].Raw;
            this.Value = ( Double ) this.Tokens[0].Value;
        }

        public override ASTNode Clone ( )
        {
            return new NumberExpression ( this.Parent, this.Scope, this.CloneTokenList ( ) );
        }
    }
}
