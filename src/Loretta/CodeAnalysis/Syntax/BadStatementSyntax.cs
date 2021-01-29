namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a bad statement.
    /// </summary>
    public sealed partial class BadStatementSyntax : StatementSyntax
    {
        internal BadStatementSyntax ( SyntaxTree syntaxTree, BadExpressionSyntax expression )
            : base ( syntaxTree, default )
        {
            this.Expression = expression;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.BadStatement;

        /// <summary>
        /// The bad expression contained by the statement.
        /// </summary>
        public BadExpressionSyntax Expression { get; }
    }
}