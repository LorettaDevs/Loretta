//#define LARGE_TESTS_DEBUG

using Loretta.CodeAnalysis.Lua.SymbolDisplay;
using Loretta.CodeAnalysis.Lua.UnitTests.Parsing;
using Loretta.CodeAnalysis.Text;
using Loretta.Test.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Lexical;

public sealed class LexicalTests : LexicalTestsBase
{
    [Theory]
    [Trait("Category", "Lexer/Diagnostics")]
    [InlineData("0b00000000000000000000000000000000000000000000000000000000000000001")]
    [InlineData("0o0000000000000000000001")]
    public void Lexer_DoesNot_CountNumberDigitsNaively(string text)
    {
        var token = LexToken(text);
        Assert.NotEqual(default(SyntaxToken), token);
        Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind());
        Assert.Equal(1d, token.Value);
        Assert.Equal(text, token.Text);
        Assert.Equal(text.Length, token.FullWidth);
        Assert.Empty(token.ErrorsAndWarnings());
    }

    [Theory]
    [Trait("Category", "Lexer/Output")]
    [InlineData("--[")]
    [InlineData("--[=")]
    [InlineData("--[==")]
    [InlineData("--[ [")]
    [InlineData("--[= [")]
    [InlineData("--[= =[")]
    public void Lexer_DoesNot_IdentifyLongCommentsNaively(string text)
    {
        var eof    = LexToken(text);
        var trivia = Assert.Single(eof.LeadingTrivia);
        Assert.Equal(SyntaxKind.SingleLineCommentTrivia, trivia.Kind());
        Assert.Equal(text, trivia.ToFullString());

        Assert.False(trivia.ContainsDiagnostics);
        Assert.False(eof.ContainsDiagnostics);
    }

    [Fact]
    [Trait("Category", "Lexer/Output")]
    public void Lexer_Lexes_ShebangsOnlyOnFileStart()
    {
        const string shebang = "#!/bin/bash";

        var eof    = LexToken(shebang);
        var trivia = Assert.Single(eof.LeadingTrivia);
        Assert.Equal(SyntaxKind.ShebangTrivia, trivia.Kind());
        Assert.Equal(shebang, trivia.ToFullString());
        Assert.Equal(new TextSpan(0, shebang.Length), trivia.Span);

        var tokens = Lex($"-- a\n{shebang}").ToImmutableArray();
        var expectedBrokenTokens = new[]
        {
            new ShortToken(SyntaxKind.HashToken, "#", new TextSpan(5, 1)),
            new ShortToken(SyntaxKind.BangToken, "!", new TextSpan(6, 1)),
            new ShortToken(SyntaxKind.SlashToken, "/", new TextSpan(7, 1)),
            new ShortToken(SyntaxKind.IdentifierToken, "bin", new TextSpan(8, 3)),
            new ShortToken(SyntaxKind.SlashToken, "/", new TextSpan(11, 1)),
            new ShortToken(SyntaxKind.IdentifierToken, "bash", new TextSpan(12, 4)),
            new ShortToken(SyntaxKind.EndOfFileToken, "", new TextSpan(16, 0)),
        };

        Assert.Equal(expectedBrokenTokens.Length, tokens.Length);
        for (var idx = 0; idx < expectedBrokenTokens.Length; idx++)
        {
            var token    = tokens[idx];
            var expected = expectedBrokenTokens[idx];

            Assert.Equal(expected.Kind, token.Kind());
            Assert.Equal(expected.Text, token.Text);
            Assert.Equal(expected.Span, token.Span);
        }
    }

    [Fact]
    [Trait("Category", "Lexer/Output")]
    public void Lexer_LexesInvalidEscapes_WhenLuaSyntaxOptionsAcceptInvalidEscapesIsTrue()
    {
        const string rawText  = @"'\A\B\C\D\E'";
        const string value    = "ABCDE";
        var          strToken = LexToken(rawText, LuaSyntaxOptions.All.With(acceptInvalidEscapes: true));

        Assert.Equal(SyntaxKind.StringLiteralToken, strToken.Kind());
        Assert.Equal(rawText, strToken.Text);
        Assert.Equal(value, strToken.Value);
        strToken.GetDiagnostics().Verify();
    }

    [Fact]
    [Trait("Category", "Lexer/Lexer_Tests")]
    public void Lexer_Covers_AllTokens()
    {
        var tokenKinds = Enum.GetValues(typeof(SyntaxKind)).Cast<SyntaxKind>()
                             .Where(static k => SyntaxFacts.IsToken(k) || SyntaxFacts.IsTrivia(k));

        var untestedTokenKinds = new HashSet<(LuaSyntaxOptions Preset, SyntaxKind Kind)>(
            from kind in tokenKinds from preset in LuaSyntaxOptions.AllPresets select (preset, kind));
        untestedTokenKinds.RemoveWhere(static pair => pair.Kind == SyntaxKind.BadToken);
        untestedTokenKinds.RemoveWhere(static pair => pair.Kind == SyntaxKind.EndOfFileToken);
        untestedTokenKinds.RemoveWhere(static pair => pair.Kind == SyntaxKind.SkippedTokensTrivia);
        untestedTokenKinds.RemoveWhere(static pair => !SyntaxFacts.IsTokenOrTriviaKindEnabled(pair.Kind, pair.Preset));
        untestedTokenKinds.RemoveWhere(
            static pair => LexicalTestData.GetTokens(pair.Preset).Any(token => token.Kind == pair.Kind)
                           || LexicalTestData.GetTrivia(pair.Preset).Any(token => token.Kind == pair.Kind));

        Assert.Empty(untestedTokenKinds);
    }

    [Theory]
    [Trait("Category", "Lexer/Output")]
    [MemberData(nameof(GetTokensData))]
    public void Lexer_Lexes_Token(LuaSyntaxOptions options, ShortToken expectedToken)
    {
        var token = LexToken(expectedToken.Text, options);

        token.GetDiagnostics().Verify();
        Assert.Equal(expectedToken.Kind, token.Kind());
        Assert.Equal(expectedToken.Text, token.Text);
        Assert.Equal(expectedToken.Span, token.Span);
        if (expectedToken.Value is not { IsSome: true, Value: var expectedValue }) return;
        if (expectedValue is string expectedStr)
        {
            var actualStr = Assert.IsType<string>(token.Value);
            var formattedExpected = ObjectDisplay.FormatLiteral(
                expectedStr,
                ObjectDisplayOptions.EscapeNonPrintableCharacters);
            var formattedActual = ObjectDisplay.FormatLiteral(
                actualStr,
                ObjectDisplayOptions.EscapeNonPrintableCharacters);
            Assert.Equal(formattedExpected, formattedActual);
        }
        Assert.Equal(expectedToken.Value.Value, token.Value);
    }

    [Theory]
    [Trait("Category", "Lexer/Output")]
    [MemberData(nameof(GetTriviaData))]
    public void Lexer_Lexes_Trivia(LuaSyntaxOptions options, ShortToken expectedTrivia)
    {
        var token        = LexToken(expectedTrivia.Text, options: options);
        var actualTrivia = Assert.Single(token.LeadingTrivia);

        token.GetDiagnostics().Verify();
        Assert.Equal(expectedTrivia.Kind, actualTrivia.Kind());
        Assert.Equal(expectedTrivia.Text, actualTrivia.ToFullString());
        Assert.Equal(expectedTrivia.Span, actualTrivia.Span);
    }

    [ConditionalTheory(typeof(NoLongTestsCondition))]
    [Trait("Category", "Lexer/Output")]
    [Trait("IsLongTest", "1")]
    [Trait("LongTestType", "TokenPairs")]
    [MemberData(nameof(GetTokenPairsData))]
    public void Lexer_Lexes_TokenPairs(LuaSyntaxOptions options, ShortToken tokenA, ShortToken tokenB)
    {
        var text   = tokenA.Text + tokenB.Text;
        var tokens = Lex(text, options: options).ToImmutableArray();

        Assert.Equal(3, tokens.Length);
        tokens[0].GetDiagnostics().Verify();
        Assert.StrictEqual(tokenA, new ShortToken(tokens[0]));
        tokens[1].GetDiagnostics().Verify();
        Assert.StrictEqual(tokenB, new ShortToken(tokens[1]));
        tokens[2].GetDiagnostics().Verify();
        Assert.Equal(SyntaxKind.EndOfFileToken, tokens[2].Kind());
    }

    [ConditionalTheory(typeof(NoLongTestsCondition))]
    [Trait("Category", "Lexer/Output")]
    [Trait("IsLongTest", "1")]
    [Trait("LongTestType", "TokenPairs")]
    [MemberData(nameof(GetTokenPairsWithSeparatorsData))]
    public void Lexer_Lexes_TokenPairs_WithSeparators(
        LuaSyntaxOptions options,
        ShortToken       tokenA,
        ShortToken       expectedSeparator,
        ShortToken       tokenB)
    {
        var text   = tokenA.Text + expectedSeparator.Text + tokenB.Text;
        var tokens = Lex(text, options: options).ToImmutableArray();

        Assert.Equal(3, tokens.Length);

        tokens[0].GetDiagnostics().Verify();
        Assert.StrictEqual(tokenA, new ShortToken(tokens[0]));

        var actualSeparator = Assert.Single(tokens[0].TrailingTrivia);
        actualSeparator.GetDiagnostics().Verify();
        Assert.StrictEqual(expectedSeparator, new ShortToken(actualSeparator));

        tokens[1].GetDiagnostics().Verify();
        Assert.StrictEqual(tokenB, new ShortToken(tokens[1]));

        tokens[2].GetDiagnostics().Verify();
        Assert.Equal(SyntaxKind.EndOfFileToken, tokens[2].Kind());
    }

    public static IEnumerable<TheoryDataRow<LuaSyntaxOptions, ShortToken>> GetTokensData()
        =>
