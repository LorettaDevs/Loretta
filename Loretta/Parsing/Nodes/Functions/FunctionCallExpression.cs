using System;
using System.Collections.Generic;
using System.Text;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Functions
{
    public class FunctionCallExpression : ASTStatement
    {
        public IList<ASTNode> Arguments { get; } = new List<ASTNode> ( );

        public ASTNode Base { get; private set; } = null;

        public FunctionCallExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void AddArgument ( ASTNode expr )
        {
            this.Arguments.Add ( expr );
            this.AddChild ( expr );
        }

        public void SetBase ( ASTNode @base )
        {
            if ( this.Children.Contains ( @base ) )
                this.RemoveChild ( @base );
            this.AddChild ( @base );

            this.Base = @base;
        }
    }
}
