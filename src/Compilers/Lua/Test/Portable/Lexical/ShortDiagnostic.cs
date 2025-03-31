using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Lexical;

public readonly record struct ShortDiagnostic(string Id, string Description, TextSpan Span)
{
    public ShortDiagnostic(Diagnostic diagnostic) : this(
        diagnostic.Id,
        diagnostic.GetMessage(),
        diagnostic.Location.SourceSpan) { }
}
