namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a vararg parameter.
    /// </summary>
    public sealed partial class VarArgParameterSyntax : ParameterSyntax
    {
        internal VarArgParameterSyntax ( SyntaxToken varArgToken )
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