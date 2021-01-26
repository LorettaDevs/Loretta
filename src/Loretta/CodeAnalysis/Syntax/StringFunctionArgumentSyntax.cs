namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a string being passed as a function's argument.
    /// </summary>
    public sealed partial class StringFunctionArgumentSyntax : FunctionArgumentSyntax
    {
        internal StringFunctionArgumentSyntax ( SyntaxTree syntaxTree, StringLiteralExpressionSyntax stringLiteral )
            : base ( syntaxTree )
        {
            this.StringLiteral = stringLiteral;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.StringFunctionArgument;

        /// <summary>
        /// The string literal being passed as an argument.
        /// </summary>
        public StringLiteralExpressionSyntax StringLiteral { get; }
    }
}