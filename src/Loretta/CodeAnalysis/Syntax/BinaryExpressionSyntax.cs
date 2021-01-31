namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a binary expression.
    /// </summary>
    public sealed partial class BinaryExpressionSyntax : ExpressionSyntax
    {
        internal BinaryExpressionSyntax (
            SyntaxTree syntaxTree,
            SyntaxKind kind,
            ExpressionSyntax left,
            SyntaxToken operatorToken,
            ExpressionSyntax right )
            : base ( syntaxTree )
        {
            this.Kind = kind;
            this.Left = left;
            this.OperatorToken = operatorToken;
            this.Right = right;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind { get; }

        /// <summary>
        /// The expression on the left side of the operator.
        /// </summary>
        public ExpressionSyntax Left { get; }

        /// <summary>
        /// The operator token.
        /// </summary>
        public SyntaxToken OperatorToken { get; }

        /// <summary>
        /// The expression on the right side of the operator.
        /// </summary>
        public ExpressionSyntax Right { get; }
    }
}