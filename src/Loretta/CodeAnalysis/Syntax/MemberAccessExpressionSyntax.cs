namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a member access expression.
    /// </summary>
    public sealed partial class MemberAccessExpressionSyntax : VariableExpressionSyntax
    {
        internal MemberAccessExpressionSyntax ( SyntaxTree syntaxTree, PrefixExpressionSyntax expression, SyntaxToken dotSeparator, SyntaxToken memberName )
            : base ( syntaxTree )
        {
            this.Expression = expression;
            this.DotSeparator = dotSeparator;
            this.MemberName = memberName;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.MemberAccessExpression;

        /// <summary>
        /// The expression that contains the member being accessed.
        /// </summary>
        public PrefixExpressionSyntax Expression { get; }

        /// <summary>
        /// The dot separating the expression and the identifier.
        /// </summary>
        public SyntaxToken DotSeparator { get; }

        /// <summary>
        /// The identifier representing the member name.
        /// </summary>
        public SyntaxToken MemberName { get; }
    }
}