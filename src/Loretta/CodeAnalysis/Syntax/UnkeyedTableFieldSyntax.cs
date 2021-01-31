namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a table field without a key.
    /// </summary>
    public sealed partial class UnkeyedTableFieldSyntax : TableFieldSyntax
    {
        internal UnkeyedTableFieldSyntax ( ExpressionSyntax value )
        {
            this.Value = value;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.UnkeyedTableField;

        /// <summary>
        /// The table field's value
        /// </summary>
        public ExpressionSyntax Value { get; }
    }
}