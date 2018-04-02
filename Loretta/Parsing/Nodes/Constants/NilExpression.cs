using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Constants
{
    public class NilExpression : ConstantExpression<Object>
    {
        public static readonly NilExpression Nil = new NilExpression ( null, null, new List<LToken> ( new[] {
            new LToken ( "nil", "nil", "nil", GParse.Lexing.TokenType.Keyword, GParse.Lexing.SourceRange.Zero )
        } ) );

        public NilExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
            if ( this.Tokens.Count != 1 || this.Tokens[0].ID != "nil" )
                throw new Exception ( "A NilExpression should be composed of only a single LToken of id 'nil'" );

            this.Raw = "nil";
            this.Value = null;
        }

        public override ASTNode Clone ( )
        {
            return new NilExpression ( this.Parent, this.Scope, this.CloneTokenList ( ) );
        }
    }
}
