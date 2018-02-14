using System.Collections.Generic;
using Loretta.Lexing;

namespace Loretta.Parsing.Nodes.Variables
{
    public class AssignmentStatement : ASTStatement
    {
        public List<ASTNode> Variables { get; } = new List<ASTNode> ( );

        public List<ASTNode> Assignments { get; } = new List<ASTNode> ( );

        public AssignmentStatement ( ASTNode parent, Scope scope, IList<LToken> tokens ) : base ( parent, scope, tokens )
        {
        }

        /// <summary>
        /// Adds a variable to this statment
        /// </summary>
        /// <param name="varExpr"></param>
        public void AddVariable ( ASTNode varExpr )
        {
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
