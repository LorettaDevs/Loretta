using System.Collections.Immutable;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents an anonymous function expression.
    /// </summary>
    public sealed partial class AnonymousFunctionExpressionSyntax : ExpressionSyntax
    {
        internal AnonymousFunctionExpressionSyntax (
            SyntaxToken functionKeyword,
            ParameterListSyntax parameters,
            ImmutableArray<StatementSyntax> body,
            SyntaxToken endKeyword )
        {
            this.FunctionKeyword = functionKeyword;
            this.Parameters = parameters;
            this.Body = body;
            this.EndKeyword = endKeyword;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.AnonymousFunctionExpression;

        /// <summary>
        /// The 'function' keyword.
        /// </summary>
        public SyntaxToken FunctionKeyword { get; }

        /// <summary>
        /// The list of parameters.
        /// </summary>
        public ParameterListSyntax Parameters { get; }

        /// <summary>
        /// The function's body.
        /// </summary>
        public ImmutableArray<StatementSyntax> Body { get; }

        /// <summary>
        /// The 'end' keyword.
        /// </summary>
        public SyntaxToken EndKeyword { get; }
    }
}