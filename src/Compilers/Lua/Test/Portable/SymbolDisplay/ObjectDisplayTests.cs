using Loretta.CodeAnalysis.Lua.SymbolDisplay;
using Loretta.Test.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests.SymbolDisplay
{
    public sealed class ObjectDisplayTests
    {
        [Theory]
        [InlineData(
            """
            a
            a
            a
            """,
            """
            [[a
            a
            a]]
            """
        )]
        [InlineData(
            """
            [[a
            a
            a]]
            """,
            """
            [=[[[a
            a
            a]]]=]
            """
        )]
        public void ObjectDisplay_FormatLiteralString_OutputsLongStringWhenQuotesWereRequestedNewLineIsPresentAndEscapingWasNotRequested(string input, string expected)
        {
            var output = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.UseQuotes);

            Assert.Equal(expected, output);
        }

        [Fact, WorkItem(89, "https://github.com/LorettaDevs/Loretta/issues/89")]
        public void ObjectDisplay_FormatLiteralString_DoesNotEscapeSpace()
        {
            const string input = "hello there";

            var output = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.UseQuotes | ObjectDisplayOptions.EscapeNonPrintableCharacters);

            Assert.Equal("\"hello there\"", output);
        }
    }
}
