using System.Collections.Immutable;
using Tsu;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// Represents a function declaration statement.
    /// </summary>
    public sealed partial class FunctionDeclarationStatementSyntax : StatementSyntax
    {
        internal FunctionDeclarationStatementSyntax (
            SyntaxTree syntaxTree,
            SyntaxToken functionKeyword,
            FunctionNameSyntax name,
            ParameterListSyntax parameters,
            ImmutableArray<StatementSyntax> body,
            SyntaxToken endKeyword,
            Option<SyntaxToken> semicolonToken )
            : base ( syntaxTree, semicolonToken )
        {
            this.FunctionKeyword = functionKeyword;
            this.Name = name;
            this.Parameters = parameters;
            this.Body = body;
            this.EndKeyword = endKeyword;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.FunctionDeclarationStatement;

        /// <summary>
        /// The 'function' keyword.
        /// </summary>
        public SyntaxToken FunctionKeyword { get; }

        /// <summary>
        /// The function's name.
        /// </summary>
        public FunctionNameSyntax Name { get; }

        /// <summary>
        /// The function's parameters.
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