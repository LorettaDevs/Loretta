using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace Loretta.Utilities
{
    internal class CharUtils
    {
        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsDecimal ( Char ch ) =>
            '0' <= ch && ch <= '9';

        [MethodImpl ( MethodImplOptions.AggressiveInlining )]
        public static Boolean IsHexadecimal ( Char ch ) =>
            IsDecimal ( ch ) || ( 'a' <= ch && ch <= 'f' ) || ( 'A' <= ch && ch <= 'F' );

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
