using System.Collections.Immutable;
using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a repeat until statement.
    /// </summary>
    public sealed partial class RepeatUntilStatementSyntax : StatementSyntax
    {
        internal RepeatUntilStatementSyntax (
            SyntaxTree syntaxTree,
            SyntaxToken repeatKeyword,
            ImmutableArray<StatementSyntax> body,
            SyntaxToken untilKeyword,
            ExpressionSyntax condition,
            Option<SyntaxToken> semicolonToken )
            : base ( syntaxTree, semicolonToken )
        {
            this.RepeatKeyword = repeatKeyword;
            this.Body = body;
            this.UntilKeyword = untilKeyword;
            this.Condition = condition;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.RepeatUntilStatement;

        /// <summary>
        /// The 'repeat' until keyword.
        /// </summary>
        public SyntaxToken RepeatKeyword { get; }

        /// <summary>
        /// The repeat until's body.
        /// </summary>
        public ImmutableArray<StatementSyntax> Body { get; }

        /// <summary>
        /// The 'until' keyword.
        /// </summary>
        public SyntaxToken UntilKeyword { get; }

        /// <summary>
        /// The repeat until's condition.
        /// </summary>
        public ExpressionSyntax Condition { get; }
    }
}