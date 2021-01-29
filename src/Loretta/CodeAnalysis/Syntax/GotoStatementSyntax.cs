using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a goto statement.
    /// </summary>
    public sealed partial class GotoStatementSyntax : StatementSyntax
    {
        internal GotoStatementSyntax (
            SyntaxTree syntaxTree,
            SyntaxToken gotoKeyword,
            SyntaxToken labelName,
            Option<SyntaxToken> semicolonToken )
            : base ( syntaxTree, semicolonToken )
        {
            this.GotoKeyword = gotoKeyword;
            this.LabelName = labelName;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.GotoStatement;

        /// <summary>
        /// The 'goto' keyword.
        /// </summary>
        public SyntaxToken GotoKeyword { get; }

        /// <summary>
        /// The name of the label being jumped to.
        /// </summary>
        public SyntaxToken LabelName { get; }
    }
}