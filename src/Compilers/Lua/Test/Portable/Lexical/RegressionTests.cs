using System.Diagnostics.CodeAnalysis;
using Loretta.Test.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Lexical;

public sealed class RegressionTests : LexicalTestsBase
{
    [Fact]
    [Trait("Type", TestType.Regression)]
    [Trait("Category", "Lexer/Output")]
    public void Lexer_Lexes_LongStringWithoutLeadingNewLine()
    {
        const string rawText = """
                               [[
                               hi
                               ]]
                               """;
        const string value = """
                             hi

                             """;
        var token = LexToken(rawText);

        Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind());
        Assert.Equal(rawText, token.Text);
        Assert.Equal(value, token.Value);
        token.GetDiagnostics().Verify();
    }

    [Fact]
    [WorkItem(120, "https://github.com/LorettaDevs/Loretta/issues/120")]
    [Trait("Type", TestType.Regression)]
    [Trait("Category", "Lexer/Diagnostics")]
    public void Lexer_Lexes_HexIntegersProperlyWhenPresetDoesntSupportIntegers()
    {
        const string rawText = "0X049bbe662";

        var token = LexToken(rawText, LuaSyntaxOptions.Lua51);

        Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind());
        Assert.Equal(rawText, token.Text);
        Assert.Equal((double) 0x049bbe662, token.Value);
        token.GetDiagnostics().Verify();
    }

    [Fact]
    [WorkItem(120, "https://github.com/LorettaDevs/Loretta/issues/120")]
    [Trait("Type", TestType.Regression)]
    [Trait("Category", "Lexer/Diagnostics")]
    public void Lexer_Warns_AboutHexFloatsProperlyWhenPresetDoesntSupportIntegers()
    {
        const string rawText = "0X049bbe662.ff";

        var token = LexToken(rawText, LuaSyntaxOptions.Lua51);

        Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind());
        Assert.Equal(rawText, token.Text);
        token.GetDiagnostics().Verify(
            // error LUA0010: Hexadecimal floating point numeric literals are not supported in this lua version
            Diagnostic(ErrorCode.ERR_HexFloatLiteralNotSupportedInVersion).WithLocation(1, 1));
    }

    [Fact]
    [WorkItem(127, "https://github.com/LorettaDevs/Loretta/issues/127")]
    [Trait("Type", TestType.Regression)]
    [Trait("Category", "Lexer/Diagnostics")]
    public void Lexer_DoesNotLexContinueAsKeywordWhenItHasBeenDisabled()
    {
        const string rawText = """
                               local continue = true

                               if continue then
                                   continue = false
                               end
                               """;

        var tokens = Lex(rawText, LuaSyntaxOptions.Lua51).Select(static t => (t.Kind(), t.ContextualKind()));

        Assert.Equal(
            [
                // local continue = true
                (SyntaxKind.LocalKeyword, SyntaxKind.LocalKeyword), (SyntaxKind.IdentifierToken, SyntaxKind.None),
                (SyntaxKind.EqualsToken, SyntaxKind.EqualsToken), (SyntaxKind.TrueKeyword, SyntaxKind.TrueKeyword),

                // if continue then
                (SyntaxKind.IfKeyword, SyntaxKind.IfKeyword), (SyntaxKind.IdentifierToken, SyntaxKind.None),
                (SyntaxKind.ThenKeyword, SyntaxKind.ThenKeyword),

                //     continue = false
                (SyntaxKind.IdentifierToken, SyntaxKind.None), (SyntaxKind.EqualsToken, SyntaxKind.EqualsToken),
                (SyntaxKind.FalseKeyword, SyntaxKind.FalseKeyword),

                // end
                (SyntaxKind.EndKeyword, SyntaxKind.EndKeyword),
                (SyntaxKind.EndOfFileToken, SyntaxKind.EndOfFileToken),
            ],
            tokens);
    }

    // This didn't exactly come from this issue, but it was another keyword that didn't have this handling.
    [Fact]
    [WorkItem(127, "https://github.com/LorettaDevs/Loretta/issues/127")]
    [Trait("Type", TestType.Regression)]
    [Trait("Category", "Lexer/Diagnostics")]
    public void Lexer_DoesNotLexGotoAsKeywordWhenItHasBeenDisabled()
    {
        const string rawText = """
                               ::label::

                               goto label
                               """;

        var tokens = Lex(rawText, LuaSyntaxOptions.Lua51).Select(static t => (t.Kind(), t.ContextualKind()));

        Assert.Equal(
            [
                // ::label::
                (SyntaxKind.ColonToken, SyntaxKind.ColonToken), (SyntaxKind.ColonToken, SyntaxKind.ColonToken),
                (SyntaxKind.IdentifierToken, SyntaxKind.None), (SyntaxKind.ColonToken, SyntaxKind.ColonToken),
                (SyntaxKind.ColonToken, SyntaxKind.ColonToken),

                // goto label
                (SyntaxKind.IdentifierToken, SyntaxKind.None), (SyntaxKind.IdentifierToken, SyntaxKind.None),
                (SyntaxKind.EndOfFileToken, SyntaxKind.EndOfFileToken),
            ],
            tokens);
    }

    [Theory]
    [WorkItem(142, "https://github.com/LorettaDevs/Loretta/issues/142")]
    [Trait("Type", TestType.Regression)]
    [Trait("Category", "Lexer/Diagnostics")]
    [InlineData(
        """
        "\30\62\71\35\5\20\120\47\117\83\71\53"
        """,
        "\x1E\x3E\x47\x23\x05\x14\x78\x2F\x75\x53\x47\x35")]
    [InlineData(
        """
        "\61\38\7\22\7\9\38\20\53\16\22\61"
        """,
        "\x3D\x26\x07\x16\x07\x09\x26\x14\x35\x10\x16\x3D")]
    [SuppressMessage("ReSharper", "CanSimplifyStringEscapeSequence")]
    public void Lexer_ProperlyParsesDecimalEscapesInStrings(string rawText, string expectedValue)
    {
        var token = LexToken(rawText, LuaSyntaxOptions.Lua51);

        Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind());
        Assert.Equal(rawText, token.Text);
        Assert.Equal(expectedValue, token.Value);
        token.GetDiagnostics().Verify();
    }
}
