using System.Collections.Immutable;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents an else clause.
    /// </summary>
    public sealed partial class ElseClauseSyntax : SyntaxNode
    {
        internal ElseClauseSyntax ( SyntaxTree syntaxTree, SyntaxToken elseKeyword, ImmutableArray<StatementSyntax> elseBody )
            : base ( syntaxTree )
        {
            this.ElseKeyword = elseKeyword;
            this.ElseBody = elseBody;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.ElseClause;

        /// <summary>
        /// The 'else' keyword.
        /// </summary>
        public SyntaxToken ElseKeyword { get; }

        /// <summary>
        /// The else's body.
        /// </summary>
        public ImmutableArray<StatementSyntax> ElseBody { get; }
    }
}