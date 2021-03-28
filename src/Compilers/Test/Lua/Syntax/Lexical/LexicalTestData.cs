using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
using Loretta.CodeAnalysis.Lua.Utilities;
using Loretta.CodeAnalysis.Text;
using static Tsu.Option;

namespace Loretta.CodeAnalysis.Lua.Syntax.UnitTests.Lexical
{
    internal static class LexicalTestData
    {
        private static double ParseNum(string str, int @base) =>
            @base switch
            {
                2 or 8 => Convert.ToInt64(str[2..].Replace("_", ""), @base),
                10 => double.Parse(str.Replace("_", ""),
                    NumberStyles.AllowDecimalPoint | NumberStyles.AllowExponent,
                    CultureInfo.InvariantCulture),
                16 => HexFloat.DoubleFromHexString(str.Replace("_", "")),
                _ => throw new InvalidOperationException(),
            };

        public static IEnumerable<ShortToken> GetTokens(LuaSyntaxOptions options)
        {
            foreach (var token in from kind in Enum.GetValues(typeof(SyntaxKind)).Cast<SyntaxKind>()
                                  let text = SyntaxFacts.GetText(kind)
                                  where !string.IsNullOrEmpty(text)
                                        && (kind != SyntaxKind.ContinueKeyword
                                            || options.ContinueType == ContinueType.Keyword)
                                  select new ShortToken(kind, text))
            {
                yield return token;
            }

            #region Numbers

            // Binary
            foreach (var text in new[] { "0b10", "0b10_10", "0B10", "0B10_10" })
            {
                yield return new ShortToken(
                    SyntaxKind.NumericLiteralToken,
                    text,
                    Some(ParseNum(text, 2)));
            }

            // Octal
            foreach (var text in new[] { "0o77", "0o77_77", "0O77", "0O77_77" })
            {
                yield return new ShortToken(
                    SyntaxKind.NumericLiteralToken,
                    text,
                    Some(ParseNum(text, 8)));
            }

            // Decimal
            foreach (var text in new[] { "1", "1.1", "1.1e10", ".1", ".1e10", "1_1", "1_1.1_1", "1_1.1_1e1_0", ".1_1", ".1_1e1_0" })
            {
                yield return new ShortToken(
                    SyntaxKind.NumericLiteralToken,
                    text,
                    Some(ParseNum(text, 10)));
            }

            // Hexadecimal
            foreach (var text in new[]
            {
                "0xf",
                "0xfp10",
                "0xf.f",
                "0xf.fp10",
                "0x.f",
                "0x.fp10",
                "0xf_f",
                "0xf_f.f_f",
                "0xf_f.f_fp1_0",
                "0x.f_f",
                "0x.f_fp1_0",
                "0xf_fp1_0"
            })
            {
                yield return new ShortToken(
                    SyntaxKind.NumericLiteralToken,
                    text,
                    Some(ParseNum(text, 16)));
            }

            #endregion Numbers

            #region Strings

            const string shortStringContentText = "hi\\\n\\\r\\\r\n\\a\\b\\f\\n\\r\\t\\v\\\\\\'\\\"\\0\\10\\255\\xF\\xFF";
            const string shortStringContentValue = "hi\n\r\r\n\a\b\f\n\r\t\v\\'\"\0\xA\xFF\xF\xFF";

            // Short strings
            foreach (var quote in new[] { '\'', '"' })
            {
                yield return new ShortToken(
                    SyntaxKind.StringLiteralToken,
                    quote + shortStringContentText + quote,
                    shortStringContentValue);
            }

            const string longStringContent = @"first line \n
second line \r\n
third line \r
fourth line \xFF.";

            // Long Strings
            IEnumerable<string> separators = Enumerable.Range(0, 6)
                                                       .Select(n => new string('=', n))
                                                       .ToImmutableArray();
            foreach (var separator in separators)
            {
                yield return new ShortToken(
                    SyntaxKind.StringLiteralToken,
                    $"[{separator}[{longStringContent}]{separator}]",
                    longStringContent);
            }

            #endregion Strings

            // Identifiers
            foreach (var identifier in new[]
            {
                "a",
                "abc",
                "_",
                "🅱",
                "\ufeff",  /* ZERO WIDTH NO-BREAK SPACE */
                "\u206b",  /* ACTIVATE SYMMETRIC SWAPPING */
                "\u202a",  /* LEFT-TO-RIGHT EMBEDDING */
                "\u206a",  /* INHIBIT SYMMETRIC SWAPPING */
                "\ufeff",  /* ZERO WIDTH NO-BREAK SPACE */
                "\u206a",  /* INHIBIT SYMMETRIC SWAPPING */
                "\u200e",  /* LEFT-TO-RIGHT MARK */
                "\u200c",  /* ZERO WIDTH NON-JOINER */
                "\u200e",  /* LEFT-TO-RIGHT MARK */
            })
            {
                yield return new ShortToken(SyntaxKind.IdentifierToken, identifier);
            }
        }

