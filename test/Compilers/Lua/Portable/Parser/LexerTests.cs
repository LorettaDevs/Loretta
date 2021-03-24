#define LARGE_TESTS
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Loretta.CodeAnalysis;
using Loretta.CodeAnalysis.Lua;
using Loretta.CodeAnalysis.Lua.Utilities;
using Loretta.CodeAnalysis.Text;
using Loretta.Utilities;
using Tsu;
using Xunit;

namespace Loretta.Tests.CodeAnalysis.Syntax
{
    public class LexerTests
    {
        private static ImmutableArray<SyntaxToken> ParseTokens(string text, LuaSyntaxOptions? options = null, bool includeEndOfFile = false)
        {
            var parseOptions = new LuaParseOptions(options ?? LuaSyntaxOptions.All);
            var tokens = SyntaxFactory.ParseTokens(text, options: parseOptions);
            if (!includeEndOfFile)
                tokens = tokens.SkipLast(1);
            return tokens.ToImmutableArray();
        }

        private static void AssertDiagnostic(Diagnostic diagnostic, ErrorCode code, TextSpan span, params object[] args)
        {
            Assert.Equal(ErrorFacts.GetId(code), diagnostic.Id);
            var message = ErrorFacts.GetMessage(code, null);
            if (args.Length > 0 || message.Contains("{0}", StringComparison.Ordinal))
                message = string.Format(message, args);
            Assert.Equal(message, diagnostic.GetMessage());
            Assert.Equal(span, diagnostic.Location.SourceSpan);

        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [Trait("Duration", "Short")]
        [InlineData("\"some\\ltext\"", "sometext", 5, 2)]
        [InlineData("'some\\ltext'", "sometext", 5, 2)]
        [InlineData("\"some\\xGtext\"", "someGtext", 5, 2)]
        [InlineData("'some\\xGtext'", "someGtext", 5, 2)]
        [InlineData("\"some\\300text\"", "sometext", 5, 4)]
        [InlineData("'some\\300text'", "sometext", 5, 4)]
        public void Lexer_EmitsDiagnosticsOn_InvalidEscapes(string text, string value, int escapePosition, int escapeLength)
        {
            var tokens = ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind());
            Assert.Equal(text, token.Text);
            Assert.Equal(value, token.Value);

            var diagnostic = Assert.Single(token.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_InvalidStringEscape, new TextSpan(escapePosition, escapeLength));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [Trait("Duration", "Short")]
        [InlineData("\"some\ntext\"", "some\ntext", 5, 1)]
        [InlineData("'some\ntext'", "some\ntext", 5, 1)]
        [InlineData("\"some\rtext\"", "some\rtext", 5, 1)]
        [InlineData("'some\rtext'", "some\rtext", 5, 1)]
        [InlineData("\"some\r\ntext\"", "some\r\ntext", 5, 2)]
        [InlineData("'some\r\ntext'", "some\r\ntext", 5, 2)]
        public void Lexer_EmitsDiagnosticsOn_StringWithLineBreak(string text, string value, int lineBreakPosition, int lineBreakLength)
        {
            var tokens = ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind());
            Assert.Equal(text, token.Text);
            Assert.Equal(value, token.Value);

            var diagnostic = Assert.Single(token.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_UnescapedLineBreakInString, new TextSpan(lineBreakPosition, lineBreakLength));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [Trait("Duration", "Short")]
        [InlineData("\"text", "text")]
        [InlineData("'text", "text")]
        [InlineData("\"text'", "text'")]
        [InlineData("'text\"", "text\"")]
        public void Lexer_EmitsDiagnosticsOn_UnterminatedShortString(string text, string value)
        {
            var tokens = ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind());
            Assert.Equal(text, token.Text);
            Assert.Equal(value, token.Value);

            var diagnostic = Assert.Single(token.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_UnfinishedString, new TextSpan(0, text.Length));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [Trait("Duration", "Short")]
        [InlineData("0b")]
        [InlineData("0b_")]
        [InlineData("0o")]
        [InlineData("0o_")]
        public void Lexer_EmitsDiagnosticsOn_InvalidNumbers(string text)
        {
            var tokens = ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind());
            Assert.Equal(text, token.Text);
            Assert.Equal(0d, token.Value);

            var diagnostic = Assert.Single(token.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_InvalidNumber, new TextSpan(0, text.Length));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [Trait("Duration", "Short")]
        [InlineData("0b10000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData("0o1000000000000000000000")]
        public void Lexer_EmitsDiagnosticsOn_LargeNumbers(string text)
        {
            var tokens = ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind());
            Assert.Equal(text, token.Text);
            Assert.Equal(0d, token.Value);

            var diagnostic = Assert.Single(token.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_NumericLiteralTooLarge, new TextSpan(0, text.Length));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [Trait("Duration", "Short")]
        [InlineData("0b00000000000000000000000000000000000000000000000000000000000000001")]
        [InlineData("0o0000000000000000000001")]
        public void Lexer_DoesNot_CountNumberDigitsNaively(string text)
        {
            var tokens = ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind());
            Assert.Equal(text, token.Text);
            Assert.Equal(1d, token.Value);

            Assert.False(token.ContainsDiagnostics);
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [Trait("Duration", "Short")]
        [InlineData("/* hi")]
        [InlineData("--[[ hi")]
        [InlineData("--[=[ hi")]
        public void Lexer_EmitsDiagnosticOn_UnfinishedLongComment(string text)
        {
            var tokens = ParseTokens(text, includeEndOfFile: true);

            var eof = Assert.Single(tokens);
            var commentTrivia = Assert.Single(eof.LeadingTrivia);
            Assert.Equal(SyntaxKind.MultiLineCommentTrivia, commentTrivia.Kind());
            Assert.Equal(text, commentTrivia.ToFullString());

            var diagnostic = Assert.Single(commentTrivia.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_UnfinishedLongComment, new TextSpan(0, text.Length));
            Assert.False(eof.ContainsDiagnostics);
        }

        [Theory]
        [Trait("Category", "Lexer/Output")]
        [Trait("Duration", "Short")]
        [InlineData("--[")]
        [InlineData("--[=")]
        [InlineData("--[==")]
        [InlineData("--[ [")]
        [InlineData("--[= [")]
        [InlineData("--[= =[")]
        public void Lexer_DoesNot_IdentifyLongCommentsNaively(string text)
        {
            var tokens = ParseTokens(text, includeEndOfFile: true);

            var eof = Assert.Single(tokens);
            var trivia = Assert.Single(eof.LeadingTrivia);
            Assert.Equal(SyntaxKind.SingleLineCommentTrivia, trivia.Kind());
            Assert.Equal(text, trivia.ToFullString());

            Assert.False(trivia.ContainsDiagnostics);
            Assert.False(eof.ContainsDiagnostics);
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        [Trait("Duration", "Short")]
        public void Lexer_EmitsDiagnosticWhen_ShebangIsFound_And_LuaSyntaxOptionsAcceptShebangIsFalse()
        {
            const string shebang = "#!/bin/bash";
            var options = LuaSyntaxOptions.All.With(acceptShebang: false);
            var tokens = ParseTokens(shebang, options: options, includeEndOfFile: true);

            var eof = Assert.Single(tokens);
            var trivia = Assert.Single(eof.LeadingTrivia);
            Assert.Equal(SyntaxKind.ShebangTrivia, trivia.Kind());
            Assert.Equal(shebang, trivia.ToFullString());

            var diagnostic = Assert.Single(trivia.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_ShebangNotSupportedInLuaVersion, new TextSpan(0, shebang.Length));
            Assert.False(eof.ContainsDiagnostics);
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        [Trait("Duration", "Short")]
        public void Lexer_EmitsDiagnosticWhen_BinaryNumberIsFound_And_LuaSyntaxOptionsAcceptBinaryNumbersIsFalse()
        {
            const string numberText = "0b1010";

            var options = LuaSyntaxOptions.All.With(acceptBinaryNumbers: false);
            var tokens = ParseTokens(numberText, options: options);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind());
            Assert.Equal(numberText, token.Text);
            Assert.Equal((double) 0b1010, token.Value);

            var diagnostic = Assert.Single(token.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_BinaryNumericLiteralNotSupportedInVersion, new TextSpan(0, numberText.Length));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        [Trait("Duration", "Short")]
        public void Lexer_EmitsDiagnosticWhen_OctalNumberIsFound_And_LuaSyntaxOptionsAcceptOctalNumbersIsFalse()
        {
            const string numberText = "0o77";

            var options = LuaSyntaxOptions.All.With(acceptOctalNumbers: false);
            var tokens = ParseTokens(numberText, options: options);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind());
            Assert.Equal(numberText, token.Text);
            Assert.Equal(7d * 8d + 7d, token.Value);

            var diagnostic = Assert.Single(token.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_OctalNumericLiteralNotSupportedInVersion, new TextSpan(0, numberText.Length));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [Trait("Duration", "Short")]
        [InlineData("0xff.ff")]
        [InlineData("0xffp10")]
        [InlineData("0xff.ffp10")]
        public void Lexer_EmitsDiagnosticWhen_HexFloatIsFound_And_LuaSyntaxOptionsAcceptHexFloatIsFalse(string text)
        {
            var options = LuaSyntaxOptions.All.With(acceptHexFloatLiterals: false);
            var tokens = ParseTokens(text, options: options);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind());
            Assert.Equal(text, token.Text);
            Assert.Equal(HexFloat.DoubleFromHexString(text), token.Value);

            var diagnostic = Assert.Single(token.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_HexFloatLiteralNotSupportedInVersion, new TextSpan(0, text.Length));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [Trait("Duration", "Short")]
        [InlineData("0b1010_1010", 0b1010_1010)]
        [InlineData("0o7070_7070", 14913080d)]
        [InlineData("10_10.10_10", 10_10.10_10d)]
        [InlineData("0xf_f", 0xF_F)]
        public void Lexer_EmitsDiagnosticWhen_UnderscoreInNumberIsFound_And_LuaSyntaxOptionsAcceptUnderscoresInNumbersIsFalse(string text, double value)
        {
            var options = LuaSyntaxOptions.All.With(acceptUnderscoreInNumberLiterals: false);
            var tokens = ParseTokens(text, options: options);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind());
            Assert.Equal(text, token.Text);
            Assert.Equal(value, token.Value);

            var diagnostic = Assert.Single(token.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion, new TextSpan(0, text.Length));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [Trait("Duration", "Short")]
        [InlineData("// hi")]
        [InlineData("/* hi */")]
        public void Lexer_EmitsDiagnosticWhen_CCommentIsFound_And_LuaSyntaxOptionsAcceptCCommentsIsFalse(string text)
        {
            LuaSyntaxOptions options = LuaSyntaxOptions.All.With(acceptCCommentSyntax: false);
            var tokens = ParseTokens(text, options: options);

            var eof = Assert.Single(tokens);
            var trivia = Assert.Single(eof.LeadingTrivia);
            Assert.Equal(
                text.StartsWith("//", StringComparison.Ordinal)
                ? SyntaxKind.SingleLineCommentTrivia
                : SyntaxKind.MultiLineCommentTrivia,
                trivia.Kind());
            Assert.Equal(text, trivia.ToFullString());

            var diagnostic = Assert.Single(trivia.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_CCommentsNotSupportedInVersion, new TextSpan(0, text.Length));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [Trait("Duration", "Short")]
        [InlineData("🅱")]
        [InlineData("\ufeff"  /* ZERO WIDTH NO-BREAK SPACE */ )]
        [InlineData("\u206b"  /* ACTIVATE SYMMETRIC SWAPPING */ )]
        [InlineData("\u202a"  /* LEFT-TO-RIGHT EMBEDDING */ )]
        [InlineData("\u206a"  /* INHIBIT SYMMETRIC SWAPPING */ )]
        [InlineData("\u200e"  /* LEFT-TO-RIGHT MARK */ )]
        [InlineData("\u200c"  /* ZERO WIDTH NON-JOINER */ )]
        public void Lexer_EmitsDiagnosticWhen_IdentifiersWithCharactersAbove0x7FAreFound_And_LuaSyntaxOptionsUseLuajitIdentifierRulesIsFalse(string text)
        {
            LuaSyntaxOptions options = LuaSyntaxOptions.All.With(useLuaJitIdentifierRules: false);
            var tokens = ParseTokens(text, options: options);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind());
            Assert.Equal(text, token.Text);

            var diagnostic = Assert.Single(token.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_LuajitIdentifierRulesNotSupportedInVersion, new TextSpan(0, text.Length));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [Trait("Duration", "Short")]
        [InlineData("$")]
        [InlineData("\\")]
        [InlineData("?")]
        public void Lexer_EmitsDiagnosticWhen_BadCharactersAreFound(string text)
        {
            var tokens = ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.BadToken, token.Kind());
            Assert.Equal(text, token.Text);

            var diagnostic = Assert.Single(token.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_BadCharacter, new TextSpan(0, text.Length), text);
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [Trait("Duration", "Short")]
        [InlineData("\"hello\\xAthere\"", "hello\xAthere", 6, 3)]
        [InlineData("'hello\\xAthere'", "hello\xAthere", 6, 3)]
        [InlineData("\"hello\\xFFthere\"", "hello\xFFthere", 6, 4)]
        [InlineData("'hello\\xFFthere'", "hello\xFFthere", 6, 4)]
        public void Lexer_EmitsDiagnosticsWhen_HexEscapesAreFound_And_LuaSyntaxOptionsAcceptHexEscapesIsFalse(string text, string value, int escapePosition, int escapeLength)
        {
            var options = LuaSyntaxOptions.All.With(acceptHexEscapesInStrings: false);
            var tokens = ParseTokens(text, options: options);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.StringLiteralToken, token.Kind());
            Assert.Equal(text, token.Text);
            Assert.Equal(value, token.Value);

            var diagnostic = Assert.Single(token.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_HexStringEscapesNotSupportedInVersion, new TextSpan(escapePosition, escapeLength));
        }

        [Fact]
        [Trait("Category", "Lexer/Output")]
        [Trait("Duration", "Short")]
        public void Lexer_Lexes_ShebangsOnlyOnFileStart()
        {
            const string shebang = "#!/bin/bash";
            var tokens = ParseTokens(shebang, includeEndOfFile: true);

            var eof = Assert.Single(tokens);
            var trivia = Assert.Single(eof.LeadingTrivia);
            Assert.Equal(SyntaxKind.ShebangTrivia, trivia.Kind());
            Assert.Equal(shebang, trivia.ToFullString());
            Assert.Equal(new TextSpan(0, shebang.Length), trivia.Span);

            tokens = ParseTokens($"\n{shebang}", includeEndOfFile: true);
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
        [Trait("Duration", "Very Short")]
        public void Lexer_Covers_AllTokens()
        {
            var tokenKinds = Enum.GetValues<SyntaxKind>()
                                 .Where(k => k.IsToken() || k.IsTrivia());

            var testedTokenKinds = GetTokens().Concat(GetTrivia())
                                              .Select(t => t.Kind);

            var untestedTokenKinds = new SortedSet<SyntaxKind>(tokenKinds);
            untestedTokenKinds.Remove(SyntaxKind.BadToken);
            untestedTokenKinds.Remove(SyntaxKind.EndOfFileToken);
            untestedTokenKinds.Remove(SyntaxKind.SkippedTokensTrivia);
            untestedTokenKinds.ExceptWith(testedTokenKinds);

            Assert.Empty(untestedTokenKinds);
        }

#if LARGE_TESTS
        [Theory]
        [Trait("Category", "Lexer/Output")]
        [Trait("Duration", "Medium")]
        [MemberData(nameof(GetTokensData))]
        public void Lexer_Lexes_Token(LuaSyntaxOptions options, ShortToken expectedToken)
        {
            var tokens = ParseTokens(expectedToken.Text, options);

            var token = Assert.Single(tokens);
            Assert.Equal(expectedToken.Kind, token.Kind());
            Assert.Equal(expectedToken.Text, token.Text);
            Assert.Equal(expectedToken.Span, token.Span);
            if (expectedToken.Value.IsSome)
            {
                Assert.Equal(expectedToken.Value.Value, token.Value);
            }
        }
#endif

#if LARGE_TESTS
        [Theory]
        [Trait("Category", "Lexer/Output")]
        [Trait("Duration", "Medium")]
        [MemberData(nameof(GetTriviaData))]
        public void Lexer_Lexes_Trivia(LuaSyntaxOptions options, ShortToken expectedTrivia)
        {
            var tokens = ParseTokens(expectedTrivia.Text, options: options, includeEndOfFile: true);

            var token = Assert.Single(tokens);
            var actualTrivia = Assert.Single(token.LeadingTrivia);
            Assert.Equal(expectedTrivia.Kind, actualTrivia.Kind());
            Assert.Equal(expectedTrivia.Text, actualTrivia.ToFullString());
            Assert.Equal(expectedTrivia.Span, actualTrivia.Span);
        }
#endif

#if LARGE_TESTS
        [Theory]
        [Trait("Category", "Lexer/Output")]
        [Trait("Duration", "Long")]
        [MemberData(nameof(GetTokenPairsData))]
        public void Lexer_Lexes_TokenPairs(LuaSyntaxOptions options, ShortToken tokenA, ShortToken tokenB)
        {
            var text = tokenA.Text + tokenB.Text;
            var tokens = ParseTokens(text, options: options);

            Assert.Equal(2, tokens.Length);
            Assert.Equal(tokenA.Kind, tokens[0].Kind());
            Assert.Equal(tokenA.Text, tokens[0].Text);
            Assert.Equal(tokenA.Span, tokens[0].Span);
            Assert.Equal(tokenB.Kind, tokens[1].Kind());
            Assert.Equal(tokenB.Text, tokens[1].Text);
            Assert.Equal(tokenB.Span, tokens[1].Span);
        }
#endif

#if LARGE_TESTS
        [Theory]
        [Trait("Category", "Lexer/Output")]
        [Trait("Duration", "Very Long")]
        [MemberData(nameof(GetTokenPairsWithSeparatorsData))]
        public void Lexer_Lexes_TokenPairs_WithSeparators(
            LuaSyntaxOptions options,
            ShortToken tokenA,
            ShortToken expectedSeparator,
            ShortToken tokenB)
        {
            var text = tokenA.Text + expectedSeparator.Text + tokenB.Text;
            var tokens = ParseTokens(text, options: options);

            Assert.Equal(2, tokens.Length);
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
        }
#endif

        public static IEnumerable<object[]> GetTokensData() =>
            from token in GetTokens()
            from options in LuaSyntaxOptions.AllPresets
                //let options = LuaOptions.All
            select new object[] { options, token };

        public static IEnumerable<object[]> GetTriviaData() =>
            from trivia in GetTrivia()
            from options in LuaSyntaxOptions.AllPresets
                //let options = LuaOptions.All
            select new object[] { options, trivia };

        public static IEnumerable<object[]> GetTokenPairsData() =>
            from pair in GetTokenPairs()
            from options in LuaSyntaxOptions.AllPresets
                //let options = LuaOptions.LuaJIT
            select new object[] { options, pair.tokenA, pair.tokenB };

        public static IEnumerable<object[]> GetTokenPairsWithSeparatorsData() =>
            from tuple in GetTokenPairsWithSeparators()
            from options in LuaSyntaxOptions.AllPresets
                //let options = LuaOptions.LuaJIT
            select new object[] { options, tuple.tokenA, tuple.separator, tuple.tokenB };

        public static IEnumerable<object[]> GetAllPresetsData() =>
            LuaSyntaxOptions.AllPresets.Select(option => new object[] { option });

        private static IEnumerable<ShortToken> GetTokens()
        {
            const string shortStringContentText = "hi\\\n\\\r\\\r\n\\a\\b\\f\\n\\r\\t\\v\\\\\\'\\\"\\0\\10\\255\\xF\\xFF";
            const string shortStringContentValue = "hi\n\r\r\n\a\b\f\n\r\t\v\\'\"\0\xA\xFF\xF\xFF";
            var fixedTokens = from kind in Enum.GetValues<SyntaxKind>()
                              let text = SyntaxFacts.GetText(kind)
                              where text is not null
                              select new ShortToken(kind, text);

            var dynamicTokens = new List<ShortToken>
            {
#region Numbers

                // Binary
                new ShortToken ( SyntaxKind.NumericLiteralToken, "0b10", Option.Some<double> ( 0b10 ) ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "0b10_10", Option.Some<double> ( 0b1010 ) ),

                // Octal
                new ShortToken ( SyntaxKind.NumericLiteralToken, "0o77", Option.Some<double> ( Convert.ToInt32 ( "77", 8 ) ) ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "0o77_77", Option.Some<double> ( Convert.ToInt32 ( "7777", 8 ) ) ),

                // Decimal
                new ShortToken ( SyntaxKind.NumericLiteralToken, "1", 1d ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "1.1", 1.1d ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "1.1e10", 1.1e10d ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, ".1", .1d ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, ".1e10", .1e10d ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "1_1", 11d ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "1_1.1_1", 11.11d ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "1_1.1_1e1_0", 11.11e10d ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, ".1_1", .11d ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, ".1_1e1_0", .11e10d ),

                // Hexadecimal
                new ShortToken ( SyntaxKind.NumericLiteralToken, "0xf", HexFloat.DoubleFromHexString ( "0xf".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "0xfp10", HexFloat.DoubleFromHexString ( "0xfp10".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "0xf.f", HexFloat.DoubleFromHexString ( "0xf.f".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "0xf.fp10", HexFloat.DoubleFromHexString ( "0xf.fp10".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "0x.f", HexFloat.DoubleFromHexString ( "0x.f".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "0x.fp10", HexFloat.DoubleFromHexString ( "0x.fp10".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "0xf_f", HexFloat.DoubleFromHexString ( "0xf_f".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "0xf_f.f_f", HexFloat.DoubleFromHexString ( "0xf_f.f_f".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "0xf_f.f_fp1_0", HexFloat.DoubleFromHexString ( "0xf_f.f_fp1_0".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "0x.f_f", HexFloat.DoubleFromHexString ( "0x.f_f".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "0x.f_fp1_0", HexFloat.DoubleFromHexString ( "0x.f_fp1_0".Replace ( "_", "" ) ) ),
                new ShortToken ( SyntaxKind.NumericLiteralToken, "0xf_fp1_0", HexFloat.DoubleFromHexString ( "0xf_fp1_0".Replace ( "_", "" ) ) ),

#endregion Numbers

                // Short strings
                new ShortToken ( SyntaxKind.StringLiteralToken, '\'' + shortStringContentText + '\'', shortStringContentValue ),
                new ShortToken ( SyntaxKind.StringLiteralToken, '"' + shortStringContentText + '"', shortStringContentValue ),

                // Identifiers
                new ShortToken ( SyntaxKind.IdentifierToken, "a" ),
                new ShortToken ( SyntaxKind.IdentifierToken, "abc" ),
                new ShortToken ( SyntaxKind.IdentifierToken, "_" ),
                new ShortToken ( SyntaxKind.IdentifierToken, "🅱" ),
                new ShortToken ( SyntaxKind.IdentifierToken, "\ufeff" ),  /* ZERO WIDTH NO-BREAK SPACE */
                new ShortToken ( SyntaxKind.IdentifierToken, "\u206b" ),  /* ACTIVATE SYMMETRIC SWAPPING */
                new ShortToken ( SyntaxKind.IdentifierToken, "\u202a" ),  /* LEFT-TO-RIGHT EMBEDDING */
                new ShortToken ( SyntaxKind.IdentifierToken, "\u206a" ),  /* INHIBIT SYMMETRIC SWAPPING */
                new ShortToken ( SyntaxKind.IdentifierToken, "\ufeff" ),  /* ZERO WIDTH NO-BREAK SPACE */
                new ShortToken ( SyntaxKind.IdentifierToken, "\u206a" ),  /* INHIBIT SYMMETRIC SWAPPING */
                new ShortToken ( SyntaxKind.IdentifierToken, "\u200e" ),  /* LEFT-TO-RIGHT MARK */
                new ShortToken ( SyntaxKind.IdentifierToken, "\u200c" ),  /* ZERO WIDTH NON-JOINER */
                new ShortToken ( SyntaxKind.IdentifierToken, "\u200e" ),  /* LEFT-TO-RIGHT MARK */
            };

            #region Strings

            var longStringContent = @"first line \n
second line \r\n
third line \r
fourth line \xFF.";

            // Long Strings
            IEnumerable<string> separators = Enumerable.Range(0, 6)
                                                       .Select(n => new string('=', n))
                                                       .ToImmutableArray();

            dynamicTokens.AddRange(separators.Select(sep => new ShortToken(SyntaxKind.StringLiteralToken, $"[{sep}[{longStringContent}]{sep}]", longStringContent)));

            #endregion Strings

            return fixedTokens.Concat(dynamicTokens);
        }

        private static IEnumerable<ShortToken> GetTrivia()
        {
            return GetSeparators().Concat(new[]
            {
                new ShortToken ( SyntaxKind.SingleLineCommentTrivia, "-- hi" ),
                new ShortToken ( SyntaxKind.SingleLineCommentTrivia, "// hi" ),
                new ShortToken ( SyntaxKind.ShebangTrivia, "#!/bin/bash" ),
            });
        }

        private static IEnumerable<ShortToken> GetSeparators()
        {
            return new[]
            {
                new ShortToken ( SyntaxKind.WhitespaceTrivia, " " ),
                new ShortToken ( SyntaxKind.WhitespaceTrivia, "  " ),
                new ShortToken ( SyntaxKind.WhitespaceTrivia, "\t" ),
                new ShortToken ( SyntaxKind.EndOfLineTrivia, "\r" ),
                new ShortToken ( SyntaxKind.EndOfLineTrivia, "\n" ),
                new ShortToken ( SyntaxKind.EndOfLineTrivia, "\r\n" ),
                new ShortToken ( SyntaxKind.MultiLineCommentTrivia, "/**/" ),
                new ShortToken ( SyntaxKind.MultiLineCommentTrivia, @"/*
aaa
*/" ),
                new ShortToken ( SyntaxKind.MultiLineCommentTrivia, "--[[]]" ),
                new ShortToken(SyntaxKind.MultiLineCommentTrivia, @"--[[
aaa
]]"),
                new ShortToken ( SyntaxKind.MultiLineCommentTrivia, "--[=[]=]" ),
                new ShortToken ( SyntaxKind.MultiLineCommentTrivia, @"--[=[
aaa
]=]" ),
                new ShortToken ( SyntaxKind.MultiLineCommentTrivia, "--[====[]====]" ),
                new ShortToken ( SyntaxKind.MultiLineCommentTrivia, @"--[====[
aaa
]====]" ),
                // Longs comments can't be used as separators because of the minus token.
            };
        }

        private static bool RequiresSeparator(SyntaxKind kindA, string kindAText, SyntaxKind kindB, string kindBText)
        {
            if (kindAText is null)
                throw new ArgumentNullException(nameof(kindAText));
            if (kindBText is null)
                throw new ArgumentNullException(nameof(kindBText));

            var kindAIsKeyword = kindA.IsKeyword();
            var kindBIsKeyowrd = kindB.IsKeyword();

            if (kindA is SyntaxKind.IdentifierToken && kindB is SyntaxKind.IdentifierToken)
                return true;
            if (kindAIsKeyword && kindBIsKeyowrd)
                return true;
            if (kindAIsKeyword && kindB is SyntaxKind.IdentifierToken)
                return true;
            if (kindA is SyntaxKind.IdentifierToken && kindBIsKeyowrd)
                return true;
            if (kindA is SyntaxKind.IdentifierToken && kindB is SyntaxKind.NumericLiteralToken)
                return true;
            if (kindA is SyntaxKind.NumericLiteralToken && kindB is SyntaxKind.IdentifierToken)
                return true;
            if (kindA is SyntaxKind.NumericLiteralToken && kindBIsKeyowrd)
                return true;
            if (kindA is SyntaxKind.NumericLiteralToken && kindB is SyntaxKind.DotToken or SyntaxKind.DotDotToken or SyntaxKind.DotDotDotToken or SyntaxKind.DotDotEqualsToken)
                return true;
            if (kindAIsKeyword && kindB is SyntaxKind.NumericLiteralToken)
                return true;
            if (kindA is SyntaxKind.NumericLiteralToken && kindB is SyntaxKind.NumericLiteralToken)
                return true;
            if (kindA is SyntaxKind.OpenBracketToken && kindB is SyntaxKind.OpenBracketToken)
                return true;
            if (kindA is SyntaxKind.OpenBracketToken && kindB == SyntaxKind.StringLiteralToken && kindBText.StartsWith('['))
                return true;
            if (kindA is SyntaxKind.ColonToken && kindB is SyntaxKind.ColonToken or SyntaxKind.ColonColonToken)
                return true;
            if (kindA is SyntaxKind.PlusToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.MinusToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.MinusToken && kindB is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia && kindBText.StartsWith('-'))
                return true;
            if (kindA is SyntaxKind.MinusToken && kindB is SyntaxKind.MinusToken or SyntaxKind.MinusEqualsToken)
                return true;
            if (kindA is SyntaxKind.StarToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.SlashToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.SlashEqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.SlashToken && kindB is SyntaxKind.SlashToken or SyntaxKind.StarToken or SyntaxKind.StartEqualsToken)
                return true;
            if (kindA is SyntaxKind.SlashToken && kindB is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia && kindBText.StartsWith('/'))
                return true;
            if (kindA is SyntaxKind.HatToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.PercentToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.DotDotToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.DotToken or SyntaxKind.DotDotToken && kindB is SyntaxKind.DotToken or SyntaxKind.DotDotToken or SyntaxKind.DotDotDotToken or SyntaxKind.DotDotEqualsToken)
                return true;
            if (kindA is SyntaxKind.EqualsToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.BangToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.LessThanToken && kindB is SyntaxKind.LessThanToken or SyntaxKind.LessThanEqualsToken or SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken or SyntaxKind.LessThanLessThanToken)
                return true;
            if (kindA is SyntaxKind.GreaterThanToken && kindB is SyntaxKind.GreaterThanToken or SyntaxKind.GreaterThanEqualsToken or SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken or SyntaxKind.GreaterThanGreaterThanToken)
                return true;
            if (kindA is SyntaxKind.AmpersandToken && kindB is SyntaxKind.AmpersandToken or SyntaxKind.AmpersandAmpersandToken)
                return true;
            if (kindA is SyntaxKind.PipeToken && kindB is SyntaxKind.PipeToken or SyntaxKind.PipePipeToken)
                return true;
            // Dot can be the start of a number
            if (kindA is SyntaxKind.DotToken or SyntaxKind.DotDotToken or SyntaxKind.DotDotDotToken && kindB is SyntaxKind.NumericLiteralToken)
                return true;
            // Shebang
            if (kindA is SyntaxKind.HashToken && kindB is SyntaxKind.BangToken or SyntaxKind.BangEqualsToken)
                return true;

            return false;
        }

        private static IEnumerable<(ShortToken tokenA, ShortToken tokenB)> GetTokenPairs() =>
            from tokenA in GetTokens()
            from tokB in GetTokens()
            where !RequiresSeparator(tokenA.Kind, tokenA.Text, tokB.Kind, tokB.Text)
            let tokenB = tokB.WithSpan(new TextSpan(tokenA.Span.End, tokB.Span.Length))
            select (tokenA, tokenB);

        private static IEnumerable<(ShortToken tokenA, ShortToken separator, ShortToken tokenB)> GetTokenPairsWithSeparators() =>
            from tokenA in GetTokens()
            from tokB in GetTokens()
            where !RequiresSeparator(tokenA.Kind, tokenA.Text, tokB.Kind, tokB.Text)
            from sep in GetSeparators()
            where !RequiresSeparator(tokenA.Kind, tokenA.Text, sep.Kind, sep.Text) && !RequiresSeparator(sep.Kind, sep.Text, tokB.Kind, tokB.Text)
            let separator = sep.WithSpan(new TextSpan(tokenA.Span.End, sep.Span.Length))
            let tokenB = tokB.WithSpan(new TextSpan(separator.Span.End, tokB.Span.Length))
            select (tokenA, separator, tokenB);
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Intended")]
    public readonly struct ShortDiagnostic
    {
        public readonly string Id;
        public readonly string Description;
        public readonly TextSpan Span;

        public ShortDiagnostic(string id, string description, TextSpan span)
        {
            Id = id;
            Description = description;
            Span = span;
        }

        public ShortDiagnostic(Diagnostic diagnostic)
            : this(diagnostic.Id, diagnostic.GetMessage(), diagnostic.Location.SourceSpan)
        {
        }

        public void Deconstruct(out string id, out string description, out TextSpan span)
        {
            id = Id;
            description = Description;
            span = Span;
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "Intended")]
    public readonly struct ShortToken
    {
        public readonly SyntaxKind Kind;
        public readonly string Text;
        public readonly Option<object?> Value;
        public readonly TextSpan Span;

        public ShortToken(SyntaxKind kind, string text, Option<object?> value = default)
        {
            Kind = kind;
            Text = text;
            Value = value;
            Span = new TextSpan(0, text.Length);
        }

        public ShortToken(SyntaxKind kind, string text, TextSpan span, Option<object?> value = default)
        {
            Kind = kind;
            Text = text;
            Value = value;
            Span = span;
        }

        public ShortToken(SyntaxToken token)
            : this(token.Kind(), token.Text, token.Span, token.Value)
        {
        }

        public ShortToken WithSpan(TextSpan span) =>
            new ShortToken(Kind, Text, span, Value);

        public override string ToString() => $"{Kind}<{Text}>";
    }
}