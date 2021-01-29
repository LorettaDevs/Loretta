using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a break statement.
    /// </summary>
    public sealed partial class BreakStatementSyntax : StatementSyntax
    {
        internal BreakStatementSyntax ( SyntaxTree syntaxTree, SyntaxToken breakKeyword, Option<SyntaxToken> semicolonToken )
            : base ( syntaxTree, semicolonToken )
        {
            this.BreakKeyword = breakKeyword;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.BreakStatement;

        /// <summary>
        /// The 'break' keyword.
        /// </summary>
        public SyntaxToken BreakKeyword { get; }
    }
}