#if LARGE_TESTS_DEBUG
                from options in new[] { LuaSyntaxOptions.All }
#else
            from options in LuaSyntaxOptions.AllPresets
#endif
            from token in LexicalTestData.GetTokens(options)
            select new TheoryDataRow<LuaSyntaxOptions, ShortToken>(options, token);

    public static IEnumerable<TheoryDataRow<LuaSyntaxOptions, ShortToken>> GetTriviaData()
        =>
#if LARGE_TESTS_DEBUG
                from options in new[] { LuaSyntaxOptions.All }
#else
            from options in LuaSyntaxOptions.AllPresets
#endif
            from trivia in LexicalTestData.GetTrivia(options)
            select new TheoryDataRow<LuaSyntaxOptions, ShortToken>(options, trivia);

    public static IEnumerable<TheoryDataRow<LuaSyntaxOptions, ShortToken, ShortToken>> GetTokenPairsData()
        =>
#if LARGE_TESTS_DEBUG
                from options in new[] { LuaSyntaxOptions.All }
#else
            from options in LuaSyntaxOptions.AllPresets
#endif
            from pair in LexicalTestData.GetTokenPairs(options)
            select new TheoryDataRow<LuaSyntaxOptions, ShortToken, ShortToken>(options, pair.tokenA, pair.tokenB);

    public static IEnumerable<TheoryDataRow<LuaSyntaxOptions, ShortToken, ShortToken, ShortToken>>
        GetTokenPairsWithSeparatorsData()
        =>
#if LARGE_TESTS_DEBUG
                from options in new[] { LuaSyntaxOptions.All }
#else
            from options in LuaSyntaxOptions.AllPresets
#endif
            from tuple in LexicalTestData.GetTokenPairsWithSeparators(options)
            select new TheoryDataRow<LuaSyntaxOptions, ShortToken, ShortToken, ShortToken>(
                options,
                tuple.tokenA,
                tuple.separator,
                tuple.tokenB);
}
