namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a table field whose key is an expression.
    /// </summary>
    public sealed partial class ExpressionKeyedTableFieldSyntax : TableFieldSyntax
    {
        internal ExpressionKeyedTableFieldSyntax (
            SyntaxToken openBracketToken,
            ExpressionSyntax key,
            SyntaxToken closeBracketToken,
            SyntaxToken equalsToken,
            ExpressionSyntax value )
        {
            this.OpenBracketToken = openBracketToken;
            this.Key = key;
            this.CloseBracketToken = closeBracketToken;
            this.EqualsToken = equalsToken;
            this.Value = value;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.ExpressionKeyedTableField;

        /// <summary>
        /// The opening bracket token.
        /// </summary>
        public SyntaxToken OpenBracketToken { get; }

        /// <summary>
        /// The table field's key.
        /// </summary>
        public ExpressionSyntax Key { get; }

        /// <summary>
        /// The closing bracket token.
        /// </summary>
        public SyntaxToken CloseBracketToken { get; }

        /// <summary>
        /// The equals token.
        /// </summary>
        public SyntaxToken EqualsToken { get; }

        /// <summary>
        /// The field's value.
        /// </summary>
        public ExpressionSyntax Value { get; }
    }
}