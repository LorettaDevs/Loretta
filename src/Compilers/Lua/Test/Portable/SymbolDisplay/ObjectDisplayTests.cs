using System.Numerics;
using Loretta.CodeAnalysis.Lua.SymbolDisplay;
using Loretta.CodeAnalysis.Lua.Utilities;
using Loretta.Test.Utilities;

namespace Loretta.CodeAnalysis.Lua.UnitTests.SymbolDisplay;

public sealed class ObjectDisplayTests
{
    [Test]
    public void ObjectDisplay_FormatLiteralString_ThrowsExceptionWhenValueIsNull()
        => Assert.Throws<ArgumentNullException>(
            static () => ObjectDisplay.FormatLiteral(null!, ObjectDisplayOptions.None));

    [Test]
    public async Task ObjectDisplay_FormatLiteralString_OnlyAddsQuotesWhenAskedTo()
    {
        var noQuotes   = ObjectDisplay.FormatLiteral("hello", ObjectDisplayOptions.None);
        var withQuotes = ObjectDisplay.FormatLiteral("hello", ObjectDisplayOptions.UseQuotes);

        await Assert.That(noQuotes.StartsWith('"') || noQuotes.EndsWith('"')).IsFalse()
                    .Because("no quotes output has quotes");
        await Assert.That(withQuotes.StartsWith('"') && withQuotes.EndsWith('"')).IsTrue()
                    .Because("with quotes output has no quotes");
    }

    [Test]
    public async Task ObjectDisplay_FormatLiteralString_OnlyEscapesNonPrintableCharactersWhenAskedTo()
    {
        const string input = "\0\t\r\n";

        var unescaped = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.None);
        var escaped   = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.EscapeNonPrintableCharacters);

        await Assert.That(unescaped).IsEqualTo(input);
        await Assert.That(escaped).IsEqualTo(@"\0\t\r\n");
    }

    [Test]
    public async Task ObjectDisplay_FormatLiteralString_OnlyEscapesWithUtf8WhenAskedTo()
    {
        const string input = "\uFEFF";

        var unescaped = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.EscapeNonPrintableCharacters);
        var escaped = ObjectDisplay.FormatLiteral(
            input,
            ObjectDisplayOptions.EscapeNonPrintableCharacters | ObjectDisplayOptions.EscapeWithUtf8);

        await Assert.That(unescaped).IsEqualTo(@"\u{FEFF}");
        await Assert.That(escaped).IsEqualTo(@"\xEF\xBB\xBF");
    }

    [Test]
    [Arguments(
        """
        a
        a
        a
        """,
        """
        [[a
        a
        a]]
        """)]
    [Arguments(
        """
        [[a
        a
        a]]
        """,
        """
        [=[[[a
        a
        a]]]=]
        """)]
    public async Task
        ObjectDisplay_FormatLiteralString_OutputsLongStringWhenQuotesWereRequestedNewLineIsPresentAndEscapingWasNotRequested(
            string input,
            string expected)
    {
        var output = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.UseQuotes);

        await Assert.That(output).IsEqualTo(expected);
    }

    [Test, WorkItem(89, "https://github.com/LorettaDevs/Loretta/issues/89")]
    public async Task ObjectDisplay_FormatLiteralString_DoesNotEscapeSpace()
    {
        const string input = "hello there";

        var output = ObjectDisplay.FormatLiteral(
            input,
            ObjectDisplayOptions.UseQuotes | ObjectDisplayOptions.EscapeNonPrintableCharacters);

        await Assert.That(output).IsEqualTo("\"hello there\"");
    }

    [Test]
    [Arguments(true, "true")]
    [Arguments(false, "false")]
    public async Task ObjectDisplay_FormatLiteralBool_ReturnsTheCorrectValues(bool input, string expected)
    {
        var output = ObjectDisplay.FormatLiteral(input);

        await Assert.That(output).IsEqualTo(expected);
    }

    [Test]
    public async Task ObjectDisplay_FormatLiteralDouble_OutputsHexadecimalFloatsWhenAskedTo()
    {
        const double input = 255.255;

        var @decimal    = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.None);
        var hexadecimal = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.UseHexadecimalNumbers);

        await Assert.That(@decimal).IsEqualTo("255.255");
        // We don't want to indirectly test HexFloat here so we just use its output.
        await Assert.That(hexadecimal).IsEqualTo(HexFloat.DoubleToHexString(input));
    }

    [Test]
    public async Task ObjectDisplay_FormatLiteralLong_OutputsHexadecimalIntegersWhenAskedTo()
    {
        const long input = 65535;

        var @decimal    = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.None);
        var hexadecimal = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.UseHexadecimalNumbers);

        await Assert.That(@decimal).IsEqualTo("65535");
        await Assert.That(hexadecimal).IsEqualTo("0xFFFF");
    }

    [Test]
    public async Task ObjectDisplay_FormatLiteralULong_OutputsHexadecimalIntegersWhenAskedTo()
    {
        const ulong input = 65535;

        var @decimal    = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.None);
        var hexadecimal = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.UseHexadecimalNumbers);

        await Assert.That(@decimal).IsEqualTo("65535ULL");
        await Assert.That(hexadecimal).IsEqualTo("0xFFFFULL");
    }

    [Test]
    public async Task ObjectDisplay_FormatLiteralComplex_OutputsHexadecimalNumbersWhenAskedTo()
    {
        var input = new Complex(0, 255.255);

        var @decimal    = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.None);
        var hexadecimal = ObjectDisplay.FormatLiteral(input, ObjectDisplayOptions.UseHexadecimalNumbers);

        await Assert.That(@decimal).IsEqualTo("255.255i");
        // We don't want to indirectly test HexFloat here so we just use its output.
        await Assert.That(hexadecimal).IsEqualTo(HexFloat.DoubleToHexString(input.Imaginary) + 'i');
    }
}
