namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal partial class SyntaxToken
    {
        internal class SyntaxTokenWithTrivia : SyntaxToken
        {
            static SyntaxTokenWithTrivia()
            {
                ObjectBinder.RegisterTypeReader(typeof(SyntaxTokenWithTrivia), r => new SyntaxTokenWithTrivia(r));
            }

            protected readonly GreenNode? _leading;
            protected readonly GreenNode? _trailing;

            internal SyntaxTokenWithTrivia(SyntaxKind kind, GreenNode? leading, GreenNode? trailing)
                : base(kind)
            {
                if (leading is not null)
                {
                    AdjustFlagsAndWidth(leading);
                    _leading = leading;
                }
                if (trailing is not null)
                {
                    AdjustFlagsAndWidth(trailing);
                    _trailing = trailing;
                }
            }

            internal SyntaxTokenWithTrivia(
                SyntaxKind kind,
                GreenNode? leading,
                GreenNode? trailing,
                DiagnosticInfo[]? diagnostics,
                SyntaxAnnotation[]? annotations)
                : base(kind, diagnostics, annotations)
            {
                if (leading is not null)
                {
                    AdjustFlagsAndWidth(leading);
                    _leading = leading;
                }
                if (trailing is not null)
                {
                    AdjustFlagsAndWidth(trailing);
                    _trailing = trailing;
                }
            }

            internal SyntaxTokenWithTrivia(ObjectReader reader)
                : base(reader)
            {
                var leading = (GreenNode?) reader.ReadValue();
                if (leading is not null)
                {
                    AdjustFlagsAndWidth(leading);
                    _leading = leading;
                }
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
                writer.WriteValue(_leading);
                writer.WriteValue(_trailing);
            }

            public override GreenNode? GetLeadingTrivia() => _leading;

            public override GreenNode? GetTrailingTrivia() => _trailing;

            public override SyntaxToken TokenWithLeadingTrivia(GreenNode? trivia)
                => new SyntaxTokenWithTrivia(Kind, trivia, _trailing, GetDiagnostics(), GetAnnotations());

            public override SyntaxToken TokenWithTrailingTrivia(GreenNode? trivia)
                => new SyntaxTokenWithTrivia(Kind, _leading, trivia, GetDiagnostics(), GetAnnotations());

            internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
                => new SyntaxTokenWithTrivia(Kind, _leading, _trailing, diagnostics, GetAnnotations());

            internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
                => new SyntaxTokenWithTrivia(Kind, _leading, _trailing, GetDiagnostics(), annotations);
        }
    }
}
