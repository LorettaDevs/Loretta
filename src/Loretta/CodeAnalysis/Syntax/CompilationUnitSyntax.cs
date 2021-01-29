using System.Collections.Immutable;

namespace Loretta.CodeAnalysis.Syntax
{
    /// <summary>
    /// The node containing all of the file's contents.
    /// </summary>
    public sealed partial class CompilationUnitSyntax : SyntaxNode
    {
        internal CompilationUnitSyntax ( SyntaxTree syntaxTree, ImmutableArray<StatementSyntax> members, SyntaxToken endOfFileToken )
            : base ( syntaxTree )
        {
            this.Members = members;
            this.EndOfFileToken = endOfFileToken;
        }

        /// <inheritdoc/>
        public override SyntaxKind Kind => SyntaxKind.CompilationUnit;

        /// <summary>
        /// The members contained within this compilation unit.
        /// </summary>
        public ImmutableArray<StatementSyntax> Members { get; }

        /// <summary>
        /// The End-of-File token.
        /// </summary>
        public SyntaxToken EndOfFileToken { get; }
    }
}
