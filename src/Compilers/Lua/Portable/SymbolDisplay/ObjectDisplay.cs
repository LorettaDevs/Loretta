using System.ComponentModel;
using System.Numerics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Loretta.CodeAnalysis.Lua.Utilities;
using Loretta.CodeAnalysis.PooledObjects;

namespace Loretta.CodeAnalysis.Lua.SymbolDisplay
{
    /// <summary>
    /// Displays an object in the Lua style.
    /// </summary>
    public static class ObjectDisplay
    {
        /// <summary>
        /// The nil literal in Lua.
        /// </summary>
        public static string NilLiteral => "nil";

        /// <inheritdoc cref="NilLiteral"/>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("Use NilLiteral instead.", true)]
        public static string NullLiteral => NilLiteral;

        /// <summary>
        /// Returns a string representation of an object of primitive type.
        /// </summary>
        /// <param name="obj">A value to display as a string.</param>
        /// <param name="options">Options used to customize formatting of an object value.</param>
        /// <returns>A string representation of an object of primitive type (or null if the type is not supported).</returns>
        /// <remarks>
        /// Handles <see cref="bool"/>, <see cref="string"/>, <see cref="double"/> and <c>null</c>.
        /// </remarks>
        public static string? FormatPrimitive(object? obj, ObjectDisplayOptions options)
        {
            if (obj is null)
                return NilLiteral;

            var type = obj.GetType();
            if (type.IsEnum)
                type = Enum.GetUnderlyingType(type);

            if (type == typeof(string))
                return FormatLiteral((string) obj, options);

            if (type == typeof(bool))
                return FormatLiteral((bool) obj);

            if (type == typeof(double))
                return FormatLiteral((double) obj, options);

            if (type == typeof(long))
                return FormatLiteral((long) obj, options);

            if (type == typeof(ulong))
                return FormatLiteral((ulong) obj, options);

            if (type == typeof(Complex))
                return FormatLiteral((Complex) obj, options);

            return null;
        }

        /// <summary>
        /// Returns a string representation of a boolean.
        /// </summary>
        /// <param name="value">A value to display as a string.</param>
        /// <returns>A string representation of an object of primitive type.</returns>
        public static string FormatLiteral(bool value) =>
            value ? "true" : "false";

        /// <summary>
        /// Returns true if the character should be replaced and sets
        /// <paramref name="replaceWith"/> to the replacement text.
        /// </summary>
        private static bool TryReplaceChar(char c, [NotNullWhen(true)] out string? replaceWith, bool utf8Encode = false)
        {
            replaceWith = null;
            switch (c)
            {
                case '\\':
                    replaceWith = "\\\\";
                    break;
                case '\0':
                    replaceWith = "\\0";
                    break;
                case '\a':
                    replaceWith = "\\a";
                    break;
                case '\b':
                    replaceWith = "\\b";
                    break;
                case '\f':
                    replaceWith = "\\f";
                    break;
                case '\n':
                    replaceWith = "\\n";
                    break;
                case '\r':
                    replaceWith = "\\r";
                    break;
                case '\t':
                    replaceWith = "\\t";
                    break;
                case '\v':
                    replaceWith = "\\v";
                    break;
            }

            if (replaceWith != null)
            {
                return true;
            }

            if (NeedsEscaping(CharUnicodeInfo.GetUnicodeCategory(c)))
            {
                replaceWith = utf8Encode ? CharUtils.EncodeCharToUtf8(c) : "\\u{" + ((int) c).ToString("x4") + "}";
                return true;
            }

            return false;
        }

        private static bool NeedsEscaping(UnicodeCategory category)
        {
            // Some characters will probably pass that shouldn't and some that should won't,
            // but this is easier than having a huge switch with all characters that are
            // "printable" or "readable".
            // We list a few of the the most common characters but the others will pass
            // through the flagset check.
            const uint categoryFlagSet = (1U << (int) UnicodeCategory.ClosePunctuation)
                                           | (1U << (int) UnicodeCategory.ConnectorPunctuation)
                                           | (1U << (int) UnicodeCategory.CurrencySymbol)
                                           | (1U << (int) UnicodeCategory.DashPunctuation)
                                           | (1U << (int) UnicodeCategory.DecimalDigitNumber)
                                           | (1U << (int) UnicodeCategory.FinalQuotePunctuation)
                                           | (1U << (int) UnicodeCategory.InitialQuotePunctuation)
                                           | (1U << (int) UnicodeCategory.LetterNumber)
                                           | (1U << (int) UnicodeCategory.LowercaseLetter)
                                           | (1U << (int) UnicodeCategory.MathSymbol)
                                           | (1U << (int) UnicodeCategory.OpenPunctuation)
                                           | (1U << (int) UnicodeCategory.OtherLetter)
                                           | (1U << (int) UnicodeCategory.OtherNumber)
                                           | (1U << (int) UnicodeCategory.OtherPunctuation)
                                           | (1U << (int) UnicodeCategory.TitlecaseLetter)
                                           | (1U << (int) UnicodeCategory.UppercaseLetter);
            return !CharUtils.IsCategoryInSet(categoryFlagSet, category);
        }

