namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a method call (obj:method args...) expression.
    /// </summary>
    public sealed partial class MethodCallExpressionSyntax : PrefixExpressionSyntax
    {
        internal MethodCallExpressionSyntax (
            PrefixExpressionSyntax expression,
            SyntaxToken colonToken,
            SyntaxToken identifier,
            FunctionArgumentSyntax argument )
        {
            this.Expression = expression;
            this.ColonToken = colonToken;
            this.Identifier = identifier;
            this.Argument = argument;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.MethodCallExpression;

        /// <summary>
        /// The expression that contains the method being called.
        /// </summary>
        public PrefixExpressionSyntax Expression { get; }

        /// <summary>
        /// The colon token.
        /// </summary>
        public SyntaxToken ColonToken { get; }

        /// <summary>
        /// The identifier containing the method name.
        /// </summary>
        public SyntaxToken Identifier { get; }

        /// <summary>
        /// The method call's arguments.
        /// </summary>
        public FunctionArgumentSyntax Argument { get; }
    }
}