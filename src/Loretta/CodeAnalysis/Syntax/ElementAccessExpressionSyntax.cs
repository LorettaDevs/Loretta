namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents an element access expression.
    /// </summary>
    public sealed partial class ElementAccessExpressionSyntax : VariableExpressionSyntax
    {
        internal ElementAccessExpressionSyntax (
            PrefixExpressionSyntax expression,
            SyntaxToken openBracketToken,
            ExpressionSyntax keyExpression,
            SyntaxToken closeBracketToken )
        {
            this.Expression = expression;
            this.OpenBracketToken = openBracketToken;
            this.KeyExpression = keyExpression;
            this.CloseBracketToken = closeBracketToken;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.ElementAccessExpression;

        /// <summary>
        /// The expression that contains the member being accessed.
        /// </summary>
        public PrefixExpressionSyntax Expression { get; }

        /// <summary>
        /// The opening bracket token.
        /// </summary>
        public SyntaxToken OpenBracketToken { get; }

        /// <summary>
        /// The key expression.
        /// </summary>
        public ExpressionSyntax KeyExpression { get; }

        /// <summary>
        /// The closing bracket token.
        /// </summary>
        public SyntaxToken CloseBracketToken { get; }
    }
}