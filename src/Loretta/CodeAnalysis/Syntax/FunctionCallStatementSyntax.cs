using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// An expression as a statement.
    /// </summary>
    public sealed partial class ExpressionStatementSyntax : StatementSyntax
    {
        internal ExpressionStatementSyntax (
            SyntaxTree syntaxTree,
            ExpressionSyntax expression,
            Option<SyntaxToken> semicolonToken )
            : base ( syntaxTree, semicolonToken )
        {
            this.Expression = expression;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.ExpressionStatement;

        /// <summary>
        /// The function call expression.
        /// </summary>
        public ExpressionSyntax Expression { get; }
    }
}