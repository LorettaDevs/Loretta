//#define LARGE_TESTS_DEBUG
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Loretta.CodeAnalysis.Text;
using Loretta.Test.Utilities;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.Syntax.UnitTests.Lexical
{
    public class LexicalTests
    {
        private static IEnumerable<SyntaxToken> Lex(string text, LuaSyntaxOptions? options = null) =>
            SyntaxFactory.ParseTokens(text, options: new LuaParseOptions(options ?? LuaSyntaxOptions.All));

        private static SyntaxToken LexToken(string text, LuaSyntaxOptions? options = null)
        {
            var result = default(SyntaxToken);
            foreach (var token in Lex(text, options))
            {
                if (result.Kind() == SyntaxKind.None)
                {
                    result = token;
                }
                else if (token.Kind() == SyntaxKind.EndOfFileToken)
                {
                    continue;
                }
                else
                {
                    Assert.True(false, "More than one token was lexed: " + token);
                }
            }
            if (result.Kind() == SyntaxKind.None)
            {
                Assert.True(false, "No tokens were lexed");
            }
            return result;
        }

        [Fact]
        [Trait("Category", "Lexer/Output")]
        public void Lexer_Lexes_ShebangsOnlyOnFileStart()
        {
            const string shebang = "#!/bin/bash";

            var eof = LexToken(shebang);
            var trivia = Assert.Single(eof.LeadingTrivia);
            Assert.Equal(SyntaxKind.ShebangTrivia, trivia.Kind());
            Assert.Equal(shebang, trivia.ToFullString());
            Assert.Equal(new TextSpan(0, shebang.Length), trivia.Span);

            var tokens = Lex($"\n{shebang}").ToImmutableArray();
            var expectedBrokenTokens = new[]
            {
                new ShortToken ( SyntaxKind.HashToken, "#", new TextSpan ( 1, 1 ) ),
                new ShortToken ( SyntaxKind.BangToken, "!", new TextSpan ( 2, 1 ) ),
                new ShortToken ( SyntaxKind.SlashToken, "/", new TextSpan ( 3, 1 ) ),
                new ShortToken ( SyntaxKind.IdentifierToken, "bin", new TextSpan ( 4, 3 ) ),
                new ShortToken ( SyntaxKind.SlashToken, "/", new TextSpan ( 7, 1 ) ),
                new ShortToken ( SyntaxKind.IdentifierToken, "bash", new TextSpan ( 8, 4 ) ),
                new ShortToken ( SyntaxKind.EndOfFileToken, "", new TextSpan ( 12, 0 ) ),
            };

            Assert.Equal(expectedBrokenTokens.Length, tokens.Length);
            for (var idx = 0; idx < expectedBrokenTokens.Length; idx++)
            {
                var token = tokens[idx];
                var expected = expectedBrokenTokens[idx];

                Assert.Equal(expected.Kind, token.Kind());
                Assert.Equal(expected.Text, token.Text);
                Assert.Equal(expected.Span, token.Span);
            }
        }

        [Fact]
        [Trait("Category", "Lexer/Lexer_Tests")]
        public void Lexer_Covers_AllTokens()
        {
            var tokenKinds = Enum.GetValues(typeof(SyntaxKind))
                                 .Cast<SyntaxKind>()
                                 .Where(k => SyntaxFacts.IsToken(k) || SyntaxFacts.IsTrivia(k));

            var testedTokenKinds = LuaSyntaxOptions.AllPresets.SelectMany(LexicalTestData.GetTokens)
                                                              .Concat(LexicalTestData.GetTrivia())
                                                              .Select(t => t.Kind);

            var untestedTokenKinds = new SortedSet<SyntaxKind>(tokenKinds);
            untestedTokenKinds.Remove(SyntaxKind.BadToken);
            untestedTokenKinds.Remove(SyntaxKind.EndOfFileToken);
            untestedTokenKinds.Remove(SyntaxKind.SkippedTokensTrivia);
            untestedTokenKinds.ExceptWith(testedTokenKinds);

            Assert.Empty(untestedTokenKinds);
        }

        [Theory]
        [Trait("Category", "Lexer/Output")]
        [MemberData(nameof(GetTokensData))]
        public void Lexer_Lexes_Token(LuaSyntaxOptions options, ShortToken expectedToken)
        {
            var token = LexToken(expectedToken.Text, options);
            Assert.Equal(expectedToken.Kind, token.Kind());
            Assert.Equal(expectedToken.Text, token.Text);
            Assert.Equal(expectedToken.Span, token.Span);
            if (expectedToken.Value.IsSome)
                Assert.Equal(expectedToken.Value.Value, token.Value);
        }

        [Theory]
        [Trait("Category", "Lexer/Output")]
        [MemberData(nameof(GetTriviaData))]
        public void Lexer_Lexes_Trivia(LuaSyntaxOptions options, ShortToken expectedTrivia)
        {
            var token = LexToken(expectedTrivia.Text, options: options);
            var actualTrivia = Assert.Single(token.LeadingTrivia);
            Assert.Equal(expectedTrivia.Kind, actualTrivia.Kind());
            Assert.Equal(expectedTrivia.Text, actualTrivia.ToFullString());
            Assert.Equal(expectedTrivia.Span, actualTrivia.Span);
        }

        [ConditionalTheory(typeof(RunLongLexerTests))]
        [Trait("Category", "Lexer/Output")]
        [MemberData(nameof(GetTokenPairsData))]
        public void Lexer_Lexes_TokenPairs(LuaSyntaxOptions options, ShortToken tokenA, ShortToken tokenB)
        {
            var text = tokenA.Text + tokenB.Text;
            var tokens = Lex(text, options: options).ToImmutableArray();

            Assert.Equal(3, tokens.Length);
            Assert.Equal(tokenA.Kind, tokens[0].Kind());
            Assert.Equal(tokenA.Text, tokens[0].Text);
            Assert.Equal(tokenA.Span, tokens[0].Span);
            Assert.Equal(tokenB.Kind, tokens[1].Kind());
            Assert.Equal(tokenB.Text, tokens[1].Text);
            Assert.Equal(tokenB.Span, tokens[1].Span);
            Assert.Equal(SyntaxKind.EndOfFileToken, tokens[2].Kind());
        }

        [ConditionalTheory(typeof(RunLongLexerTests))]
        [Trait("Category", "Lexer/Output")]
        [MemberData(nameof(GetTokenPairsWithSeparatorsData))]
        public void Lexer_Lexes_TokenPairs_WithSeparators(
            LuaSyntaxOptions options,
            ShortToken tokenA,
            ShortToken expectedSeparator,
            ShortToken tokenB)
        {
            var text = tokenA.Text + expectedSeparator.Text + tokenB.Text;
            var tokens = Lex(text, options: options).ToImmutableArray();

            Assert.Equal(3, tokens.Length);
            Assert.Equal(tokenA.Kind, tokens[0].Kind());
            Assert.Equal(tokenA.Text, tokens[0].Text);
            Assert.Equal(tokenA.Span, tokens[0].Span);

            var actualSeparator = Assert.Single(tokens[0].TrailingTrivia);
            Assert.Equal(expectedSeparator.Kind, actualSeparator.Kind());
            Assert.Equal(expectedSeparator.Text, actualSeparator.ToFullString());
            Assert.Equal(expectedSeparator.Span, actualSeparator.Span);

            Assert.Equal(tokenB.Kind, tokens[1].Kind());
            Assert.Equal(tokenB.Text, tokens[1].Text);
            Assert.Equal(tokenB.Span, tokens[1].Span);

            Assert.Equal(SyntaxKind.EndOfFileToken, tokens[2].Kind());
        }

        public static IEnumerable<object[]> GetTokensData() =>
#if LARGE_TESTS_DEBUG
            from options in new[] { LuaSyntaxOptions.All }
#else
            from options in LuaSyntaxOptions.AllPresets
#endif
            from token in LexicalTestData.GetTokens(options)
            select new object[] { options, token };

        public static IEnumerable<object[]> GetTriviaData() =>
#if LARGE_TESTS_DEBUG
            from options in new[] { LuaSyntaxOptions.All }
#else
            from options in LuaSyntaxOptions.AllPresets
#endif
            from trivia in LexicalTestData.GetTrivia()
            select new object[] { options, trivia };

        public static IEnumerable<object[]> GetTokenPairsData() =>
#if LARGE_TESTS_DEBUG
            from options in new[] { LuaSyntaxOptions.All }
#else
            from options in LuaSyntaxOptions.AllPresets
#endif
            from pair in LexicalTestData.GetTokenPairs(options)
            select new object[] { options, pair.tokenA, pair.tokenB };

        public static IEnumerable<object[]> GetTokenPairsWithSeparatorsData() =>
#if LARGE_TESTS_DEBUG
            from options in new[] { LuaSyntaxOptions.All }
#else
            from options in LuaSyntaxOptions.AllPresets
#endif
            from tuple in LexicalTestData.GetTokenPairsWithSeparators(options)
            select new object[] { options, tuple.tokenA, tuple.separator, tuple.tokenB };
    }
}
