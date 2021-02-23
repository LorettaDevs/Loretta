using Loretta.CodeAnalysis.Syntax.InternalSyntax;
using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal abstract class LuaSyntaxNode : GreenNode
    {
        protected LuaSyntaxNode(SyntaxKind kind)
            : base((ushort) kind)
        {
            GreenStats.NoteGreen(this);
        }

        protected LuaSyntaxNode(SyntaxKind kind, int fullWidth)
            : base((ushort) kind, fullWidth)
        {
            GreenStats.NoteGreen(this);
        }

        protected LuaSyntaxNode(SyntaxKind kind, DiagnosticInfo[]? diagnostics)
            : base((ushort) kind, diagnostics)
        {
            GreenStats.NoteGreen(this);
        }

        protected LuaSyntaxNode(SyntaxKind kind, DiagnosticInfo[]? diagnostics, int fullWidth)
            : base((ushort) kind, diagnostics, fullWidth)
        {
            GreenStats.NoteGreen(this);
        }

        protected LuaSyntaxNode(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
            : base((ushort) kind, diagnostics, annotations)
        {
            GreenStats.NoteGreen(this);
        }

        protected LuaSyntaxNode(SyntaxKind kind, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations, int fullWidth)
            : base((ushort) kind, diagnostics, annotations, fullWidth)
        {
            GreenStats.NoteGreen(this);
        }

        internal LuaSyntaxNode(ObjectReader reader) : base(reader)
        {
        }

        public override string Language => LanguageNames.Lua;

        public SyntaxKind Kind => (SyntaxKind) RawKind;

        public override string KindText => Kind.ToString();

        public override int RawContextualKind => RawKind;

        public override bool IsStructuredTrivia => this is StructuredTriviaSyntax;
        public override bool IsSkippedTokensTrivia => Kind == SyntaxKind.SkippedTokensTrivia;
        public override bool IsDocumentationCommentTrivia => SyntaxFacts.IsDocumentationCommentTrivia(Kind);

    }
}
