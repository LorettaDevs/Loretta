namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a function call expression.
    /// </summary>
    public sealed partial class FunctionCallExpressionSyntax : PrefixExpressionSyntax
    {
        internal FunctionCallExpressionSyntax ( SyntaxTree syntaxTree, PrefixExpressionSyntax expression, FunctionArgumentSyntax argument )
            : base ( syntaxTree )
        {
            this.Expression = expression;
            this.Argument = argument;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.FunctionCallExpression;

        /// <summary>
        /// The expression returning the function to be called.
        /// </summary>
        public PrefixExpressionSyntax Expression { get; }

        /// <summary>
        /// The function's arguments.
        /// </summary>
        public FunctionArgumentSyntax Argument { get; }
    }
}