namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a function name which is only an identifier.
    /// </summary>
    public sealed partial class SimpleFunctionNameSyntax : FunctionNameSyntax
    {
        internal SimpleFunctionNameSyntax ( SyntaxToken identifier )
        {
            this.Identifier = identifier;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.SimpleFunctionName;

        /// <summary>
        /// The function name.
        /// </summary>
        public SyntaxToken Identifier { get; }
    }
}