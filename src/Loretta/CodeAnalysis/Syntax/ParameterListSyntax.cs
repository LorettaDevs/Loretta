using System.Collections.Immutable;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a function parameter list.
    /// </summary>
    public sealed partial class ParameterListSyntax : SyntaxNode
    {
        internal ParameterListSyntax ( SyntaxTree syntaxTree, SyntaxToken openParenthesisToken, SeparatedSyntaxList<ParameterSyntax> parameters, SyntaxToken closeParenthesisToken )
            : base ( syntaxTree )
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