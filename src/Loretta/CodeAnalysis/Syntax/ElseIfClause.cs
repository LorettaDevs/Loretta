using System.Collections.Immutable;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents an elseif clause.
    /// </summary>
    public sealed partial class ElseIfClauseSyntax : SyntaxNode
    {
        internal ElseIfClauseSyntax (
            SyntaxToken elseIfKeyword,
            ExpressionSyntax condition,
            SyntaxToken thenKeyword,
            ImmutableArray<StatementSyntax> body )
        {
            this.ElseIfKeyword = elseIfKeyword;
            this.Condition = condition;
            this.ThenKeyword = thenKeyword;
            this.Body = body;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.ElseIfClause;

        /// <summary>
        /// The 'elseif' keyword.
        /// </summary>
        public SyntaxToken ElseIfKeyword { get; }

        /// <summary>
        /// The condition.
        /// </summary>
        public ExpressionSyntax Condition { get; }

        /// <summary>
        /// The 'then' keyword.
        /// </summary>
        public SyntaxToken ThenKeyword { get; }

        /// <summary>
        /// The elseif body.
        /// </summary>
        public ImmutableArray<StatementSyntax> Body { get; }
    }
}