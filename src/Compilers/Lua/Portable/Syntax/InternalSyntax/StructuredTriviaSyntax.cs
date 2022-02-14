namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal abstract partial class StructuredTriviaSyntax : LuaSyntaxNode
    {
        internal StructuredTriviaSyntax(
            SyntaxKind kind,
            DiagnosticInfo[]? diagnostics = null,
            SyntaxAnnotation[]? annotations = null)
            : base(kind, diagnostics, annotations)
        {
            Initialize();
        }

        internal StructuredTriviaSyntax(ObjectReader reader)
            : base(reader)
        {
            Initialize();
        }

        private void Initialize()
        {
            flags |= NodeFlags.ContainsStructuredTrivia;

            if (Kind == SyntaxKind.SkippedTokensTrivia)
            {
                flags |= NodeFlags.ContainsSkippedText;
            }
        }
    }
}
