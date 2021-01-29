namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents an unary expression.
    /// </summary>
    public sealed partial class UnaryExpressionSyntax : ExpressionSyntax
    {
        internal UnaryExpressionSyntax ( SyntaxTree syntaxTree, SyntaxKind kind, SyntaxToken operatorToken, ExpressionSyntax operand )
            : base ( syntaxTree )
        {
            this.Kind = kind;
            this.OperatorToken = operatorToken;
            this.Operand = operand;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind { get; }

        /// <summary>
        /// The operator token.
        /// </summary>
        public SyntaxToken OperatorToken { get; }

        /// <summary>
        /// The operand expression.
        /// </summary>
        public ExpressionSyntax Operand { get; }
    }
}