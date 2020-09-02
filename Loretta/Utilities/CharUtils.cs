using System;
using System.Runtime.CompilerServices;

namespace Loretta.Utilities
{
    /// <summary>
    /// A general character utility class.
    /// </summary>
    internal class CharUtils
    {
        /// <summary>
        /// Checks whether the provided <paramref name="value" /> is in the range [ <paramref
        /// name="start" />, <paramref name="end" />].
        /// </summary>
        /// <param name="start">The first character of the range (inclusive).</param>
        /// <param name="value">The character to check for.</param>
        /// <param name="end">The last character of the range (inclusive).</param>
        /// <returns>Whether the provided character is in the range.</returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsInRange ( Char start, Char value, Char end ) =>
            ( UInt32 ) ( value - start ) <= ( end - start );

        /// <summary>
        /// Checks whether the provided character is a decimal character (between 0 and 9).
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>Whether the provided character is a decimal character.</returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsDecimal ( Char ch ) =>
            IsInRange ( '0', ch, '9' );

        /// <summary>
        /// Checks whether the provided character is a hexadecimal character.
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>Whether the provided character is hexadecimal.</returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsHexadecimal ( Char ch ) =>
            IsDecimal ( ch )
            // Using the table on /.notes/number-parsing.md, one can see that there's a bit that can
            // be used to convert uppercase characters to lower case (the 6th bit right to left). So
            // we preemptively set it on the char to avoid an extra range check. Validated by the
            // IsAlphaCheckMicrobenchmark microbenchmark.
            || IsInRange ( 'a', ( Char ) ( ch | 0b0100000 ), 'f' );

        /// <summary>
        /// Checks whether the provided character is an alpha character (a-z, A-Z).
        /// </summary>
        /// <param name="ch">The character to check.</param>
        /// <returns>Whether the provided character is an alpha character.</returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsAlpha ( Char ch ) =>
            // Refer to IsHexadecimal(Char) for an explanation of the bitwise or.
            IsInRange ( 'a', ( Char ) ( ch | 0b100000 ), 'z' );

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
        /// <param name="useLuaJitIdentifierRules">
        /// Whether to use LuaJIT's identifier matching rule.
        /// </param>
        /// <param name="ch">The character to check.</param>
        /// <returns>Whether the provided character is a valid first identifier character.</returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsValidFirstIdentifierChar ( Boolean useLuaJitIdentifierRules, Char ch ) =>
            ch == '_' || IsAlpha ( ch ) || ( useLuaJitIdentifierRules && ch >= 0x7F );

        /// <summary>
        /// Checks whether the provided character is a valid trailing identifier character.
        /// </summary>
        /// <param name="useLuaJitIdentifierRules">
        /// Whether to use LuaJIT's identifier matching rule.
        /// </param>
        /// <param name="ch">The character to check.</param>
        /// <returns>Whether the provided character is a valid trailing identifier character.</returns>
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsValidTrailingIdentifierChar ( Boolean useLuaJitIdentifierRules, Char ch ) =>
            IsValidFirstIdentifierChar ( useLuaJitIdentifierRules, ch ) || IsDecimal ( ch );

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