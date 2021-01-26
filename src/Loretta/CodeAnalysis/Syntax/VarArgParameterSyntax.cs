namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a vararg parameter.
    /// </summary>
    public sealed partial class VarArgParameterSyntax : ParameterSyntax
    {
        internal VarArgParameterSyntax ( SyntaxTree syntaxTree, SyntaxToken varArgToken )
            : base ( syntaxTree )
        {
            this.VarArgToken = varArgToken;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.VarArgParameter;

        /// <summary>
        /// The vararg token.
        /// </summary>
        public SyntaxToken VarArgToken { get; }
    }
}