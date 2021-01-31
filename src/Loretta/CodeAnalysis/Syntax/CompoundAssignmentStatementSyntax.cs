using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a compound assignment expression.
    /// </summary>
    public sealed partial class CompoundAssignmentStatementSyntax : StatementSyntax
    {
        internal CompoundAssignmentStatementSyntax ( SyntaxKind kind, PrefixExpressionSyntax variable, SyntaxToken assignmentOperatorToken, ExpressionSyntax expression, Option<SyntaxToken> semicolonToken )
            : base ( semicolonToken )
        {
            this.Kind = kind;
            this.Variable = variable;
            this.AssignmentOperatorToken = assignmentOperatorToken;
            this.Expression = expression;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind { get; }

        /// <summary>
        /// The variable being assigned to.
        /// </summary>
        public PrefixExpressionSyntax Variable { get; }

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