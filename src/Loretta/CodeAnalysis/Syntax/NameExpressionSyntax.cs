namespace Loretta.CodeAnalysis.Syntax
{
    public sealed partial class NameExpressionSyntax : VariableExpression
    {
        internal NameExpressionSyntax ( SyntaxTree syntaxTree, SyntaxToken identifier )
            : base ( syntaxTree )
        {
            this.Identifier = identifier;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.NameExpression;

        /// <summary>
        /// The identifier.
        /// </summary>
        public SyntaxToken Identifier { get; }
    }
}