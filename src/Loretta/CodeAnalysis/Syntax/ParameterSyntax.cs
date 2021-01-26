namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// The base type for parameter nodes.
    /// </summary>
    public abstract class ParameterSyntax : SyntaxNode
    {
        private protected ParameterSyntax ( SyntaxTree syntaxTree ) : base ( syntaxTree )
        {
        }
    }
}