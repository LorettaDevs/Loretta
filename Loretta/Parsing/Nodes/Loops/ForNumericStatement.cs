using System;
using System.Collections.Generic;
using System.Text;
using Loretta.Lexing;
using Loretta.Parsing.Nodes.Variables;

namespace Loretta.Parsing.Nodes.Loops
{
    public class ForNumericStatement : ASTStatement
    {
        public VariableExpression IncrementVariable { get; private set; }

        public ASTNode InitialExpression { get; private set; }

        public ASTNode FinalExpression { get; private set; }

        public ASTNode IncrementExpression { get; private set; }

        public StatementList Body { get; private set; }

        public ForNumericStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetIncrementVariable ( VariableExpression incr )
        {
            if ( this.IncrementVariable != null )
                this.RemoveChild ( this.IncrementVariable );
            this.AddChild ( incr );
            this.IncrementVariable = incr;
        }

        public void SetInitialExpression ( ASTNode init )
        {
            if ( this.InitialExpression != null )
                this.RemoveChild ( this.InitialExpression );
            this.AddChild ( init );
            this.InitialExpression = init;
        }

        public void SetFinalExpression ( ASTNode final )
        {
            if ( this.FinalExpression != null )
                this.RemoveChild ( this.FinalExpression );
            this.AddChild ( final );
            this.FinalExpression = final;
        }

        public void SetIncrementExpression ( ASTNode incr )
        {
            if ( this.IncrementExpression != null )
                this.RemoveChild ( this.IncrementExpression );
            this.AddChild ( incr );
            this.IncrementExpression = incr;
        }

        public void SetBody ( StatementList body )
        {
            if ( this.Body != null )
                this.RemoveChild ( this.Body );
            this.AddChild ( body );
            this.Body = body;
        }
    }
}
