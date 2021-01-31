using System.Collections.Immutable;
using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a while statement.
    /// </summary>
    public sealed partial class WhileStatementSyntax : StatementSyntax
    {
        internal WhileStatementSyntax (
            SyntaxToken whileKeyword,
            ExpressionSyntax condition,
            SyntaxToken doKeyword,
            ImmutableArray<StatementSyntax> body,
            SyntaxToken endKeyword,
            Option<SyntaxToken> semicolonToken )
            : base ( semicolonToken )
        {
            this.WhileKeyword = whileKeyword;
            this.Condition = condition;
            this.DoKeyword = doKeyword;
            this.Body = body;
            this.EndKeyword = endKeyword;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.WhileStatement;

        /// <summary>
        /// The 'while' keyword.
        /// </summary>
        public SyntaxToken WhileKeyword { get; }

        /// <summary>
        /// The while's condition.
        /// </summary>
        public ExpressionSyntax Condition { get; }

        /// <summary>
        /// The 'do' keyword.
        /// </summary>
        public SyntaxToken DoKeyword { get; }

        /// <summary>
        /// The while's body.
        /// </summary>
        public ImmutableArray<StatementSyntax> Body { get; }

        /// <summary>
        /// The 'end' keyword.
        /// </summary>
        public SyntaxToken EndKeyword { get; }
    }
}