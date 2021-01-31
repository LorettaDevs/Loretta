namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a literal expression.
    /// </summary>
    public sealed partial class LiteralExpressionSyntax : ExpressionSyntax
    {
        internal LiteralExpressionSyntax ( SyntaxKind kind, SyntaxToken token )
        {
            this.Kind = kind;
            this.Token = token;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind { get; }

        /// <summary>
        /// The literal token.
        /// </summary>
        public SyntaxToken Token { get; }
    }
}
