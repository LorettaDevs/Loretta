using System.Globalization;
using System.Runtime.CompilerServices;

namespace Loretta.CodeAnalysis.Lua.Utilities
{
    /// <summary>
    /// A general character utility class.
    /// </summary>
    internal static class CharUtils
    {
        /// <summary>
        /// Checks whether the provided <paramref name="value" /> is in the range [<paramref
        /// name="start" />, <paramref name="end" />].
        /// </summary>
        /// <param name="start">The first character of the range (inclusive).</param>
        /// <param name="value">The character to check for.</param>
        /// <param name="end">The last character of the range (inclusive).</param>
        /// <returns>Whether the provided character is in the range.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsInRange(char start, char value, char end) =>
            (uint) (value - start) <= (end - start);

        /// <summary>
        /// Converts the provided ASCII character into lower-case ASCII.
        /// </summary>
        /// <param name="ch">The character to convert.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static char AsciiLowerCase(char ch) =>
            (char) (ch | 0b100000);

        /// <summary>
        /// Checks if the provided character is in the middle of any of the ranges
        /// in the provided (sorted and flattened) list.
        /// </summary>
        /// <param name="idx">The index found by binary search.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool InnerIsInRangesIndexCheck(int idx) =>
            // If the next greatest value's index is odd, then the character is in
            // the middle of a range. Since the length is always even, we don't need
            // to worry about the element not being in the array since it'll return 0
            // or an even number which will not pass the odd check.
            idx >= 0 || (idx & 1) == 0;

        /// <summary>
        /// Checks if the provided character is in the middle of any of the ranges
        /// in the provided SORTED AND FLATTENED range list.
        /// </summary>
        /// <param name="ranges">The sorted and flattened list.</param>
        /// <param name="ch">The character to find.</param>
        /// <returns></returns>
        public static bool IsInRanges(ImmutableArray<char> ranges, char ch) =>
            ranges.Length == 2
            ? IsInRange(ranges[0], ch, ranges[1])
            : InnerIsInRangesIndexCheck(ranges.BinarySearch(ch));

        /// <summary>
        /// Checks if the provided character is in the middle of any of the ranges
        /// in the provided SORTED AND FLATTENED range list.
        /// </summary>
        /// <param name="ranges">The sorted and flattened list.</param>
        /// <param name="ch">The character to find.</param>
        /// <returns></returns>
        public static bool IsInRanges(ReadOnlySpan<char> ranges, char ch) =>
            ranges.Length == 2
            ? IsInRange(ranges[0], ch, ranges[1])
            : InnerIsInRangesIndexCheck(ranges.BinarySearch(ch));

        /// <summary>
        /// Creates a flagset from a list of unicode categories.
        /// </summary>
        /// <param name="unicodeCategories"></param>
        /// <returns></returns>
        public static uint CreateCategoryFlagSet(IEnumerable<UnicodeCategory> unicodeCategories) =>
            unicodeCategories.Aggregate(0U, (flagSet, unicodeCategory) => flagSet | (1U << (int) unicodeCategory));

        /// <summary>
        /// Checks if the provided category is in the flagset.
        /// </summary>
        /// <param name="flagSet"></param>
        /// <param name="unicodeCategory"></param>
        /// <returns></returns>
        public static bool IsCategoryInSet(uint flagSet, UnicodeCategory unicodeCategory) =>
            ((1 << (int) unicodeCategory) & flagSet) != 0;

        /// <summary>
        /// Checks whether the provided character is a decimal character (between 0 and 9).
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>Whether the provided character is a decimal character.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsDecimal(char ch) =>
            IsInRange('0', ch, '9');

        /// <summary>
        /// Checks whether the provided character is a hexadecimal character.
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>Whether the provided character is hexadecimal.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsHexadecimal(char ch) =>
            IsDecimal(ch)
            || IsInRange('a', AsciiLowerCase(ch), 'f');

        /// <summary>
        /// Checks whether the provided character is an alpha character (a-z, A-Z).
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>Whether the provided character is an alpha character.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlpha(char ch) =>
            IsInRange('a', AsciiLowerCase(ch), 'z');

        /// <summary>
        /// Checks whether the provided character is an alphanumeric character (a-z, A-Z, 0-9).
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>Whether the provided character is an alphanumeric character.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsAlphaNumeric(char ch) =>
            IsDecimal(ch) || IsAlpha(ch);

        /// <summary>
        /// Checks whether the provided character is whitespace
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsWhitespace(char ch) =>
            // basically checks [ \t\n\v\f\r]
            // which simplifies to: [ \t-\r]
            // which is what has been implemented here.
            ch == ' ' || IsInRange('\t', ch, '\r');

        /// <summary>
        /// Checks whether the provided character is a valid first identifier character.
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// 
        /// <returns>Whether the provided character is a valid first identifier character.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidFirstIdentifierChar(char ch) =>
            ch == '_' || IsAlpha(ch) || ch >= 0x7F;

        /// <summary>
        /// Checks whether the provided character is a valid trailing identifier character.
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// 
        /// <returns>Whether the provided character is a valid trailing identifier character.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsValidTrailingIdentifierChar(char ch) =>
            IsValidFirstIdentifierChar(ch) || IsDecimal(ch);

        /// <summary>
        /// Encodes the provided character into a hexadecimal escape sequence representing its UTF-8 bytes.
        /// </summary>
        /// <param name="ch">The character to encode.</param>
        /// <returns>The provided character encoded in UTF-8 hexadecimal escape sequences.</returns>
        public static string EncodeCharToUtf8(char ch)
        {
            var n = (ushort) ch;
            if (n < 0x7F)
            {
                return $"\\x{n:X2}";
            }
            else if (n < 0x7FF)
            {
                // 00000yyy yyxxxxxx -> [ 110yyyyy 10xxxxxx ]
                var byte01 = (byte) (0b11000000 | ((n >> 6) & 0b11111));
                var byte02 = (byte) (0b10000000 | (n & 0b111111));
                return $"\\x{byte01:X2}\\x{byte02:X2}";
            }
            else
            {
                // zzzzyyyy yyxxxxxx -> [ 1110zzzz 10yyyyyy 10xxxxxx ]
                var byte01 = (byte) (0b11100000 | ((n >> 12) & 0b1111));
                var byte02 = (byte) (0b10000000 | ((n >> 6) & 0b111111));
                var byte03 = (byte) (0b10000000 | (n & 0b111111));
                return $"\\x{byte01:X2}\\x{byte02:X2}\\x{byte03:X2}";
            }
        }
    }
}
