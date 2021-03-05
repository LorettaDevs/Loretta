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
            protected readonly string _valueText;

            internal SyntaxIdentifierExtended(SyntaxKind contextualKind, string text, string valueText)
                : base(text)
            {
                _contextualKind = contextualKind;
                _valueText = valueText;
            }

            internal SyntaxIdentifierExtended(
                SyntaxKind contextualKind,
                string text,
                string valueText,
                DiagnosticInfo[]? diagnostics,
                SyntaxAnnotation[]? annotations)
                : base(text, diagnostics, annotations)
            {
                _contextualKind = contextualKind;
                _valueText = valueText;
            }

            internal SyntaxIdentifierExtended(ObjectReader reader)
                : base(reader)
            {
                _contextualKind = (SyntaxKind) reader.ReadUInt16();
                _valueText = reader.ReadString();
            }

            internal override void WriteTo(ObjectWriter writer)
            {
                base.WriteTo(writer);
                writer.WriteUInt16((ushort) _contextualKind);
                writer.WriteString(_valueText);
            }

            public override SyntaxKind ContextualKind => _contextualKind;

            public override string ValueText => _valueText;

            public override object Value => _valueText;

            public override SyntaxToken TokenWithLeadingTrivia(GreenNode? trivia)
                => new SyntaxTokenWithTrivia(Kind, trivia, null, GetDiagnostics(), GetAnnotations());

            public override SyntaxToken TokenWithTrailingTrivia(GreenNode? trivia)
                => new SyntaxTokenWithTrivia(Kind, null, trivia, GetDiagnostics(), GetAnnotations());

            internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
                => new SyntaxIdentifierExtended(_contextualKind, _name, _valueText, diagnostics, GetAnnotations());

            internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
                => new SyntaxIdentifierExtended(_contextualKind, _name, _valueText, GetDiagnostics(), annotations);
        }
    }
}
