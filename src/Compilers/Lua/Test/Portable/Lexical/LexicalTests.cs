//#define LARGE_TESTS_DEBUG

using System.Diagnostics.CodeAnalysis;
using Loretta.CodeAnalysis.Lua.SymbolDisplay;
using Loretta.CodeAnalysis.Test.Utilities;
using Loretta.CodeAnalysis.Text;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Lexical;

public sealed class LexicalTests : LexicalTestsBase
{
    [Test]
    [Arguments("0b00000000000000000000000000000000000000000000000000000000000000001")]
    [Arguments("0o0000000000000000000001")]
    public async Task Lexer_DoesNot_CountNumberDigitsNaively(string text)
    {
        var token = LexToken(text);
        // @formatter:off
        await Assert.That(token).IsNotDefault().And
                    .HasKind(SyntaxKind.NumericLiteralToken)
                    .And.HasValue(1d)
                    .And.HasText(text)
                    .And.Satisfies(static token => token.FullWidth, len => len.IsEqualTo(text.Length))
                    .And.Satisfies(
                        static token => token.GetDiagnostics(),
                        static diags => diags.IsEmpty());
        // @formatter:on
    }

    [Test]
    [Arguments("--[")]
    [Arguments("--[=")]
    [Arguments("--[==")]
    [Arguments("--[ [")]
    [Arguments("--[= [")]
    [Arguments("--[= =[")]
    public async Task Lexer_DoesNot_IdentifyLongCommentsNaively(string text)
    {
        var eof = LexToken(text);
        using (Assert.Multiple())
        {
            var trivia = await Assert.That<IEnumerable<SyntaxTrivia>>(eof.LeadingTrivia).HasSingleItem();

            // @formatter:off
            await Assert.That(trivia)
                        .HasKind(SyntaxKind.SingleLineCommentTrivia)
                        .And.HasText(text)
                        .And.Satisfies(static trivia => trivia.ContainsDiagnostics, static hasDiags => hasDiags.IsFalse());
            // @formatter:on
            await Assert.That(eof.ContainsDiagnostics).IsFalse();
        }
    }

    [Test]
    public async Task Lexer_Lexes_ShebangsOnlyOnFileStart()
    {
        const string shebang = "#!/bin/bash";

        var eof    = LexToken(shebang);
        var trivia = await Assert.That<IEnumerable<SyntaxTrivia>>(eof.LeadingTrivia).HasSingleItem();
        await Assert.That(trivia).HasKind(SyntaxKind.ShebangTrivia).And.HasText(shebang).And
                    .HasSpan(new TextSpan(0, shebang.Length));

        var tokens = Lex($"-- a\n{shebang}").ToImmutableArray();
        await Assert.That(tokens.Select(static tok => new ShortToken(tok))).IsEquivalentTo(
            [
                new ShortToken(SyntaxKind.HashToken, "#", new TextSpan(5, 1), "#"),
                new ShortToken(SyntaxKind.BangToken, "!", new TextSpan(6, 1), "!"),
                new ShortToken(SyntaxKind.SlashToken, "/", new TextSpan(7, 1), "/"),
                new ShortToken(SyntaxKind.IdentifierToken, "bin", new TextSpan(8, 3), "bin"),
                new ShortToken(SyntaxKind.SlashToken, "/", new TextSpan(11, 1), "/"),
                new ShortToken(SyntaxKind.IdentifierToken, "bash", new TextSpan(12, 4), "bash"),
                new ShortToken(SyntaxKind.EndOfFileToken, "", new TextSpan(16, 0), ""),
            ],
            EqualityComparer<ShortToken>.Default);
    }

    [Test]
    public async Task Lexer_LexesInvalidEscapes_WhenLuaSyntaxOptionsAcceptInvalidEscapesIsTrue()
    {
        const string rawText  = @"'\A\B\C\D\E'";
        const string value    = "ABCDE";
        var          strToken = LexToken(rawText, LuaSyntaxOptions.All.With(acceptInvalidEscapes: true));

        using (Assert.Multiple())
        {
            await Assert.That(strToken).HasKind(SyntaxKind.StringLiteralToken).And.HasText(rawText).And.HasValue(value);
            strToken.GetDiagnostics().Verify();
        }
    }

    [Test]
    public async Task Lexer_Covers_AllTokens()
    {
        var tokenKinds = Enum.GetValues<SyntaxKind>()
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

        await Assert.That(untestedTokenKinds).IsEmpty();
    }

    [Test]
    [MethodDataSource(nameof(GetTokensData))]
    public async Task Lexer_Lexes_Token(LuaSyntaxOptions options, ShortToken expectedToken)
    {
        var token = LexToken(expectedToken.Text, options);

        using (Assert.Multiple())
        {
            token.GetDiagnostics().Verify();
            await Assert.That(token).HasKind(expectedToken.Kind).And.HasText(expectedToken.Text).And
                        .HasSpan(expectedToken.Span);
            if (expectedToken.Value is not { IsSome: true, Value: var expectedValue }) return;
            if (expectedValue is string expectedStr)
            {
                var formattedExpected = ObjectDisplay.FormatLiteral(
                    expectedStr,
                    ObjectDisplayOptions.EscapeNonPrintableCharacters);
                await Assert.That(token.Value).IsTypeOf<string>().And.Satisfies(
                    static txt => ObjectDisplay.FormatLiteral(txt, ObjectDisplayOptions.EscapeNonPrintableCharacters),
                    txt => txt.IsEqualTo(formattedExpected)!);
            }
            await Assert.That(token.Value).IsEqualTo(expectedToken.Value.Value);
        }
    }

