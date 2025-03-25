using System.Diagnostics.CodeAnalysis;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests
{
    [SuppressMessage("ReSharper", "MemberCanBeFileLocal", Justification = "xUnit needs public classes")]
    public sealed class SyntaxFactsGetKeywordKindTests
    {
        [Theory, MemberData(nameof(Data))]
        public void SyntaxFacts_GetKeywordKindString_ReturnsTheCorrectKindForEachKeyword(
            string     text,
            SyntaxKind expected)
        {
            var actual = SyntaxFacts.GetKeywordKind(text);

            Assert.Equal(expected, actual);
        }

        [Theory, MemberData(nameof(Data))]
        public void SyntaxFacts_GetKeywordKindSpan_ReturnsTheCorrectKindForEachKeyword(string text, SyntaxKind expected)
        {
            var actual = SyntaxFacts.GetKeywordKind(text.AsSpan());

            Assert.Equal(expected, actual);
        }

        public static TheoryData<string, SyntaxKind> Data
        {
            get
            {
                var data = new TheoryData<string, SyntaxKind>();
#pragma warning disable CA2263 // Not available in .NET Framework
                foreach (var kind in Enum.GetValues(typeof(SyntaxKind)).Cast<SyntaxKind>().Where(SyntaxFacts.IsKeyword))
#pragma warning restore CA2263 // Not available in .NET Framework
                    data.Add(p1: SyntaxFacts.GetText(kind), kind);
                data.Add("alseif", SyntaxKind.IdentifierToken);
                data.Add("doif", SyntaxKind.IdentifierToken);
                data.Add("andor", SyntaxKind.IdentifierToken);
                data.Add("and or", SyntaxKind.IdentifierToken);
                return data;
            }
        }
    }
}
