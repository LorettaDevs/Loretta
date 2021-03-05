using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal partial class SyntaxToken
    {
        internal class SyntaxIdentifierWithTrailingTrivia : SyntaxIdentifier
        {
            static SyntaxIdentifierWithTrailingTrivia()
            {
                ObjectBinder.RegisterTypeReader(typeof(SyntaxIdentifierWithTrailingTrivia), r => new SyntaxIdentifierWithTrailingTrivia(r));
            }

            private readonly GreenNode? _trailing;

            internal SyntaxIdentifierWithTrailingTrivia(string name, GreenNode? trailing)
                : base(name)
            {
                if (trailing is not null)
                {
                    AdjustFlagsAndWidth(trailing);
                    _trailing = trailing;
                }
            }

            internal SyntaxIdentifierWithTrailingTrivia(
                string name,
                GreenNode? trailing,
                DiagnosticInfo[]? diagnostics,
                SyntaxAnnotation[]? annotations)
                : base(name, diagnostics, annotations)
            {
                if (trailing is not null)
                {
                    AdjustFlagsAndWidth(trailing);
                    _trailing = trailing;
                }
            }

            internal SyntaxIdentifierWithTrailingTrivia(ObjectReader reader)
                : base(reader)
            {
                var trailing = (GreenNode?) reader.ReadValue();
                if (trailing is not null)
                {
                    AdjustFlagsAndWidth(trailing);
                    _trailing = trailing;
                }
            }

            internal override void WriteTo(ObjectWriter writer)
            {
                base.WriteTo(writer);
                writer.WriteValue(_trailing);
            }

            public override GreenNode? GetTrailingTrivia() => _trailing;

            public override SyntaxToken TokenWithLeadingTrivia(GreenNode? trivia)
                => new SyntaxIdentifierWithTrivia(Kind, _name, _name, trivia, _trailing, GetDiagnostics(), GetAnnotations());

            public override SyntaxToken TokenWithTrailingTrivia(GreenNode? trivia)
                => new SyntaxIdentifierWithTrailingTrivia(_name, trivia, GetDiagnostics(), GetAnnotations());

            internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
                => new SyntaxIdentifierWithTrailingTrivia(_name, _trailing, diagnostics, GetAnnotations());

            internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
                => new SyntaxIdentifierWithTrailingTrivia(_name, _trailing, GetDiagnostics(), annotations);
        }
    }
}
