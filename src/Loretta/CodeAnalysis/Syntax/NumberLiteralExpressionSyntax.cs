using System;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a numeric literal expression.
    /// </summary>
    public sealed partial class NumberLiteralExpressionSyntax : ExpressionSyntax
    {
        internal NumberLiteralExpressionSyntax ( SyntaxTree syntaxTree, SyntaxToken numberToken )
            : base ( syntaxTree )
        {
            this.NumberToken = numberToken;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.NumberLiteralExpression;

        /// <summary>
        /// The number token.
        /// </summary>
        public SyntaxToken NumberToken { get; }

        /// <summary>
        /// The numeric value.
        /// </summary>
        public Double Value => ( Double ) this.NumberToken.Value.Value!;
    }
}