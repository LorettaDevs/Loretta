namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// The base class for variable expressions (values that can be assigned to).
    /// </summary>
    public abstract class VariableExpressionSyntax : PrefixExpressionSyntax
    {
        private protected VariableExpressionSyntax ( SyntaxTree syntaxTree ) : base ( syntaxTree )
        {
        }
    }
}