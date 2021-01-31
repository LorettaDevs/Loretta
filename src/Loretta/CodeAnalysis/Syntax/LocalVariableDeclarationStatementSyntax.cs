using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a local variable declaration statement.
    /// </summary>
    public sealed partial class LocalVariableDeclarationStatementSyntax : StatementSyntax
    {
        internal LocalVariableDeclarationStatementSyntax (
            SyntaxToken localKeyword,
            SeparatedSyntaxList<NameExpressionSyntax> names,
            Option<SyntaxToken> equalsToken,
            Option<SeparatedSyntaxList<ExpressionSyntax>> values,
            Option<SyntaxToken> semicolonToken )
            : base ( semicolonToken )
        {
            this.LocalKeyword = localKeyword;
            this.Names = names;
            this.EqualsToken = equalsToken;
            this.Values = values;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.LocalVariableDeclarationStatement;

        /// <summary>
        /// The local keyword.
        /// </summary>
        public SyntaxToken LocalKeyword { get; }

        /// <summary>
        /// The list of names being assigned to.
        /// </summary>
        public SeparatedSyntaxList<NameExpressionSyntax> Names { get; }

        /// <summary>
        /// The equals token.
        /// May be None if no values were assigned.
        /// </summary>
        public Option<SyntaxToken> EqualsToken { get; }

        /// <summary>
        /// The list of values being assigned.
        /// May be None if no values were assigned.
        /// </summary>
        public Option<SeparatedSyntaxList<ExpressionSyntax>> Values { get; }
    }
}