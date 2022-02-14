namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal partial class SyntaxToken
    {
        internal class SyntaxTokenWithValueAndTrivia<T> : SyntaxTokenWithValue<T>
        {
            static SyntaxTokenWithValueAndTrivia()
            {
                ObjectBinder.RegisterTypeReader(typeof(SyntaxTokenWithValueAndTrivia<T>), r => new SyntaxTokenWithValueAndTrivia<T>(r));
            }

            private readonly GreenNode? _leading;
            private readonly GreenNode? _trailing;

            internal SyntaxTokenWithValueAndTrivia(
                SyntaxKind kind,
                string text,
                T value,
                GreenNode? leading,
                GreenNode? trailing)
                : base(kind, text, value)
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

            internal SyntaxTokenWithValueAndTrivia(
                SyntaxKind kind,
                string text,
                T value,
                GreenNode? leading,
                GreenNode? trailing,
                DiagnosticInfo[]? diagnostics,
                SyntaxAnnotation[]? annotations)
                : base(kind, text, value, diagnostics, annotations)
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

            internal SyntaxTokenWithValueAndTrivia(ObjectReader reader)
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
                => new SyntaxTokenWithValueAndTrivia<T>(Kind, _text, _value, trivia, _trailing, GetDiagnostics(), GetAnnotations());

            public override SyntaxToken TokenWithTrailingTrivia(GreenNode? trivia)
                => new SyntaxTokenWithValueAndTrivia<T>(Kind, _text, _value, _leading, trivia, GetDiagnostics(), GetAnnotations());

            internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
                => new SyntaxTokenWithValueAndTrivia<T>(Kind, _text, _value, _leading, _trailing, diagnostics, GetAnnotations());

            internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
                => new SyntaxTokenWithValueAndTrivia<T>(Kind, _text, _value, _leading, _trailing, GetDiagnostics(), annotations);
        }
    }
}
