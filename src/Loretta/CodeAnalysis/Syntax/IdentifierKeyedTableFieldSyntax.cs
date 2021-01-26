namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a table field whose key is an identifier.
    /// </summary>
    public sealed partial class IdentifierKeyedTableFieldSyntax : TableFieldSyntax
    {
        internal IdentifierKeyedTableFieldSyntax (
            SyntaxTree syntaxTree,
            SyntaxToken identifier,
            SyntaxToken equals,
            ExpressionSyntax value )
            : base ( syntaxTree )
        {
            this.Identifier = identifier;
            this.EqualsToken = equals;
            this.Value = value;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.IdentifierKeyedTableField;

        /// <summary>
        /// The table field's key.
        /// </summary>
        public SyntaxToken Identifier { get; }

        /// <summary>
        /// The equals token.
        /// </summary>
        public SyntaxToken EqualsToken { get; }

        /// <summary>
        /// The table field's value
        /// </summary>
        public ExpressionSyntax Value { get; }
    }
}