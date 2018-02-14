using System.Collections.Generic;
using GParse.Parsing.Errors;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Variables
{
    public class LocalVariableStatement : ASTStatement
    {
        public IList<VariableExpression> Variables { get; } = new List<VariableExpression> ( );

        public IList<ASTNode> Assignments { get; } = new List<ASTNode> ( );

        public LocalVariableStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        /// <summary>
        /// Adds a variable to this statment
        /// </summary>
        /// <param name="varExpr"></param>
        public void AddVariable ( VariableExpression varExpr )
        {
            if ( varExpr.Variable == null || !varExpr.Variable.IsLocal )
                throw new ParseException ( this.Tokens[0].Range.Start, "Non local variable added to LocalVariableStatement" );

            this.Variables.Add ( varExpr );
            this.AddChild ( varExpr );
        }

        /// <summary>
        /// Adds an assignment to this statement
        /// </summary>
        /// <param name="node"></param>
        public void AddAssignment ( ASTNode node )
        {
            this.Assignments.Add ( node );
            this.AddChild ( node );
        }
    }
}
