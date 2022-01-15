#define LARGE_TESTS
//#define LARGE_TESTS_DEBUG
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Lexical
{
    public readonly struct ShortDiagnostic
    {
        public readonly string Id;
        public readonly string Description;
        public readonly TextSpan Span;

        public ShortDiagnostic(string id, string description, TextSpan span)
        {
            Id = id;
            Description = description;
            Span = span;
        }

        public ShortDiagnostic(Diagnostic diagnostic)
            : this(diagnostic.Id, diagnostic.GetMessage(), diagnostic.Location.SourceSpan)
        {
        }

        public void Deconstruct(out string id, out string description, out TextSpan span)
        {
            id = Id;
            description = Description;
            span = Span;
        }
    }
}
