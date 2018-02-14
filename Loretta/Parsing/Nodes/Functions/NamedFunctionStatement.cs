using System;
using System.Collections.Generic;
using System.Linq;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Functions
{
    public class NamedFunctionStatement : ASTStatement
    {
        public List<ASTNode> Arguments { get; } = new List<ASTNode> ( );

        public ASTNode Identifier { get; private set; }

        public StatementList Body { get; private set; }

        public NamedFunctionStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetIdentifier ( ASTNode ident )
        {
            if ( this.Children.Contains ( ident ) )
                this.RemoveChild ( ident );
            this.AddChild ( ident );

            this.Identifier = ident;
        }

        public void SetArguments ( IEnumerable<ASTNode> args )
        {
            this.Arguments.Clear ( );
            this.Arguments.AddRange ( args );
            this.AddChildren ( args );
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
