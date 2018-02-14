using System;
using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Functions
{
    public class AnonymousFunctionExpression : ASTNode
    {
        public List<ASTNode> Arguments { get; } = new List<ASTNode> ( );

        public StatementList Body { get; private set; }

        public AnonymousFunctionExpression ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetArguments ( IEnumerable<ASTNode> args )
        {
            this.Arguments.Clear ( );
            this.Arguments.AddRange ( args );
            this.AddChildren ( args );
        }

        public void AddArgument ( ASTNode arg )
        {
            this.Arguments.Add ( arg );
            this.AddChild ( arg );
        }

        public void SetBody ( StatementList body )
        {
            if ( this.Children.Contains ( body ) )
                this.RemoveChild ( body );
            this.AddChild ( body );

            this.Body = body;
        }
    }
}