    [Test]
    [MethodDataSource(nameof(GetTriviaData))]
    public async Task Lexer_Lexes_Trivia(LuaSyntaxOptions options, ShortToken expectedTrivia)
    {
        var token        = LexToken(expectedTrivia.Text, options: options);
        var actualTrivia = await Assert.That<IEnumerable<SyntaxTrivia>>(token.LeadingTrivia).HasSingleItem();

        using (Assert.Multiple())
        {
            token.GetDiagnostics().Verify();
            await Assert.That(actualTrivia).HasKind(expectedTrivia.Kind).And.HasText(expectedTrivia.Text).And
                        .HasSpan(expectedTrivia.Span);
        }
    }

    [Test]
    [Category("LongTests"), Category("LongTests/100k")]
    [MethodDataSource(nameof(GetTokenPairsData))]
    public async Task Lexer_Lexes_TokenPairs(LuaSyntaxOptions options, ShortToken tokenA, ShortToken tokenB)
    {
        var text   = tokenA.Text + tokenB.Text;
        var tokens = Lex(text, options: options).ToImmutableArray();

        using (Assert.Multiple())
        {
            await Assert.That(tokens).HasCount().EqualTo(3);
            tokens[0].GetDiagnostics().Verify();
            await Assert.That(new ShortToken(tokens[0])).IsEquivalentTo(tokenA);
            tokens[1].GetDiagnostics().Verify();
            await Assert.That(new ShortToken(tokens[1])).IsEquivalentTo(tokenB);
            tokens[2].GetDiagnostics().Verify();
            await Assert.That(tokens[2].Kind()).IsEqualTo(SyntaxKind.EndOfFileToken);
        }
    }

    [Test]
    [Category("LongTests"), Category("LongTests/1M")]
    [MethodDataSource(nameof(GetTokenPairsWithSeparatorsData))]
    public async Task Lexer_Lexes_TokenPairs_WithSeparators(
        LuaSyntaxOptions options,
        ShortToken       tokenA,
        ShortToken       expectedSeparator,
        ShortToken       tokenB)
    {
        var text   = tokenA.Text + expectedSeparator.Text + tokenB.Text;
        var tokens = Lex(text, options: options).ToImmutableArray();

        using (Assert.Multiple())
        {
            await Assert.That(tokens).HasCount().EqualTo(3);

            tokens[0].GetDiagnostics().Verify();
            await Assert.That(new ShortToken(tokens[0])).IsEquivalentTo(tokenA);

            var actualSeparator = await Assert.That<IEnumerable<SyntaxTrivia>>(tokens[0].TrailingTrivia)
                                              .HasSingleItem();
            actualSeparator.GetDiagnostics().Verify();
            await Assert.That(new ShortToken(actualSeparator)).IsEquivalentTo(expectedSeparator);

            tokens[1].GetDiagnostics().Verify();
            await Assert.That(new ShortToken(tokens[1])).IsEquivalentTo(tokenB);

            tokens[2].GetDiagnostics().Verify();
            await Assert.That(tokens[2].Kind()).IsEqualTo(SyntaxKind.EndOfFileToken);
        }
    }

    [SuppressMessage(
        "Usage",
        "TUnit0046:Return a `Func<T>` rather than a `<T>`",
        Justification = "All of them are immutable.")]
    public static IEnumerable<(LuaSyntaxOptions, ShortToken)> GetTokensData()
        =>
#if LARGE_TESTS_DEBUG
            from options in new[] { LuaSyntaxOptions.All }
#else
            from options in LuaSyntaxOptions.AllPresets
#endif
            from token in LexicalTestData.GetTokens(options)
            select (options, token);

    [SuppressMessage(
        "Usage",
        "TUnit0046:Return a `Func<T>` rather than a `<T>`",
        Justification = "All of them are immutable.")]
    public static IEnumerable<(LuaSyntaxOptions, ShortToken)> GetTriviaData()
        =>
#if LARGE_TESTS_DEBUG
            from options in new[] { LuaSyntaxOptions.All }
#else
            from options in LuaSyntaxOptions.AllPresets
#endif
            from trivia in LexicalTestData.GetTrivia(options)
            select (options, trivia);

    [SuppressMessage(
        "Usage",
        "TUnit0046:Return a `Func<T>` rather than a `<T>`",
        Justification = "All of them are immutable.")]
    public static IEnumerable<(LuaSyntaxOptions, ShortToken, ShortToken)> GetTokenPairsData()
        =>
#if LARGE_TESTS_DEBUG
            from options in new[] { LuaSyntaxOptions.All }
#else
            from options in LuaSyntaxOptions.AllPresets
#endif
            from pair in LexicalTestData.GetTokenPairs(options)
            select (options, pair.tokenA, pair.tokenB);

    [SuppressMessage(
        "Usage",
        "TUnit0046:Return a `Func<T>` rather than a `<T>`",
        Justification = "All of them are immutable.")]
    public static IEnumerable<(LuaSyntaxOptions, ShortToken, ShortToken, ShortToken)> GetTokenPairsWithSeparatorsData()
        =>
#if LARGE_TESTS_DEBUG
            from options in new[] { LuaSyntaxOptions.All }
#else
            from options in LuaSyntaxOptions.AllPresets
#endif
            from tuple in LexicalTestData.GetTokenPairsWithSeparators(options)
            select (options, tuple.tokenA, tuple.separator, tuple.tokenB);
}
