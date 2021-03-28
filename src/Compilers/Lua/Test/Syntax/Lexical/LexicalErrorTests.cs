using System;
using System.Collections.Immutable;
using System.Globalization;
using Loretta.CodeAnalysis.Lua.Utilities;
using Loretta.CodeAnalysis.Text;
using Xunit;

namespace Loretta.CodeAnalysis.Lua.Syntax.UnitTests.Lexical
{
    public class LexicalErrorTests
    {
        private static ImmutableArray<SyntaxToken> ParseTokens(string text, LuaSyntaxOptions? options = null, bool includeEndOfFile = false)
        {
            var parseOptions = new LuaParseOptions(options ?? LuaSyntaxOptions.All);
            var tokens = SyntaxFactory.ParseTokens(text, options: parseOptions).ToImmutableArray();
            if (includeEndOfFile)
                return tokens;
            Assert.True(tokens[^1].Kind() == SyntaxKind.EndOfFileToken, "Last element of the token list was not an EOF token.");
            return tokens.RemoveAt(tokens.Length - 1);
        }

        private static void AssertDiagnostic(DiagnosticInfo diagnostic, ErrorCode code, TextSpan span, params object[] args)
        {
            Assert.Equal(ErrorFacts.GetId(code), diagnostic.MessageIdentifier);
            var message = ErrorFacts.GetMessage(code, CultureInfo.InvariantCulture);
            if (args.Length > 0 || message.IndexOf("{0}", StringComparison.Ordinal) >= 0)
                message = string.Format(message, args);
            Assert.Equal(message, diagnostic.GetMessage());
            Assert.Equal(span, new TextSpan(((SyntaxDiagnosticInfo) diagnostic).Offset, ((SyntaxDiagnosticInfo) diagnostic).Width));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
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

            var diagnostic = Assert.Single(token.Node!.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_InvalidStringEscape, new TextSpan(escapePosition, escapeLength));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
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
            Assert.Equal(text, token.ToFullString());
            Assert.Equal(value, token.Value);

            var diagnostic = Assert.Single(token.Node!.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_UnescapedLineBreakInString, new TextSpan(lineBreakPosition, lineBreakLength));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
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

            var diagnostic = Assert.Single(token.Node!.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_UnfinishedString, new TextSpan(0, text.Length));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
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

            var diagnostic = Assert.Single(token.Node!.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_InvalidNumber, new TextSpan(0, text.Length));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [InlineData("0b10000000000000000000000000000000000000000000000000000000000000000")]
        [InlineData("0o1000000000000000000000")]
        public void Lexer_EmitsDiagnosticsOn_LargeNumbers(string text)
        {
            var tokens = ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind());
            Assert.Equal(text, token.Text);
            Assert.Equal(0d, token.Value);

            var diagnostic = Assert.Single(token.Node!.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_NumericLiteralTooLarge, new TextSpan(0, text.Length));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
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

            var diagnostic = Assert.Single(commentTrivia.UnderlyingNode!.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_UnfinishedLongComment, new TextSpan(0, text.Length));
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
        public void Lexer_EmitsDiagnosticWhen_ShebangIsFound_And_LuaSyntaxOptionsAcceptShebangIsFalse()
        {
            const string shebang = "#!/bin/bash";
            var options = LuaSyntaxOptions.All.With(acceptShebang: false);
            var tokens = ParseTokens(shebang, options: options, includeEndOfFile: true);

            var eof = Assert.Single(tokens);
            var trivia = Assert.Single(eof.LeadingTrivia);
            Assert.Equal(SyntaxKind.ShebangTrivia, trivia.Kind());
            Assert.Equal(shebang, trivia.ToFullString());

            var diagnostic = Assert.Single(trivia.UnderlyingNode!.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_ShebangNotSupportedInLuaVersion, new TextSpan(0, shebang.Length));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticWhen_BinaryNumberIsFound_And_LuaSyntaxOptionsAcceptBinaryNumbersIsFalse()
        {
            const string numberText = "0b1010";

            var options = LuaSyntaxOptions.All.With(acceptBinaryNumbers: false);
            var tokens = ParseTokens(numberText, options: options);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind());
            Assert.Equal(numberText, token.Text);
            Assert.Equal((double) 0b1010, token.Value);

            var diagnostic = Assert.Single(token.Node!.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_BinaryNumericLiteralNotSupportedInVersion, new TextSpan(0, numberText.Length));
        }

        [Fact]
        [Trait("Category", "Lexer/Diagnostics")]
        public void Lexer_EmitsDiagnosticWhen_OctalNumberIsFound_And_LuaSyntaxOptionsAcceptOctalNumbersIsFalse()
        {
            const string numberText = "0o77";

            var options = LuaSyntaxOptions.All.With(acceptOctalNumbers: false);
            var tokens = ParseTokens(numberText, options: options);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.NumericLiteralToken, token.Kind());
            Assert.Equal(numberText, token.Text);
            Assert.Equal(7d * 8d + 7d, token.Value);

            var diagnostic = Assert.Single(token.Node!.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_OctalNumericLiteralNotSupportedInVersion, new TextSpan(0, numberText.Length));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
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

            var diagnostic = Assert.Single(token.Node!.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_HexFloatLiteralNotSupportedInVersion, new TextSpan(0, text.Length));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
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

            var diagnostic = Assert.Single(token.Node!.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_UnderscoreInNumericLiteralNotSupportedInVersion, new TextSpan(0, text.Length));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [InlineData("// hi")]
        [InlineData("/* hi */")]
        public void Lexer_EmitsDiagnosticWhen_CCommentIsFound_And_LuaSyntaxOptionsAcceptCCommentsIsFalse(string text)
        {
            var options = LuaSyntaxOptions.All.With(acceptCCommentSyntax: false);
            var tokens = ParseTokens(text, options: options, includeEndOfFile: true);

            var eof = Assert.Single(tokens);
            var trivia = Assert.Single(eof.LeadingTrivia);
            Assert.Equal(
                text.StartsWith("//", StringComparison.Ordinal)
                ? SyntaxKind.SingleLineCommentTrivia
                : SyntaxKind.MultiLineCommentTrivia,
                trivia.Kind());
            Assert.Equal(text, trivia.ToFullString());

            var diagnostic = Assert.Single(trivia.UnderlyingNode!.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_CCommentsNotSupportedInVersion, new TextSpan(0, text.Length));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [InlineData("🅱")]
        [InlineData("\ufeff"  /* ZERO WIDTH NO-BREAK SPACE */ )]
        [InlineData("\u206b"  /* ACTIVATE SYMMETRIC SWAPPING */ )]
        [InlineData("\u202a"  /* LEFT-TO-RIGHT EMBEDDING */ )]
        [InlineData("\u206a"  /* INHIBIT SYMMETRIC SWAPPING */ )]
        [InlineData("\u200e"  /* LEFT-TO-RIGHT MARK */ )]
        [InlineData("\u200c"  /* ZERO WIDTH NON-JOINER */ )]
        public void Lexer_EmitsDiagnosticWhen_IdentifiersWithCharactersAbove0x7FAreFound_And_LuaSyntaxOptionsUseLuajitIdentifierRulesIsFalse(string text)
        {
            var options = LuaSyntaxOptions.All.With(useLuaJitIdentifierRules: false);
            var tokens = ParseTokens(text, options: options);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.IdentifierToken, token.Kind());
            Assert.Equal(text, token.Text);

            var diagnostic = Assert.Single(token.Node!.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_LuajitIdentifierRulesNotSupportedInVersion, new TextSpan(0, text.Length));
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
        [InlineData("$")]
        [InlineData("\\")]
        [InlineData("?")]
        public void Lexer_EmitsDiagnosticWhen_BadCharactersAreFound(string text)
        {
            var tokens = ParseTokens(text);

            var token = Assert.Single(tokens);
            Assert.Equal(SyntaxKind.BadToken, token.Kind());
            Assert.Equal(text, token.Text);

            var diagnostic = Assert.Single(token.Node!.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_BadCharacter, new TextSpan(0, text.Length), text);
        }

        [Theory]
        [Trait("Category", "Lexer/Diagnostics")]
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

            var diagnostic = Assert.Single(token.Node!.GetDiagnostics());
            AssertDiagnostic(diagnostic, ErrorCode.ERR_HexStringEscapesNotSupportedInVersion, new TextSpan(escapePosition, escapeLength));
        }
    }
}