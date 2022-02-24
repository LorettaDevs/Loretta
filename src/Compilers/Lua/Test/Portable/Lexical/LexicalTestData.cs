using System.Globalization;
using Loretta.CodeAnalysis.Lua.Utilities;
using Loretta.CodeAnalysis.Text;
using static Tsu.Option;

namespace Loretta.CodeAnalysis.Lua.UnitTests.Lexical
{
    internal static class LexicalTestData
    {
        private static long ParseLong(string str, int @base) =>
            @base switch
            {
                2 or 8 => Convert.ToInt64(str[2..].Replace("_", ""), @base),
                10 => long.Parse(str.Replace("_", ""), NumberStyles.None, CultureInfo.InvariantCulture),
                16 => long.Parse(str[2..].Replace("_", ""), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture),
                _ => throw new InvalidOperationException()
            };

        private static double ParseDouble(string str, int @base) =>
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
            foreach (var token in from kind in Enum.GetValues(typeof(SyntaxKind))
                                                   .Cast<SyntaxKind>()
                                  where !SyntaxFacts.IsManufacturedToken(kind, options)
                                  let text = SyntaxFacts.GetText(kind)
                                  where !string.IsNullOrEmpty(text)
                                  select new ShortToken(kind, text))
            {
                yield return token;
            }

            #region Numbers

            // Binary
            foreach (var text in new[] { "0b10", "0b10_10", "0B10", "0B10_10" })
            {
                var value = Some<object?>(options.BinaryIntegerFormat == IntegerFormats.Int64
                    ? (object) ParseLong(text, 2)
                    : ParseDouble(text, 2));
                yield return new ShortToken(SyntaxKind.NumericLiteralToken, text, value);
            }

            // Octal
            foreach (var text in new[] { "0o77", "0o77_77", "0O77", "0O77_77" })
            {
                var value = Some<object?>(options.OctalIntegerFormat == IntegerFormats.Int64
                    ? (object) ParseLong(text, 8)
                    : ParseDouble(text, 8));
                yield return new ShortToken(SyntaxKind.NumericLiteralToken, text, value);
            }

            // Decimal
            foreach (var text in new[]
            {
                "1",
                "1e10",
                "1.1",
                "1.1e10",
                ".1",
                ".1e10",
                "1_1",
                "1_1e1_0",
                "1_1.1_1",
                "1_1.1_1e1_0",
                ".1_1",
                ".1_1e1_0"
            })
            {
                object value;
                if (options.DecimalIntegerFormat != IntegerFormats.NotSupported
                    && !text.Contains('.')
                    && !text.Contains('e'))
                {
                    value = options.DecimalIntegerFormat switch
                    {
                        IntegerFormats.Double => (double) ParseLong(text, 10),
                        IntegerFormats.Int64 => (object) ParseLong(text, 10),
                        _ => throw new InvalidOperationException(),
                    };
                }
                else
                {
                    value = ParseDouble(text, 10);
                }

                yield return new ShortToken(
                    SyntaxKind.NumericLiteralToken,
                    text,
                    Some<object?>(value));
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
                object value;
                if (options.HexIntegerFormat != IntegerFormats.NotSupported
                    && !text.Contains('.')
                    && !text.Contains('p'))
                {
                    value = options.HexIntegerFormat switch
                    {
                        IntegerFormats.Double => (double) ParseLong(text, 16),
                        IntegerFormats.Int64 => (object) ParseLong(text, 16),
                        _ => throw new InvalidOperationException(),
                    };
                }
                else
                {
                    value = ParseDouble(text, 16);
                }

                yield return new ShortToken(
                    SyntaxKind.NumericLiteralToken,
                    text,
                    Some<object?>(value));
            }

            #endregion Numbers

            #region Strings

            const string shortStringContentText = "hi\\\n\\\r\\\r\n\\a\\b\\f\\n\\r\\t\\v\\\\\\'\\\"\\0\\10"
                + "\\255\\xF\\xFF\\z    \t\t\r\n\\t\\r\\n\\u{F}\\u{FF}\\u{FFF}\\u{D800}\\u{10FFFF}";
            const string shortStringContentValue = "hi\n\r\r\n\a\b\f\n\r\t\v\\'\"\0\xA"
                + "\xFF\xF\xFF\t\r\n\u000F\u00FF\u0FFF\uD800\U0010FFFF";

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

            yield return new ShortToken(
                SyntaxKind.HashStringLiteralToken,
                $"`{shortStringContentText}`",
                Hash.GetJenkinsOneAtATimeHashCode(shortStringContentValue.AsSpan()));

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

        public static IEnumerable<(ShortToken tokenA, ShortToken tokenB)> GetTokenPairs(LuaSyntaxOptions options) =>
            from tokenA in GetTokens(options)
            from tokB in GetTokens(options)
            where !SyntaxFacts.RequiresSeparator(tokenA.Kind, tokenA.Text, tokB.Kind, tokB.Text)
            let tokenB = tokB.WithSpan(new TextSpan(tokenA.Span.End, tokB.Span.Length))
            select (tokenA, tokenB);

        public static IEnumerable<(ShortToken tokenA, ShortToken separator, ShortToken tokenB)> GetTokenPairsWithSeparators(LuaSyntaxOptions options) =>
            from tokenA in GetTokens(options)
            from tokB in GetTokens(options)
            where !SyntaxFacts.RequiresSeparator(tokenA.Kind, tokenA.Text, tokB.Kind, tokB.Text)
            from sep in GetSeparators()
            where !SyntaxFacts.RequiresSeparator(tokenA.Kind, tokenA.Text, sep.Kind, sep.Text) && !SyntaxFacts.RequiresSeparator(sep.Kind, sep.Text, tokB.Kind, tokB.Text)
            let separator = sep.WithSpan(new TextSpan(tokenA.Span.End, sep.Span.Length))
            let tokenB = tokB.WithSpan(new TextSpan(separator.Span.End, tokB.Span.Length))
            select (tokenA, separator, tokenB);
    }
}
