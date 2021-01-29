using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a compound assignment expression.
    /// </summary>
    public sealed partial class CompoundAssignmentStatementSyntax : StatementSyntax
    {
        internal CompoundAssignmentStatementSyntax ( SyntaxTree syntaxTree, SyntaxKind kind, VariableExpressionSyntax variable, SyntaxToken assignmentToken, ExpressionSyntax expression, Option<SyntaxToken> semicolonToken )
            : base ( syntaxTree, semicolonToken )
        {
            this.Kind = kind;
            this.Variable = variable;
            this.AssignmentOperatorToken = assignmentToken;
            this.Expression = expression;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind { get; }

        /// <summary>
        /// The variable being assigned to.
        /// </summary>
        public VariableExpressionSyntax Variable { get; }

        /// <summary>
        /// The compound assignment operator token.
        /// </summary>
        public SyntaxToken AssignmentOperatorToken { get; }

        /// <summary>
        /// The expression on the right side of the operator.
        /// </summary>
        public ExpressionSyntax Expression { get; }
    }
}