namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a method-style function name syntax.
    /// </summary>
    public sealed partial class MethodFunctionNameSyntax : FunctionNameSyntax
    {
        internal MethodFunctionNameSyntax (
            FunctionNameSyntax name,
            SyntaxToken colonToken,
            SyntaxToken identifier )
        {
            this.Name = name;
            this.ColonToken = colonToken;
            this.Identifier = identifier;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.MethodFunctionName;

        /// <summary>
        /// The base name for this method.
        /// </summary>
        public FunctionNameSyntax Name { get; }

        /// <summary>
        /// The colon token.
        /// </summary>
        public SyntaxToken ColonToken { get; }

        /// <summary>
        /// The method name.
        /// </summary>
        public SyntaxToken Identifier { get; }
    }
}