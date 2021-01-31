namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a vararg expression.
    /// </summary>
    public sealed partial class VarArgExpressionSyntax : ExpressionSyntax
    {
        internal VarArgExpressionSyntax ( SyntaxToken varArgToken )
        {
            this.VarArgToken = varArgToken;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.VarArgExpression;

        /// <summary>
        /// The vararg token.
        /// </summary>
        public SyntaxToken VarArgToken { get; }
    }
}