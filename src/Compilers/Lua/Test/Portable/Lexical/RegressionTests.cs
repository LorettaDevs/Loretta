using System.Diagnostics.CodeAnalysis;
using Loretta.Test.Utilities;
using Loretta.CodeAnalysis.Test.Utilities;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Lexical;

public sealed class RegressionTests : LexicalTestsBase
{
    [Test]
    public async Task Lexer_Lexes_LongStringWithoutLeadingNewLine()
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

        using (Assert.Multiple())
        {
            await Assert.That(token).HasKind(SyntaxKind.StringLiteralToken).And.HasText(rawText).And.HasValue(value);
            token.GetDiagnostics().Verify();
        }
    }

    [Test]
    [WorkItem(120, "https://github.com/LorettaDevs/Loretta/issues/120")]
    public async Task Lexer_Lexes_HexIntegersProperlyWhenPresetDoesntSupportIntegers()
    {
        const string rawText = "0X049bbe662";

        var token = LexToken(rawText, LuaSyntaxOptions.Lua51);

        using (Assert.Multiple())
        {
            await Assert.That(token).HasKind(SyntaxKind.NumericLiteralToken).And.HasText(rawText).And
                        .HasValue((double) 0x049bbe662);
            token.GetDiagnostics().Verify();
        }
    }

    [Test]
    [WorkItem(120, "https://github.com/LorettaDevs/Loretta/issues/120")]
    public async Task Lexer_Warns_AboutHexFloatsProperlyWhenPresetDoesntSupportIntegers()
    {
        const string rawText = "0X049bbe662.ff";

        var token = LexToken(rawText, LuaSyntaxOptions.Lua51);

        using (Assert.Multiple())
        {
            await Assert.That(token).HasKind(SyntaxKind.NumericLiteralToken).And.HasText(rawText);
            token.GetDiagnostics().Verify(
                // error LUA0010: Hexadecimal floating point numeric literals are not supported in this lua version
                Diagnostic(ErrorCode.ERR_HexFloatLiteralNotSupportedInVersion).WithLocation(1, 1));
        }
    }

    [Test]
    [WorkItem(127, "https://github.com/LorettaDevs/Loretta/issues/127")]
    public async Task Lexer_DoesNotLexContinueAsKeywordWhenItHasBeenDisabled()
    {
        const string rawText = """
                               local continue = true

                               if continue then
                                   continue = false
                               end
                               """;

        var tokens = Lex(rawText, LuaSyntaxOptions.Lua51);

        await Assert.That(tokens.Select(static t => (t.Kind(), t.ContextualKind()))).IsEquivalentTo(
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
            (SyntaxKind.EndKeyword, SyntaxKind.EndKeyword), (SyntaxKind.EndOfFileToken, SyntaxKind.EndOfFileToken),
        ]);
    }

    // This didn't exactly come from this issue, but it was another keyword that didn't have this handling.
    [Test]
    [WorkItem(127, "https://github.com/LorettaDevs/Loretta/issues/127")]
    public async Task Lexer_DoesNotLexGotoAsKeywordWhenItHasBeenDisabled()
    {
        const string rawText = """
                               ::label::

                               goto label
                               """;

        var tokens = Lex(rawText, LuaSyntaxOptions.Lua51);

        await Assert.That(tokens.Select(static t => (t.Kind(), t.ContextualKind()))).IsEquivalentTo(
        [
            // ::label::
            (SyntaxKind.ColonToken, SyntaxKind.ColonToken), (SyntaxKind.ColonToken, SyntaxKind.ColonToken),
            (SyntaxKind.IdentifierToken, SyntaxKind.None), (SyntaxKind.ColonToken, SyntaxKind.ColonToken),
            (SyntaxKind.ColonToken, SyntaxKind.ColonToken),

            // goto label
            (SyntaxKind.IdentifierToken, SyntaxKind.None), (SyntaxKind.IdentifierToken, SyntaxKind.None),
            (SyntaxKind.EndOfFileToken, SyntaxKind.EndOfFileToken),
        ]);
    }

    [Test]
    [WorkItem(142, "https://github.com/LorettaDevs/Loretta/issues/142")]
    [Arguments(
        """
        "\30\62\71\35\5\20\120\47\117\83\71\53"
        """,
        "\x1E\x3E\x47\x23\x05\x14\x78\x2F\x75\x53\x47\x35")]
    [Arguments(
        """
        "\61\38\7\22\7\9\38\20\53\16\22\61"
        """,
        "\x3D\x26\x07\x16\x07\x09\x26\x14\x35\x10\x16\x3D")]
    [SuppressMessage("ReSharper", "CanSimplifyStringEscapeSequence")]
    public async Task Lexer_ProperlyParsesDecimalEscapesInStrings(string rawText, string expectedValue)
    {
        var token = LexToken(rawText, LuaSyntaxOptions.Lua51);

        using (Assert.Multiple())
        {
            await Assert.That(token).HasKind(SyntaxKind.StringLiteralToken).And.HasText(rawText).And
                        .HasValue(expectedValue);
            token.GetDiagnostics().Verify();
        }
    }
}
