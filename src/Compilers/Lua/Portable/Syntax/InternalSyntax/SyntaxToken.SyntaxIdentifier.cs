using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal partial class SyntaxToken
    {
        internal class SyntaxIdentifier : SyntaxToken
        {
            static SyntaxIdentifier()
            {
                ObjectBinder.RegisterTypeReader(typeof(SyntaxIdentifier), r => new SyntaxIdentifier(r));
            }

            protected readonly string _name;

            internal SyntaxIdentifier(string name)
                : base(SyntaxKind.IdentifierToken, name.Length)
            {
                _name = name;
            }

            internal SyntaxIdentifier(string name, DiagnosticInfo[]? diagnostics, SyntaxAnnotation[]? annotations)
                : base(SyntaxKind.IdentifierToken, name.Length, diagnostics, annotations)
            {
                _name = name;
            }

            internal SyntaxIdentifier(ObjectReader reader)
                : base(reader)
            {
                _name = reader.ReadString();
                FullWidth = _name.Length;
            }

            internal override void WriteTo(ObjectWriter writer)
            {
                base.WriteTo(writer);
                writer.WriteString(_name);
            }

            public override string Text => _name;

            public override object Value => _name;

            public override string ValueText => _name;

            public override SyntaxToken TokenWithLeadingTrivia(GreenNode? trivia)
                => new SyntaxTokenWithTrivia(Kind, trivia, null, GetDiagnostics(), GetAnnotations());

            public override SyntaxToken TokenWithTrailingTrivia(GreenNode? trivia)
                => new SyntaxIdentifierWithTrailingTrivia(_name, trivia, GetDiagnostics(), GetAnnotations());

            internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
                => new SyntaxIdentifier(_name, diagnostics, GetAnnotations());

            internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
                => new SyntaxIdentifier(_name, GetDiagnostics(), annotations);
        }
    }
}
