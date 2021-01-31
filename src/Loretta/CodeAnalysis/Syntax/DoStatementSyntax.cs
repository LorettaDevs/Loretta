using System.Collections.Immutable;
using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a do statement.
    /// </summary>
    public sealed partial class DoStatementSyntax : StatementSyntax
    {
        internal DoStatementSyntax (
            SyntaxToken doKeyword,
            ImmutableArray<StatementSyntax> body,
            SyntaxToken endKeyword,
            Option<SyntaxToken> semicolonToken )
            : base ( semicolonToken )
        {
            this.DoKeyword = doKeyword;
            this.Body = body;
            this.EndKeyword = endKeyword;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.DoStatement;

        /// <summary>
        /// The 'do' keyword.
        /// </summary>
        public SyntaxToken DoKeyword { get; }

        /// <summary>
        /// The do's body.
        /// </summary>
        public ImmutableArray<StatementSyntax> Body { get; }

        /// <summary>
        /// The 'end' keyword.
        /// </summary>
        public SyntaxToken EndKeyword { get; }
    }
}