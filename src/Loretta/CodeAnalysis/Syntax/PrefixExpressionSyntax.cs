namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// The base node for prefix expressions (expressions that can be called as functions,
    /// have methods called on them and/or have members/elements accessed on them).
    /// </summary>
    public abstract class PrefixExpressionSyntax : ExpressionSyntax
    {
        private protected PrefixExpressionSyntax ( SyntaxTree syntaxTree ) : base ( syntaxTree )
        {
        }
    }
}
