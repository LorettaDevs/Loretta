using System;
using System.Collections.Generic;
using System.Text;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.IfStatements
{
    public class IfStatement : ASTStatement
    {
        public IfClause MainClause { get; private set; }

        public List<IfClause> ElseIfClauses { get; } = new List<IfClause> ( );

        public StatementList ElseBlock { get; private set; }

        public IfStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        public void SetMainClause ( IfClause main )
        {
            if ( this.MainClause != null )
                this.RemoveChild ( this.MainClause );
            this.AddChild ( main );
            this.MainClause = main;
        }

        public void AddElseIfClause ( IfClause elif )
        {
            this.AddChild ( elif );
            this.ElseIfClauses.Add ( elif );
        }

        public void SetElseBlock ( StatementList elseb )
        {
            if ( this.ElseBlock != null )
                this.RemoveChild ( this.ElseBlock );
            this.AddChild ( elseb );
            this.ElseBlock = elseb;
        }
    }
}