        /// <summary>
        /// Returns a Lua string literal with the given value.
        /// </summary>
        /// <param name="value">The value that the resulting string literal should have.</param>
        /// <param name="options">Options used to customize formatting of an object value.</param>
        /// <returns>A string literal with the given value.</returns>
        /// <remarks>
        /// Optionally escapes non-printable characters.
        /// </remarks>
        public static string FormatLiteral(string value, ObjectDisplayOptions options)
        {
            if (value is null) throw new ArgumentNullException(nameof(value));
            const char shortStringQuote = '"';
            var pooledBuilder = PooledStringBuilder.GetInstance();
            var builder = pooledBuilder.Builder;

            var useQuotes = options.IncludesOption(ObjectDisplayOptions.UseQuotes);
            var escapeNonPrintable = options.IncludesOption(ObjectDisplayOptions.EscapeNonPrintableCharacters);
            var utf8Escape = options.IncludesOption(ObjectDisplayOptions.EscapeWithUtf8);

            var isVerbatim = useQuotes && !escapeNonPrintable && ContainsNewLine(value);

            string endDelimiter = "";
            if (useQuotes)
            {
                if (isVerbatim)
                {
                    if (!TryFastGetVerbatimEquals(value.AsSpan(), out var startDelimiter, out endDelimiter!))
                        (startDelimiter, endDelimiter) = SlowGetVerbatimEquals(value);
                    builder.Append(startDelimiter);
                }
                else
                {
                    endDelimiter = shortStringQuote.ToString();
                    builder.Append(shortStringQuote);
                }
            }

            for (var idx = 0; idx < value.Length; idx++)
            {
                var ch = value[idx];
                if (escapeNonPrintable && TryReplaceChar(ch, out var replaceWith, utf8Escape))
                {
                    builder.Append(replaceWith);
                }
                else if (useQuotes && !isVerbatim && ch == shortStringQuote)
                {
                    builder.Append('\\');
                    builder.Append(shortStringQuote);
                }
                else
                {
                    builder.Append(ch);
                }
            }

            if (useQuotes)
            {
                builder.Append(endDelimiter);
            }

            return pooledBuilder.ToStringAndFree();
        }

        private static readonly char[] s_newLineChars = { '\r', '\n' };
        private static bool ContainsNewLine(string s) => s.IndexOfAny(s_newLineChars) >= 0;
        private static bool TryFastGetVerbatimEquals(ReadOnlySpan<char> value, [NotNullWhen(true)] out string? startDelimiter, [NotNullWhen(true)] out string? endDelimiter)
        {
            const int bufferSize = 62;
            Span<char> startBuffer = stackalloc char[bufferSize], endBuffer = stackalloc char[bufferSize];
            var idx = 1;

            while (value.Contains(startBuffer[..(idx + 1)], StringComparison.Ordinal) || value.Contains(endBuffer[..(idx + 1)], StringComparison.Ordinal))
            {
                if (idx >= bufferSize - 1)
                {
                    startDelimiter = endDelimiter = null;
                    return false;
                }

                startBuffer[idx] = '='; startBuffer[idx + 1] = '[';
                endBuffer[idx] = '='; endBuffer[idx + 1] = ']';
                idx++;
            }

            startDelimiter = startBuffer.ToString(); endDelimiter = endBuffer.ToString();
            return true;
        }

#pragma warning disable IDE0079 // Remove unnecessary suppression
        [SuppressMessage("Usage", "CA2249:Consider using 'string.Contains' instead of 'string.IndexOf'", Justification = "Not available in all TFMs.")]
#pragma warning restore IDE0079 // Remove unnecessary suppression
        private static (string startDelimiter, string endDelimiter) SlowGetVerbatimEquals(string value)
        {
            var equalsPooledBuilder = PooledStringBuilder.GetInstance();
            var equalsBuilder = equalsPooledBuilder.Builder;
            equalsBuilder.Insert(0, "=", 61); // The fast get verbatim equals can handle up to 60 equals.

            string startDelimiter, endDelimiter;
            while (value.IndexOf(startDelimiter = $"[{equalsBuilder}[", StringComparison.Ordinal) >= 0
                   || value.IndexOf(endDelimiter = $"]{equalsBuilder}]", StringComparison.Ordinal) >= 0)
            {
                equalsBuilder.Append('=');
            }

            return (startDelimiter, endDelimiter);
        }

        /// <summary>
        /// Returns a Lua number literal with the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public static string FormatLiteral(double value, ObjectDisplayOptions options, CultureInfo? cultureInfo = null)
        {
            if (options.IncludesOption(ObjectDisplayOptions.UseHexadecimalNumbers))
            {
                return HexFloat.DoubleToHexString(value);
            }
            else
            {
                return value.ToString("R", GetFormatCulture(cultureInfo));
            }
        }

        /// <summary>
        /// Returns a Lua number literal with the given value.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public static string FormatLiteral(long value, ObjectDisplayOptions options, CultureInfo? cultureInfo = null)
        {
            if (options.IncludesOption(ObjectDisplayOptions.UseHexadecimalNumbers))
            {
                return "0x" + value.ToString("X", GetFormatCulture(cultureInfo));
            }
            else
            {
                return value.ToString("D", GetFormatCulture(cultureInfo));
            }
        }

        /// <summary>
        /// Returns a Lua number literal with the given value and the <c>ULL</c> suffix.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public static string FormatLiteral(ulong value, ObjectDisplayOptions options, CultureInfo? cultureInfo = null)
        {
            if (options.IncludesOption(ObjectDisplayOptions.UseHexadecimalNumbers))
            {
                return $"0x{value.ToString("X", GetFormatCulture(cultureInfo))}ULL";
            }
            else
            {
                return value.ToString("D", GetFormatCulture(cultureInfo)) + "ULL";
            }
        }

        /// <summary>
        /// Returns a Lua number literal with the given value and the <c>i</c> suffix.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="options"></param>
        /// <param name="cultureInfo"></param>
        /// <returns></returns>
        public static string FormatLiteral(Complex value, ObjectDisplayOptions options, CultureInfo? cultureInfo = null) =>
            FormatLiteral(value.Imaginary, options, cultureInfo) + "i";

        private static CultureInfo GetFormatCulture(CultureInfo? cultureInfo) => cultureInfo ?? CultureInfo.InvariantCulture;
    }
}
