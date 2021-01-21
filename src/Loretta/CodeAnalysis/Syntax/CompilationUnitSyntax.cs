using System.Collections.Immutable;

namespace Loretta.CodeAnalysis.Syntax
{
    public sealed class CompilationUnitSyntax : SyntaxNode
    {
        internal CompilationUnitSyntax ( SyntaxTree syntaxTree, ImmutableArray<MemberSyntax> members, SyntaxToken endOfFileToken )
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
        public ImmutableArray<MemberSyntax> Members { get; }

        /// <summary>
        /// The End-of-File token.
        /// </summary>
        public SyntaxToken EndOfFileToken { get; }
    }
}
