namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a function declaration's parameter list.
    /// </summary>
    public sealed partial class ParameterListSyntax : SyntaxNode
    {
        internal ParameterListSyntax (
            SyntaxToken openParenthesisToken,
            SeparatedSyntaxList<ParameterSyntax> parameters,
            SyntaxToken closeParenthesisToken )
        {
            this.OpenParenthesisToken = openParenthesisToken;
            this.Parameters = parameters;
            this.CloseParenthesisToken = closeParenthesisToken;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.ParameterList;

        /// <summary>
        /// The parameter list opening parenthesis.
        /// </summary>
        public SyntaxToken OpenParenthesisToken { get; }

        /// <summary>
        /// The parameter names (and possibly a vararg at the end).
        /// </summary>
        public SeparatedSyntaxList<ParameterSyntax> Parameters { get; }

        /// <summary>
        /// The close parenthesis token.
        /// </summary>
        public SyntaxToken CloseParenthesisToken { get; }
    }
}