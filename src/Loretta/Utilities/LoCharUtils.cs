using System;
using System.Globalization;
using System.Runtime.CompilerServices;
using GParse.Utilities;

namespace Loretta.Utilities
{
    /// <summary>
    /// A general character utility class.
    /// </summary>
    internal static class LoCharUtils
    {
        /// <summary>
        /// Checks whether the provided character is a decimal character (between 0 and 9).
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>Whether the provided character is a decimal character.</returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsDecimal ( Char ch ) =>
            CharUtils.IsInRange ( '0', ch, '9' );

        /// <summary>
        /// Checks whether the provided character is a hexadecimal character.
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>Whether the provided character is hexadecimal.</returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsHexadecimal ( Char ch ) =>
            IsDecimal ( ch )
            || CharUtils.IsInRange ( 'a', CharUtils.AsciiLowerCase ( ch ), 'f' );

        /// <summary>
        /// Checks whether the provided character is an alpha character (a-z, A-Z).
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>Whether the provided character is an alpha character.</returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsAlpha ( Char ch ) =>
            CharUtils.IsInRange ( 'a', CharUtils.AsciiLowerCase ( ch ), 'z' );

        /// <summary>
        /// Checks whether the provided character is an alphanumeric character (a-z, A-Z, 0-9).
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>Whether the provided character is an alphanumeric character.</returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsAlphaNumeric ( Char ch ) =>
            IsDecimal ( ch ) || IsAlpha ( ch );

        /// <summary>
        /// Checks whether the provided character is a valid first identifier character.
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// 
        /// <returns>Whether the provided character is a valid first identifier character.</returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsValidFirstIdentifierChar ( Char ch ) =>
            ch == '_' || IsAlpha ( ch ) || ch >= 0x7F;

        /// <summary>
        /// Checks whether the provided character is a valid trailing identifier character.
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// 
        /// <returns>Whether the provided character is a valid trailing identifier character.</returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsValidTrailingIdentifierChar ( Char ch ) =>
            IsValidFirstIdentifierChar ( ch ) || IsDecimal ( ch );

        public static String ToReadableString ( Char ch )
        {
            // Some characters will probably pass that shouldn't and some that should won't,
            // but this is easier than having a huge switch with all characters that are
            // "printable" or "readable".
            // We list a few of the the most common characters but the others will pass
            // through the flagset check.
            const UInt32 categoryFlagSet = ( 1U << ( Int32 ) UnicodeCategory.ClosePunctuation )
                                           | ( 1U << ( Int32 ) UnicodeCategory.ConnectorPunctuation )
                                           | ( 1U << ( Int32 ) UnicodeCategory.CurrencySymbol )
                                           | ( 1U << ( Int32 ) UnicodeCategory.DashPunctuation )
                                           | ( 1U << ( Int32 ) UnicodeCategory.DecimalDigitNumber )
                                           | ( 1U << ( Int32 ) UnicodeCategory.FinalQuotePunctuation )
                                           | ( 1U << ( Int32 ) UnicodeCategory.InitialQuotePunctuation )
                                           | ( 1U << ( Int32 ) UnicodeCategory.LetterNumber )
                                           | ( 1U << ( Int32 ) UnicodeCategory.LowercaseLetter )
                                           | ( 1U << ( Int32 ) UnicodeCategory.MathSymbol )
                                           | ( 1U << ( Int32 ) UnicodeCategory.OpenPunctuation )
                                           | ( 1U << ( Int32 ) UnicodeCategory.OtherLetter )
                                           | ( 1U << ( Int32 ) UnicodeCategory.OtherNumber )
                                           | ( 1U << ( Int32 ) UnicodeCategory.OtherPunctuation )
                                           | ( 1U << ( Int32 ) UnicodeCategory.TitlecaseLetter )
                                           | ( 1U << ( Int32 ) UnicodeCategory.UppercaseLetter );

            switch ( ch )
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
                    if ( ch is ' ' || CharUtils.IsCategoryInSet ( categoryFlagSet, Char.GetUnicodeCategory ( ch ) ) )
                        return ch.ToString ( );
                    else
                        return $"\\x{( Int32 ) ch:X}";
            }
        }

        /// <summary>
        /// Encodes the provided character into a hexadecimal escape sequence representing its UTF-8 bytes.
        /// </summary>
        /// <param name="ch">The character to encode.</param>
        /// <returns>The provided character encoded in UTF-8 hexadecimal escape sequences.</returns>
        public static String EncodeCharToUtf8 ( Char ch )
        {
            var n = ( UInt16 ) ch;
            if ( n < 0x7F )
            {
                return $"\\x{n:X2}";
            }
            else if ( n < 0x7FF )
            {
                // 00000yyy yyxxxxxx -> [ 110yyyyy 10xxxxxx ]
                var byte01 = ( Byte ) ( 0b11000000 | ( ( n >> 6 ) & 0b11111 ) );
                var byte02 = ( Byte ) ( 0b10000000 | ( n & 0b111111 ) );
                return $"\\x{byte01:X2}\\x{byte02:X2}";
            }
            else
            {
                // zzzzyyyy yyxxxxxx -> [ 1110zzzz 10yyyyyy 10xxxxxx ]
                var byte01 = ( Byte ) ( 0b11100000 | ( ( n >> 12 ) & 0b1111 ) );
                var byte02 = ( Byte ) ( 0b10000000 | ( ( n >> 6 ) & 0b111111 ) );
                var byte03 = ( Byte ) ( 0b10000000 | ( n & 0b111111 ) );
                return $"\\x{byte01:X2}\\x{byte02:X2}\\x{byte03:X2}";
            }
        }
    }
}