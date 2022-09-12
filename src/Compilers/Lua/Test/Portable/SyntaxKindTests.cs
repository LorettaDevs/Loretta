using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests
{
    public class SyntaxKindTests
    {
        [Fact]
        public void SyntaxKindHasNoDuplicates()
        {
            var names = Enum.GetNames(typeof(SyntaxKind)).Except(new[] { "StartEqualsToken" });
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
                SyntaxKind.NumericLiteralToken,
                SyntaxKind.StringLiteralToken,
                SyntaxKind.IdentifierToken,
                SyntaxKind.HashStringLiteralToken,
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
