namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a parenthesized expression.
    /// </summary>
    public sealed partial class ParenthesizedExpressionSyntax : PrefixExpressionSyntax
    {
        internal ParenthesizedExpressionSyntax (
            SyntaxTree syntaxTree,
            SyntaxToken openParenthesisToken,
            ExpressionSyntax expression,
            SyntaxToken closeParenthesisToken )
            : base ( syntaxTree )
        {
            this.OpenParenthesisToken = openParenthesisToken;
            this.InnerExpression = expression;
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
        public ExpressionSyntax InnerExpression { get; }

        /// <summary>
        /// The closing parenthesis token.
        /// </summary>
        public SyntaxToken CloseParenthesisToken { get; }
    }
}