using System;
using System.Collections.Generic;
using Loretta.Lexing;
using Loretta.Parsing.Nodes.Variables;

namespace Loretta.Parsing.Nodes.Functions
{
    public class LocalFunctionStatement : ASTStatement
    {
        public List<ASTNode> Arguments { get; } = new List<ASTNode> ( );

        public VariableExpression Identifier { get; private set; }

        public StatementList Body { get; private set; }

        public LocalFunctionStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetIdentifier ( VariableExpression ident )
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

        public void SetModifiers ( IEnumerable<String> mods )
        {
            this.Modifiers.Clear ( );
            this.Modifiers.AddRange ( mods );
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
