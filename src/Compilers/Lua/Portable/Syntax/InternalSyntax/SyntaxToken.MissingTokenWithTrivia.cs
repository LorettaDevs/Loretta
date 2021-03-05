using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal partial class SyntaxToken
    {
        internal class MissingTokenWithTrivia : SyntaxTokenWithTrivia
        {
            static MissingTokenWithTrivia()
            {
                ObjectBinder.RegisterTypeReader(typeof(MissingTokenWithTrivia), r => new MissingTokenWithTrivia(r));
            }

            internal MissingTokenWithTrivia(SyntaxKind kind, GreenNode? leading, GreenNode? trailing)
                : base(kind, leading, trailing)
            {
                flags &= ~NodeFlags.IsNotMissing;
            }

            internal MissingTokenWithTrivia(
                SyntaxKind kind,
                GreenNode? leading,
                GreenNode? trailing,
                DiagnosticInfo[]? diagnostics,
                SyntaxAnnotation[]? annotations)
                : base(kind, leading, trailing, diagnostics, annotations)
            {
                flags &= ~NodeFlags.IsNotMissing;
            }

            internal MissingTokenWithTrivia(ObjectReader reader)
                : base(reader)
            {
                flags &= ~NodeFlags.IsNotMissing;
            }

            public override string Text => string.Empty;

            public override object? Value
            {
                get
                {
                    return Kind switch
                    {
                        SyntaxKind.IdentifierToken => string.Empty,
                        _ => null,
                    };
                }
            }

            public override SyntaxToken TokenWithLeadingTrivia(GreenNode? trivia)
                => new MissingTokenWithTrivia(Kind, trivia, _trailing, GetDiagnostics(), GetAnnotations());

            public override SyntaxToken TokenWithTrailingTrivia(GreenNode? trivia)
                => new MissingTokenWithTrivia(Kind, _leading, trivia, GetDiagnostics(), GetAnnotations());

            internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics)
                => new MissingTokenWithTrivia(Kind, _leading, _trailing, diagnostics, GetAnnotations());

            internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations)
                => new MissingTokenWithTrivia(Kind, _leading, _trailing, GetDiagnostics(), annotations);
        }
    }
}
