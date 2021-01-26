using System;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a boolean literal expression.
    /// </summary>
    public sealed partial class BooleanLiteralExpressionSyntax : ExpressionSyntax
    {
        internal BooleanLiteralExpressionSyntax ( SyntaxTree syntaxTree, SyntaxToken keywordToken )
            : base ( syntaxTree )
        {
            this.KeywordToken = keywordToken;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.BooleanLiteralExpression;

        /// <summary>
        /// The keyword's token.
        /// </summary>
        public SyntaxToken KeywordToken { get; }

        /// <summary>
        /// The boolean value.
        /// </summary>
        public Boolean Value => this.KeywordToken.Kind == SyntaxKind.TrueKeyword;
    }
}