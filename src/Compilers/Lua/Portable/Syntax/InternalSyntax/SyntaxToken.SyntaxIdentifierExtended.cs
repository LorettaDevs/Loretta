using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal partial class SyntaxToken
    {
        internal class SyntaxIdentifierExtended : SyntaxIdentifier
        {
            static SyntaxIdentifierExtended()
            {
                ObjectBinder.RegisterTypeReader(typeof(SyntaxIdentifierExtended), r => new SyntaxIdentifierExtended(r));
            }

            protected readonly SyntaxKind _contextualKind;

            internal SyntaxIdentifierExtended(SyntaxKind contextualKind, string text)
                : base(text)
            {
                _contextualKind = contextualKind;
            }

            internal SyntaxIdentifierExtended(
                SyntaxKind contextualKind,
                string text,
                DiagnosticInfo[]? diagnostics,
                SyntaxAnnotation[]? annotations)
                : base(text, diagnostics, annotations)
            {
                _contextualKind = contextualKind;
            }

            internal SyntaxIdentifierExtended(ObjectReader reader)
                : base(reader)
            {
                _contextualKind = (SyntaxKind) reader.ReadUInt16();
            }

            internal override void WriteTo(ObjectWriter writer)
            {
                base.WriteTo(writer);
                writer.WriteUInt16((ushort) _contextualKind);
            }

            public override SyntaxKind ContextualKind => _contextualKind;

            public override SyntaxToken TokenWithLeadingTrivia(GreenNode? trivia)
                => new SyntaxTokenWithTrivia(Kind, trivia, null, GetDiagnostics(), GetAnnotations());

            public override SyntaxToken TokenWithTrailingTrivia(GreenNode? trivia)
                => new SyntaxTokenWithTrivia(Kind, null, trivia, GetDiagnostics(), GetAnnotations());

            internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
                => new SyntaxIdentifierExtended(_contextualKind, _name, diagnostics, GetAnnotations());

            internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
                => new SyntaxIdentifierExtended(_contextualKind, _name, GetDiagnostics(), annotations);
        }
    }
}
