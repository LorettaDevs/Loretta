using System;
using System.Collections.Generic;
using System.Text;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a string literal expression.
    /// </summary>
    public sealed partial class StringLiteralExpressionSyntax : ExpressionSyntax
    {
        internal StringLiteralExpressionSyntax ( SyntaxTree syntaxTree, SyntaxToken stringToken )
            : base ( syntaxTree )
        {
            this.StringToken = stringToken;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.StringLiteralExpression;

        /// <summary>
        /// The string token.
        /// </summary>
        public SyntaxToken StringToken { get; }

        /// <summary>
        /// The string value.
        /// </summary>
        public String Value => ( String ) this.StringToken.Value.Value!;
    }
}
