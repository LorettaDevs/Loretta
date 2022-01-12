using Loretta.Utilities;

namespace Loretta.CodeAnalysis.Lua.Syntax.InternalSyntax
{
    internal class SyntaxTrivia : LuaSyntaxNode
    {
        public readonly string Text;

        internal SyntaxTrivia(
            SyntaxKind kind,
            string text,
            DiagnosticInfo[]? diagnostics = null,
            SyntaxAnnotation[]? annotations = null)
            : base(kind, diagnostics, annotations, text.Length)
        {
            Text = text;
        }

        internal SyntaxTrivia(ObjectReader reader)
            : base(reader)
        {
            Text = reader.ReadString();
            FullWidth = Text.Length;
        }

        static SyntaxTrivia()
        {
            ObjectBinder.RegisterTypeReader(typeof(SyntaxTrivia), r => new SyntaxTrivia(r));
        }

        public override bool IsTrivia => true;

        internal override bool ShouldReuseInSerialization => Kind == SyntaxKind.WhitespaceTrivia &&
                                                             FullWidth < Lexer.MaxCachedTokenSize;

        internal override void WriteTo(ObjectWriter writer)
        {
            base.WriteTo(writer);
            writer.WriteString(Text);
        }

        internal static SyntaxTrivia Create(SyntaxKind kind, string text) => new SyntaxTrivia(kind, text);

        public override string ToFullString() => Text;

        public override string ToString() => Text;

        internal override GreenNode GetSlot(int index) => throw ExceptionUtilities.Unreachable;

        public override int Width
        {
            get
            {
                LorettaDebug.Assert(FullWidth == Text.Length);
                return FullWidth;
            }
        }

        public override int GetLeadingTriviaWidth() => 0;

        public override int GetTrailingTriviaWidth() => 0;

        internal override GreenNode SetDiagnostics(DiagnosticInfo[]? diagnostics) => new SyntaxTrivia(Kind, Text, diagnostics, GetAnnotations());

        internal override GreenNode SetAnnotations(SyntaxAnnotation[]? annotations) => new SyntaxTrivia(Kind, Text, GetDiagnostics(), annotations);

        public override TResult Accept<TResult>(LuaSyntaxVisitor<TResult> visitor) where TResult : default
            => visitor.VisitTrivia(this);

        public override void Accept(LuaSyntaxVisitor visitor) => visitor.VisitTrivia(this);

        protected override void WriteTriviaTo(System.IO.TextWriter writer) => writer.Write(Text);

        public static implicit operator CodeAnalysis.SyntaxTrivia(SyntaxTrivia trivia) =>
            new CodeAnalysis.SyntaxTrivia(token: default, trivia, position: 0, index: 0);

        public override bool IsEquivalentTo(GreenNode? other)
        {
            if (!base.IsEquivalentTo(other))
                return false;

            if (Text != ((SyntaxTrivia) other).Text)
                return false;

            return true;
        }

        internal override SyntaxNode CreateRed(SyntaxNode? parent, int position) => throw ExceptionUtilities.Unreachable;
    }
}
