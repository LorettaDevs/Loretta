using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests
{
    public class SyntaxKindTests
    {
        [Fact]
        public void SyntaxKindHasNoDuplicates()
        {
#pragma warning disable CA1825 // (Justification: Not performance critical and helps people see what it should be used for.)
            var names = Enum.GetNames(typeof(SyntaxKind)).Except(new string[] { /* insert backwards compat kinds here */ });
#pragma warning restore CA1825 // (Justification: Not performance critical and helps people see what it should be used for.)
            var groups = names.GroupBy(name => (SyntaxKind) Enum.Parse(typeof(SyntaxKind), name));
            foreach (var kinds in groups.Where(group => group.Count() > 1))
                Assert.True(false, $"Found duplicates kinds: {string.Join(", ", kinds)}.");
        }

        [Fact]
        public void TokenKindsHaveText()
        {
            var kinds = Enum.GetValues(typeof(SyntaxKind))
                            .Cast<SyntaxKind>()
                            .Where(SyntaxFacts.IsToken);

            var textfulTokens = new[]
            {
                SyntaxKind.BadToken,
                SyntaxKind.HashStringLiteralToken,
                SyntaxKind.IdentifierToken,
                SyntaxKind.InterpolatedStringTextToken,
                SyntaxKind.InterpolatedStringToken,
                SyntaxKind.NumericLiteralToken,
                SyntaxKind.StringLiteralToken,
            };

            foreach (var kind in kinds)
            {
                if (kind == SyntaxKind.EndOfFileToken || textfulTokens.Contains(kind))
                    continue;

                var text = SyntaxFacts.GetText(kind);
                Assert.False(string.IsNullOrEmpty(text), $"Token {kind} has no fixed text.");
            }
        }
    }
}
