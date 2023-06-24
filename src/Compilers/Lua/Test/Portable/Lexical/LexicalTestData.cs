using System.Globalization;
using System.Numerics;
using Loretta.CodeAnalysis.Lua.Syntax;
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
                                    && (text != "//" || options.AcceptFloorDivision)
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

            // LuaJIT

            // Normal
            foreach (var text in new[]
            {
                "10ULL",
                "20ULL",
                "200005ULL",
                "18446744073709551615ULL",
                "10uLL",
                "20uLL",
                "200005uLL",
                "18446744073709551615uLL"
            })
            {
                yield return new ShortToken(
                    SyntaxKind.NumericLiteralToken,
                    text,
                    ulong.Parse(text[..^3]));
            }

            foreach (var text in new[]
{
                "10LL",
                "20LL",
                "200005LL",
                "9223372036854775807LL",
                "10lL",
                "20lL",
                "200005lL",
                "9223372036854775807lL"
            })
            {
                yield return new ShortToken(
                    SyntaxKind.NumericLiteralToken,
                    text,
                    long.Parse(text[..^2]));
            }

            // Binary & Hexadecimal
            foreach (var text in new[]
            {
                "0b0001LL",
                "0b000111LL",
                "0b0111111111111111111111111111111111111111111111111111111111111111LL",
                "0b0001lL",
                "0b000111lL",
                "0b0111111111111111111111111111111111111111111111111111111111111111lL"
            })
            {
                yield return new ShortToken(
                    SyntaxKind.NumericLiteralToken,
                    text,
                    Convert.ToInt64(text[2..^2], 2));
            }

            foreach (var text in new[]
            {
                "0x11000013d077020LL",
                "0x7FFFFFFFFFFFFFFFLL",
                "0x11000013d077020lL",
                "0x7FFFFFFFFFFFFFFFlL"
            })
            {
                yield return new ShortToken(
                    SyntaxKind.NumericLiteralToken,
                    text,
                    long.Parse(text[2..^2], NumberStyles.HexNumber));
            }

            foreach (var text in new[]
            {
                "0b0001ULL",
                "0b000111ULL",
                "0b1111111111111111111111111111111111111111111111111111111111111111ULL",
                "0b0001uLl",
                "0b000111uLl",
                "0b1111111111111111111111111111111111111111111111111111111111111111uLl"
            })
            {
                yield return new ShortToken(
                    SyntaxKind.NumericLiteralToken,
                    text,
                    Convert.ToUInt64(text[2..^3], 2));
            }

            foreach (var text in new[]
            {
                "0x11000013d077020ULL",
                "0xFFFFFFFFFFFFFFFFULL",
                "0x11000013d077020uLl",
                "0xFFFFFFFFFFFFFFFFuLl"
            })
            {
                yield return new ShortToken(
                    SyntaxKind.NumericLiteralToken,
                    text,
                    ulong.Parse(text[2..^3], NumberStyles.HexNumber));
            }

            foreach (var text in new[]
            {
                "0x11i",
                "0x1020i",
                "0x11I",
                "0x1020I"
            })
            {
                yield return new ShortToken(
                    SyntaxKind.NumericLiteralToken,
                    text,
                    new Complex(0, ParseDouble(text[..^1], 16)));
            }

            foreach (var text in new[]
{
                "0b0001i",
                "0b111111i"
            })
            {
                yield return new ShortToken(
                    SyntaxKind.NumericLiteralToken,
                    text,
                    new Complex(0, ParseDouble(text[..^1], 2)));
            }

            foreach (var text in new[]
            {
                "100i",
                "999999999999999i",
                "100I",
                "999999999999999I"
            })
            {
                yield return new ShortToken(
                    SyntaxKind.NumericLiteralToken,
                    text,
                    new Complex(0, ParseDouble(text[..^1], 10)));
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

            var shortStringContentText = "hi\\n\\r\\b\\f\\n\\v\\u{D800}\\u{10FFFF}\\xF\\xFF\\z ";
            var shortStringContentValue = "hi\n\r\b\f\n\vu{D800}u{10FFFF}xFxFFz ";

            if (options.AcceptHexEscapesInStrings || !options.AcceptInvalidEscapes)
            {
                shortStringContentValue = shortStringContentValue.Replace("xFxFF", "\xF\xFF");
            }

            if (options.AcceptUnicodeEscape || !options.AcceptInvalidEscapes)
            {
                shortStringContentValue = shortStringContentValue.Replace("u{D800}u{10FFFF}", "\uD800\U0010FFFF");
            }

            if (options.AcceptWhitespaceEscape || !options.AcceptInvalidEscapes)
            {
                shortStringContentValue = shortStringContentValue.Replace("z ", "");
            }

            if (options.AcceptInvalidEscapes)
            {
                shortStringContentText += "\\l";
                shortStringContentValue += "l";
            }

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
                Hash.GetJenkinsOneAtATimeHashCode(shortStringContentValue.ToLowerInvariant().AsSpan()));

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

        public static IEnumerable<ShortToken> GetTrivia(LuaSyntaxOptions options)
        {
            foreach (var trivia in GetSeparators(options))
                yield return trivia;
            yield return new ShortToken(SyntaxKind.SingleLineCommentTrivia, "-- hi");
            if (options.AcceptCCommentSyntax)
                yield return new ShortToken(SyntaxKind.SingleLineCommentTrivia, "// hi");
            yield return new ShortToken(SyntaxKind.ShebangTrivia, "#!/bin/bash");
        }

        public static IEnumerable<ShortToken> GetSeparators(LuaSyntaxOptions options)
        {
            foreach (var ws in new[] { " ", "  ", "\t" })
                yield return new ShortToken(SyntaxKind.WhitespaceTrivia, ws);
            foreach (var eol in new[] { "\r", "\n", "\r\n" })
                yield return new ShortToken(SyntaxKind.EndOfLineTrivia, eol);
            if (options.AcceptCCommentSyntax)
            {
                foreach (var comment in new[] { "/**/", "/*\naaa\n*/" })
                    yield return new ShortToken(SyntaxKind.MultiLineCommentTrivia, comment);
            }
            foreach (var comment in new[] { "--[[]]", "--[[\naaa\n]]", "--[=[]=]", "--[=[\naaa\n]=]", "--[====[]====]", "--[====[\naaa\n]====]" })
                yield return new ShortToken(SyntaxKind.MultiLineCommentTrivia, comment);
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
            from sep in GetSeparators(options)
            where !SyntaxFacts.RequiresSeparator(tokenA.Kind, tokenA.Text, sep.Kind, sep.Text) && !SyntaxFacts.RequiresSeparator(sep.Kind, sep.Text, tokB.Kind, tokB.Text)
            let separator = sep.WithSpan(new TextSpan(tokenA.Span.End, sep.Span.Length))
            let tokenB = tokB.WithSpan(new TextSpan(separator.Span.End, tokB.Span.Length))
            select (tokenA, separator, tokenB);
    }
}
