namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a string being passed as a function's argument.
    /// </summary>
    public sealed partial class StringFunctionArgumentSyntax : FunctionArgumentSyntax
    {
        internal StringFunctionArgumentSyntax ( LiteralExpressionSyntax expression )
        {
            this.Expression = expression;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.StringFunctionArgument;

        /// <summary>
        /// The string literal being passed as an argument.
        /// </summary>
        public LiteralExpressionSyntax Expression { get; }
    }
}