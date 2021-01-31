using System.Collections.Immutable;
using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a generic for loop statement.
    /// </summary>
    public sealed partial class GenericForStatementSyntax : StatementSyntax
    {
        internal GenericForStatementSyntax (
            SyntaxToken forKeyword,
            SeparatedSyntaxList<SyntaxToken> identifiers,
            SyntaxToken inKeyword,
            SeparatedSyntaxList<ExpressionSyntax> expressions,
            SyntaxToken doKeyword,
            ImmutableArray<StatementSyntax> body,
            SyntaxToken endKeyword,
            Option<SyntaxToken> semicolonToken )
            : base ( semicolonToken )
        {
            this.ForKeyword = forKeyword;
            this.Identifiers = identifiers;
            this.InKeyword = inKeyword;
            this.Expressions = expressions;
            this.DoKeyword = doKeyword;
            this.Body = body;
            this.EndKeyword = endKeyword;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.GenericForStatement;

        /// <summary>
        /// The 'for' keyword.
        /// </summary>
        public SyntaxToken ForKeyword { get; }

        /// <summary>
        /// The list of loop variables.
        /// </summary>
        public SeparatedSyntaxList<SyntaxToken> Identifiers { get; }

        /// <summary>
        /// The 'in' keyword.
        /// </summary>
        public SyntaxToken InKeyword { get; }

        /// <summary>
        /// The list of expressions.
        /// </summary>
        public SeparatedSyntaxList<ExpressionSyntax> Expressions { get; }

        /// <summary>
        /// The 'do' keyword.
        /// </summary>
        public SyntaxToken DoKeyword { get; }

        /// <summary>
        /// The loop's body.
        /// </summary>
        public ImmutableArray<StatementSyntax> Body { get; }

        /// <summary>
        /// The 'end' keyword.
        /// </summary>
        public SyntaxToken EndKeyword { get; }
    }
}