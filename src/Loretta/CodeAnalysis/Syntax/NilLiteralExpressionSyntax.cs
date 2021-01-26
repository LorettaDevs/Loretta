namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a nil literal expression.
    /// </summary>
    public sealed partial class NilLiteralExpressionSyntax : ExpressionSyntax
    {
        internal NilLiteralExpressionSyntax ( SyntaxTree syntaxTree, SyntaxToken nilToken )
            : base ( syntaxTree )
        {
            this.NilToken = nilToken;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.NilLiteralExpression;

        /// <summary>
        /// The nil token.
        /// </summary>
        public SyntaxToken NilToken { get; }
    }
}