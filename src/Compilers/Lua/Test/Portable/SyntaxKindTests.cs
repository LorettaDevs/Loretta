namespace Loretta.CodeAnalysis.Lua.UnitTests;

public sealed class SyntaxKindTests
{
    [Test]
    public void SyntaxKindHasNoDuplicates()
    {
#pragma warning disable CA1825 // (Justification: Not performance critical and helps people see what it should be used for.)
        var names = Enum.GetNames(typeof(SyntaxKind)).Except([ /* insert backwards compat kinds here */]);
#pragma warning restore CA1825 // (Justification: Not performance critical and helps people see what it should be used for.)
        var groups = names.GroupBy(static name => (SyntaxKind) Enum.Parse(typeof(SyntaxKind), name));
        foreach (var kinds in groups.Where(static group => group.Count() > 1))
            Assert.Fail($"Found duplicates kinds: {string.Join(", ", kinds)}.");
    }

    [Test]
    public async Task TokenKindsHaveText()
    {
        var kinds = Enum.GetValues(typeof(SyntaxKind)).Cast<SyntaxKind>().Where(SyntaxFacts.IsToken);

        var textfulTokens = new[]
        {
            SyntaxKind.BadToken, SyntaxKind.HashStringLiteralToken, SyntaxKind.IdentifierToken,
            SyntaxKind.InterpolatedStringTextToken, SyntaxKind.InterpolatedStringToken,
            SyntaxKind.NumericLiteralToken, SyntaxKind.StringLiteralToken,
        };

        foreach (var kind in kinds)
        {
            if (kind == SyntaxKind.EndOfFileToken || textfulTokens.Contains(kind)) continue;

            var text = SyntaxFacts.GetText(kind);
            await Assert.That(string.IsNullOrEmpty(text)).IsFalse()
                  .Because($"token SyntaxKind.{kind} should have a fixed text");
        }
    }
}
