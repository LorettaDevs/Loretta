using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a goto label statement.
    /// </summary>
    public sealed partial class GotoLabelStatementSyntax : StatementSyntax
    {
        internal GotoLabelStatementSyntax (
            SyntaxTree syntaxTree,
            SyntaxToken leftDelimiterToken,
            SyntaxToken identifier,
            SyntaxToken rightDelimiterToken,
            Option<SyntaxToken> semicolonToken )
            : base ( syntaxTree, semicolonToken )
        {
            this.LeftDelimiterToken = leftDelimiterToken;
            this.Identifier = identifier;
            this.RightDelimiterToken = rightDelimiterToken;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.GotoLabelStatement;

        /// <summary>
        /// The delimiter on the left of the name.
        /// </summary>
        public SyntaxToken LeftDelimiterToken { get; }

        /// <summary>
        /// The label name.
        /// </summary>
        public SyntaxToken Identifier { get; }

        /// <summary>
        /// The delimiter on the right of the name.
        /// </summary>
        public SyntaxToken RightDelimiterToken { get; }
    }
}