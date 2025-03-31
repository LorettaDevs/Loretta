using System.Diagnostics.CodeAnalysis;

namespace Loretta.CodeAnalysis.Lua.UnitTests;

[SuppressMessage("ReSharper", "MemberCanBeFileLocal", Justification = "xUnit needs public classes")]
public sealed class SyntaxFactsGetKeywordKindTests
{
    [Test, MethodDataSource(nameof(Data))]
    public async Task SyntaxFacts_GetKeywordKindString_ReturnsTheCorrectKindForEachKeyword(
        string     text,
        SyntaxKind expected)
    {
        var actual = SyntaxFacts.GetKeywordKind(text);

        await Assert.That(actual).IsEqualTo(expected);
    }

    [Test, MethodDataSource(nameof(Data))]
    public async Task SyntaxFacts_GetKeywordKindSpan_ReturnsTheCorrectKindForEachKeyword(
        string     text,
        SyntaxKind expected)
    {
        var actual = SyntaxFacts.GetKeywordKind(text.AsSpan());

        await Assert.That(actual).IsEqualTo(expected);
    }

    public static IEnumerable<(string, SyntaxKind)> Data()
    {
#pragma warning disable CA2263 // Not available in .NET Framework
        foreach (var kind in Enum.GetValues(typeof(SyntaxKind)).Cast<SyntaxKind>().Where(SyntaxFacts.IsKeyword))
#pragma warning restore CA2263 // Not available in .NET Framework
            yield return (SyntaxFacts.GetText(kind), kind);
        yield return ("alseif", SyntaxKind.IdentifierToken);
        yield return ("doif", SyntaxKind.IdentifierToken);
        yield return ("andor", SyntaxKind.IdentifierToken);
        yield return ("and or", SyntaxKind.IdentifierToken);
    }
}
