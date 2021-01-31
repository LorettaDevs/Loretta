namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// A name expression.
    /// </summary>
    public sealed partial class NameExpressionSyntax : VariableExpressionSyntax
    {
        internal NameExpressionSyntax ( SyntaxToken identifier )
        {
            this.Identifier = identifier;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.NameExpression;

        /// <summary>
        /// The identifier.
        /// </summary>
        public SyntaxToken Identifier { get; }
    }
}