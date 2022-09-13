using System.Numerics;
using Loretta.CodeAnalysis.Lua.SymbolDisplay;
using Loretta.CodeAnalysis.Lua.Utilities;
using Loretta.Test.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests.SymbolDisplay
{
    public sealed class ObjectDisplayTests
    {
        [Fact]
        public void ObjectDisplay_FormatLiteralString_ThrowsExceptionWhenValueIsNull() =>
            Assert.Throws<ArgumentNullException>(() => ObjectDisplay.FormatLiteral(null!, ObjectDisplayOptions.None));

        [Fact]
        public void ObjectDisplay_FormatLiteralString_OnlyAddsQuotesWhenAskedTo()
        {
            var noQuotes = ObjectDisplay.FormatLiteral("hello", ObjectDisplayOptions.None);
            var withQuotes = ObjectDisplay.FormatLiteral("hello", ObjectDisplayOptions.UseQuotes);

            Assert.False(noQuotes.StartsWith("\"") || noQuotes.EndsWith("\""), "No quotes output has quotes.");
            Assert.True(withQuotes.StartsWith("\"") || withQuotes.EndsWith("\""), "With quotes output has no quotes.");
        }

        [Fact]
        public void ObjectDisplay_FormatLiteralString_OnlyEscapesNonPrintableCharactersWhenAskedTo()
        {
            const string input = "\0\t\r\n";

            var unescaped = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.None);
            var escaped = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.EscapeNonPrintableCharacters);

            Assert.Equal(input, unescaped);
            Assert.Equal(@"\0\t\r\n", escaped);
        }

        [Fact]
        public void ObjectDisplay_FormatLiteralString_OnlyEscapesWithUtf8WhenAskedTo()
        {
            const string input = "\uFEFF";

            var unescaped = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.EscapeNonPrintableCharacters);
            var escaped = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.EscapeNonPrintableCharacters | ObjectDisplayOptions.EscapeWithUtf8);

            Assert.Equal(@"\u{FEFF}", unescaped);
            Assert.Equal(@"\xEF\xBB\xBF", escaped);
        }

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

        [Theory]
        [InlineData(true, "true")]
        [InlineData(false, "false")]
        public void ObjectDisplay_FormatLiteralBool_ReturnsTheCorrectValues(bool input, string expected)
        {
            var output = ObjectDisplay.FormatLiteral(input);

            Assert.Equal(expected, output);
        }

        [Fact]
        public void ObjectDisplay_FormatLiteralDouble_OutputsHexadecimalFloatsWhenAskedTo()
        {
            const double input = 255.255;

            var @decimal = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.None);
            var hexadecimal = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.UseHexadecimalNumbers);

            Assert.Equal("255.255", @decimal);
            // We don't want to indirectly test HexFloat here so we just use its output.
            Assert.Equal(HexFloat.DoubleToHexString(input), hexadecimal);
        }

        [Fact]
        public void ObjectDisplay_FormatLiteralLong_OutputsHexadecimalIntegersWhenAskedTo()
        {
            const long input = 65535;

            var @decimal = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.None);
            var hexadecimal = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.UseHexadecimalNumbers);

            Assert.Equal("65535", @decimal);
            Assert.Equal("0xFFFF", hexadecimal);
        }

        [Fact]
        public void ObjectDisplay_FormatLiteralULong_OutputsHexadecimalIntegersWhenAskedTo()
        {
            const ulong input = 65535;

            var @decimal = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.None);
            var hexadecimal = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.UseHexadecimalNumbers);

            Assert.Equal("65535ULL", @decimal);
            Assert.Equal("0xFFFFULL", hexadecimal);
        }

        [Fact]
        public void ObjectDisplay_FormatLiteralComplex_OutputsHexadecimalNumbersWhenAskedTo()
        {
            var input = new Complex(0, 255.255);

            var @decimal = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.None);
            var hexadecimal = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.UseHexadecimalNumbers);

            Assert.Equal("255.255i", @decimal);
            // We don't want to indirectly test HexFloat here so we just use its output.
            Assert.Equal(HexFloat.DoubleToHexString(input.Imaginary) + 'i', hexadecimal);
        }
    }
}
