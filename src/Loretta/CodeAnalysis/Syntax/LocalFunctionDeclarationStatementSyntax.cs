using System.Collections.Immutable;
using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a local function declaration statement.
    /// </summary>
    public sealed partial class LocalFunctionDeclarationStatementSyntax : StatementSyntax
    {
        internal LocalFunctionDeclarationStatementSyntax (
            SyntaxTree syntaxTree,
            SyntaxToken localKeyword,
            SyntaxToken functionKeyword,
            SyntaxToken identifier,
            ParameterListSyntax parameters,
            ImmutableArray<StatementSyntax> body,
            SyntaxToken endKeyword,
            Option<SyntaxToken> semicolonToken )
            : base ( syntaxTree, semicolonToken )
        {
            this.LocalKeyword = localKeyword;
            this.FunctionKeyword = functionKeyword;
            this.Identifier = identifier;
            this.Parameters = parameters;
            this.Body = body;
            this.EndKeyword = endKeyword;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.LocalFunctionDeclarationStatement;

        /// <summary>
        /// The 'local' keyword.
        /// </summary>
        public SyntaxToken LocalKeyword { get; }

        /// <summary>
        /// The 'function' keyword.
        /// </summary>
        public SyntaxToken FunctionKeyword { get; }

        /// <summary>
        /// The function's name.
        /// </summary>
        public SyntaxToken Identifier { get; }

        /// <summary>
        /// The parameter list.
        /// </summary>
        public ParameterListSyntax Parameters { get; }

        /// <summary>
        /// The function's body.
        /// </summary>
        public ImmutableArray<StatementSyntax> Body { get; }

        /// <summary>
        /// The 'end' keyword.
        /// </summary>
        public SyntaxToken EndKeyword { get; }
    }
}