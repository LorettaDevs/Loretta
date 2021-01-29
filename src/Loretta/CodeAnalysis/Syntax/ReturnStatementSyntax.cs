using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a return statement.
    /// </summary>
    public sealed partial class ReturnStatementSyntax : StatementSyntax
    {
        internal ReturnStatementSyntax (
            SyntaxTree syntaxTree,
            SyntaxToken returnKeyword,
            SeparatedSyntaxList<ExpressionSyntax> expressions,
            Option<SyntaxToken> semicolonToken )
            : base ( syntaxTree, semicolonToken )
        {
            this.ReturnKeyword = returnKeyword;
            this.Expressions = expressions;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.ReturnStatement;

        /// <summary>
        /// The 'return' keyword.
        /// </summary>
        public SyntaxToken ReturnKeyword { get; }

        /// <summary>
        /// The expressions being returned.
        /// </summary>
        public SeparatedSyntaxList<ExpressionSyntax> Expressions { get; }
    }
}