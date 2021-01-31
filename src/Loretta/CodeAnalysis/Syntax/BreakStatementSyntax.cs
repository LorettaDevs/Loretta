using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a break statement.
    /// </summary>
    public sealed partial class BreakStatementSyntax : StatementSyntax
    {
        internal BreakStatementSyntax ( SyntaxToken breakKeyword, Option<SyntaxToken> semicolonToken )
            : base ( semicolonToken )
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