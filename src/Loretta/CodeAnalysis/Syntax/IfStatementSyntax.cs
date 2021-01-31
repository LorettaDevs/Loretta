using System.Collections.Immutable;
using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents an if statement.
    /// </summary>
    public sealed partial class IfStatementSyntax : StatementSyntax
    {
        internal IfStatementSyntax (
            SyntaxToken ifKeyword,
            ExpressionSyntax condition,
            SyntaxToken thenKeyword,
            ImmutableArray<StatementSyntax> body,
            ImmutableArray<ElseIfClauseSyntax> elseIfClauses,
            Option<ElseClauseSyntax> elseClause,
            SyntaxToken endKeyword,
            Option<SyntaxToken> semicolonToken )
            : base ( semicolonToken )
        {
            this.IfKeyword = ifKeyword;
            this.Condition = condition;
            this.ThenKeyword = thenKeyword;
            this.Body = body;
            this.ElseIfClauses = elseIfClauses;
            this.ElseClause = elseClause;
            this.EndKeyword = endKeyword;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.IfStatement;

        /// <summary>
        /// The 'if' keyword.
        /// </summary>
        public SyntaxToken IfKeyword { get; }

        /// <summary>
        /// The if's condition.
        /// </summary>
        public ExpressionSyntax Condition { get; }

        /// <summary>
        /// The 'then' keyword.
        /// </summary>
        public SyntaxToken ThenKeyword { get; }

        /// <summary>
        /// The if's body.
        /// </summary>
        public ImmutableArray<StatementSyntax> Body { get; }

        /// <summary>
        /// The elseif clauses.
        /// </summary>
        public ImmutableArray<ElseIfClauseSyntax> ElseIfClauses { get; }

        /// <summary>
        /// The else clause.
        /// </summary>
        public Option<ElseClauseSyntax> ElseClause { get; }

        /// <summary>
        /// The 'end' keyword.
        /// </summary>
        public SyntaxToken EndKeyword { get; }
    }
}