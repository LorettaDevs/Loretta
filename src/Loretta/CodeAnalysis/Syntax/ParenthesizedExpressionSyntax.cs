namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a parenthesized expression.
    /// </summary>
    public sealed partial class ParenthesizedExpressionSyntax : PrefixExpressionSyntax
    {
        internal ParenthesizedExpressionSyntax (
            SyntaxToken openParenthesisToken,
            ExpressionSyntax expression,
            SyntaxToken closeParenthesisToken )
        {
            this.OpenParenthesisToken = openParenthesisToken;
            this.Expression = expression;
            this.CloseParenthesisToken = closeParenthesisToken;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;

        /// <summary>
        /// The opening parenthesis token.
        /// </summary>
        public SyntaxToken OpenParenthesisToken { get; }

        /// <summary>
        /// The inner expression.
        /// </summary>
        public ExpressionSyntax Expression { get; }

        /// <summary>
        /// The closing parenthesis token.
        /// </summary>
        public SyntaxToken CloseParenthesisToken { get; }
    }
}