        public static IEnumerable<ShortToken> GetTrivia()
        {
            return GetSeparators().Concat(new[]
            {
                new ShortToken ( SyntaxKind.SingleLineCommentTrivia, "-- hi" ),
                new ShortToken ( SyntaxKind.SingleLineCommentTrivia, "// hi" ),
                new ShortToken ( SyntaxKind.ShebangTrivia, "#!/bin/bash" ),
            });
        }

        public static IEnumerable<ShortToken> GetSeparators()
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
            };
        }

        public static bool RequiresSeparator(SyntaxKind kindA, string kindAText, SyntaxKind kindB, string kindBText)
        {
            if (kindAText is null)
                throw new ArgumentNullException(nameof(kindAText));
            if (kindBText is null)
                throw new ArgumentNullException(nameof(kindBText));

            var kindAIsKeyword = SyntaxFacts.IsKeyword(kindA);
            var kindBIsKeyowrd = SyntaxFacts.IsKeyword(kindB);

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
            if (kindA is SyntaxKind.OpenBracketToken && kindB == SyntaxKind.StringLiteralToken && kindBText.StartsWith("["))
                return true;
            if (kindA is SyntaxKind.ColonToken && kindB is SyntaxKind.ColonToken or SyntaxKind.ColonColonToken)
                return true;
            if (kindA is SyntaxKind.PlusToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.MinusToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.MinusToken && kindB is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia && kindBText.StartsWith("-"))
                return true;
            if (kindA is SyntaxKind.MinusToken && kindB is SyntaxKind.MinusToken or SyntaxKind.MinusEqualsToken)
                return true;
            if (kindA is SyntaxKind.StarToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.SlashToken && kindB is SyntaxKind.EqualsToken or SyntaxKind.SlashEqualsToken or SyntaxKind.EqualsEqualsToken)
                return true;
            if (kindA is SyntaxKind.SlashToken && kindB is SyntaxKind.SlashToken or SyntaxKind.StarToken or SyntaxKind.StartEqualsToken)
                return true;
            if (kindA is SyntaxKind.SlashToken && kindB is SyntaxKind.SingleLineCommentTrivia or SyntaxKind.MultiLineCommentTrivia && kindBText.StartsWith("/"))
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

        public static IEnumerable<(ShortToken tokenA, ShortToken tokenB)> GetTokenPairs(LuaSyntaxOptions options) =>
            from tokenA in GetTokens(options)
            from tokB in GetTokens(options)
            where !RequiresSeparator(tokenA.Kind, tokenA.Text, tokB.Kind, tokB.Text)
            let tokenB = tokB.WithSpan(new TextSpan(tokenA.Span.End, tokB.Span.Length))
            select (tokenA, tokenB);

        public static IEnumerable<(ShortToken tokenA, ShortToken separator, ShortToken tokenB)> GetTokenPairsWithSeparators(LuaSyntaxOptions options) =>
            from tokenA in GetTokens(options)
            from tokB in GetTokens(options)
            where !RequiresSeparator(tokenA.Kind, tokenA.Text, tokB.Kind, tokB.Text)
            from sep in GetSeparators()
            where !RequiresSeparator(tokenA.Kind, tokenA.Text, sep.Kind, sep.Text) && !RequiresSeparator(sep.Kind, sep.Text, tokB.Kind, tokB.Text)
            let separator = sep.WithSpan(new TextSpan(tokenA.Span.End, sep.Span.Length))
            let tokenB = tokB.WithSpan(new TextSpan(separator.Span.End, tokB.Span.Length))
            select (tokenA, separator, tokenB);
    }
}
