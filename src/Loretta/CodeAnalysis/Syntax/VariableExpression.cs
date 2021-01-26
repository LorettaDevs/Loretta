namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// The base class for variable expressions (values that can be assigned to).
    /// </summary>
    public abstract class VariableExpression : PrefixExpressionSyntax
    {
        private protected VariableExpression ( SyntaxTree syntaxTree ) : base ( syntaxTree )
        {
        }
    }
}