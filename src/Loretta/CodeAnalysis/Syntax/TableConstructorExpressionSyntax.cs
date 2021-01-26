namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a table constructor expresion.
    /// </summary>
    public sealed partial class TableConstructorExpressionSyntax : ExpressionSyntax
    {
        internal TableConstructorExpressionSyntax (
            SyntaxTree syntaxTree,
            SyntaxToken openBraceToken,
            SeparatedSyntaxList<TableFieldSyntax> fields,
            SyntaxToken closeBraceToken )
            : base ( syntaxTree )
        {
            this.OpenBraceToken = openBraceToken;
            this.Fields = fields;
            this.CloseBraceToken = closeBraceToken;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.TableConstructorExpression;

        /// <summary>
        /// The opening brace token.
        /// </summary>
        public SyntaxToken OpenBraceToken { get; }

        /// <summary>
        /// The list of fields.
        /// </summary>
        public SeparatedSyntaxList<TableFieldSyntax> Fields { get; }

        /// <summary>
        /// The closing brace token.
        /// </summary>
        public SyntaxToken CloseBraceToken { get; }
    }
}