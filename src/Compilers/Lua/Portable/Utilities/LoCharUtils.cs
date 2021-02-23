using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Globalization;
using System.Linq;
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
        /// 
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

        public static string ToReadableString(char ch)
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

            switch (ch)
            {
                case '\a': return "\\a";
                case '\b': return "\\b";
                case '\f': return "\\f";
                case '\n': return "\\n";
                case '\r': return "\\r";
                case '\t': return "\\t";
                case '\v': return "\\v";
                case ' ': return " ";
                case 'a': return "a";
                case 'b': return "b";
                case 'c': return "c";
                case 'd': return "d";
                case 'e': return "e";
                case 'f': return "f";
                case 'g': return "g";
                case 'h': return "h";
                case 'i': return "i";
                case 'j': return "j";
                case 'k': return "k";
                case 'l': return "l";
                case 'm': return "m";
                case 'n': return "n";
                case 'o': return "o";
                case 'p': return "p";
                case 'q': return "q";
                case 'r': return "r";
                case 's': return "s";
                case 't': return "t";
                case 'u': return "u";
                case 'v': return "v";
                case 'w': return "w";
                case 'x': return "x";
                case 'y': return "y";
                case 'z': return "z";
                case 'A': return "A";
                case 'B': return "B";
                case 'C': return "C";
                case 'D': return "D";
                case 'E': return "E";
                case 'F': return "F";
                case 'G': return "G";
                case 'H': return "H";
                case 'I': return "I";
                case 'J': return "J";
                case 'K': return "K";
                case 'L': return "L";
                case 'M': return "M";
                case 'N': return "N";
                case 'O': return "O";
                case 'P': return "P";
                case 'Q': return "Q";
                case 'R': return "R";
                case 'S': return "S";
                case 'T': return "T";
                case 'U': return "U";
                case 'V': return "V";
                case 'W': return "W";
                case 'X': return "X";
                case 'Y': return "Y";
                case 'Z': return "Z";
                case '0': return "0";
                case '1': return "1";
                case '2': return "2";
                case '3': return "3";
                case '4': return "4";
                case '5': return "5";
                case '6': return "6";
                case '7': return "7";
                case '8': return "8";
                case '9': return "9";
                case '!': return "!";
                case '@': return "@";
                case '#': return "#";
                case '$': return "$";
                case '%': return "%";
                case '&': return "&";
                case '*': return "*";
                case '(': return "(";
                case '[': return "[";
                case '{': return "{";
                case '}': return "}";
                case ']': return "]";
                case ')': return ")";
                case '-': return "-";
                case '_': return "_";
                case '+': return "+";
                case '=': return "=";
                case '~': return "~";
                case '^': return "^";
                case 'ç': return "ç";
                case ',': return ",";
                case '.': return ".";
                case '<': return "<";
                case '>': return ">";
                case ';': return ";";
                case ':': return ":";
                case '/': return "/";
                case '?': return "?";
                case '\\': return "\\";
                case '|': return "|";
                default:
                    if (ch is ' ' || IsCategoryInSet(categoryFlagSet, char.GetUnicodeCategory(ch)))
                        return ch.ToString();
                    else
                        return $"\\u{(int) ch:X4}";
            }
        }

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