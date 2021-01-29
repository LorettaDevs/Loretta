using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents an assignment statement.
    /// </summary>
    public sealed partial class AssignmentStatementSyntax : StatementSyntax
    {
        internal AssignmentStatementSyntax (
            SyntaxTree syntaxTree,
            SeparatedSyntaxList<VariableExpressionSyntax> variables,
            SyntaxToken equalsToken,
            SeparatedSyntaxList<ExpressionSyntax> values,
            Option<SyntaxToken> semicolonToken )
            : base ( syntaxTree, semicolonToken )
        {
            this.Variables = variables;
            this.EqualsToken = equalsToken;
            this.Values = values;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.AssignmentStatement;

        /// <summary>
        /// The variables being assigned to.
        /// </summary>
        public SeparatedSyntaxList<VariableExpressionSyntax> Variables { get; }

        /// <summary>
        /// The equals token.
        /// </summary>
        public SyntaxToken EqualsToken { get; }

        /// <summary>
        /// The values being assigned.
        /// </summary>
        public SeparatedSyntaxList<ExpressionSyntax> Values { get; }
    }
}